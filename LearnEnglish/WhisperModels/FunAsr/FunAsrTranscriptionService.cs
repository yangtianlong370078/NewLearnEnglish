using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NAudio.Wave;

namespace LearnEnglish.WhisperModels.FunAsr
{
    /// <summary>
    /// 基于 FunASR-CTC-Nano (Fun-ASR-Nano) INT8 ONNX 模型的本地语音转文字服务。
    /// 推理管线：16k 单声道 PCM -> FBANK(80) -> LFR(m=7,n=6,560维) -> encoder -> ctc -> CTC 贪心解码 -> 文本。
    /// </summary>
    public interface IFunAsrTranscriptionService
    {
        Task<string> TranscribeAsync(string wavFilePath);
    }

    public sealed class FunAsrTranscriptionService : IFunAsrTranscriptionService, IDisposable
    {
        private const int BlankId = 60514; // tokens.txt 中 <blk> 的 id（最后一个）

        private readonly InferenceSession _encoder;
        private readonly InferenceSession _ctc;
        private readonly Dictionary<int, byte[]> _tokens;
        private readonly FunAsrFbank _fbank = new();

        public FunAsrTranscriptionService(string modelDir)
        {
            var encoderPath = Path.Combine(modelDir, "encoder.int8.onnx");
            var ctcPath = Path.Combine(modelDir, "ctc.int8.onnx");
            var tokensPath = Path.Combine(modelDir, "tokens.txt");

            if (!File.Exists(encoderPath) || !File.Exists(ctcPath) || !File.Exists(tokensPath))
            {
                throw new FileNotFoundException(
                    $"FunASR 模型文件缺失，请检查目录: {modelDir}（需要 encoder.int8.onnx / ctc.int8.onnx / tokens.txt）");
            }

            var options = new Microsoft.ML.OnnxRuntime.SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            };

            _encoder = new InferenceSession(encoderPath, options);
            _ctc = new InferenceSession(ctcPath, options);
            _tokens = LoadTokens(tokensPath);
        }

        public Task<string> TranscribeAsync(string wavFilePath)
            => Task.Run(() => Transcribe(wavFilePath));

        private string Transcribe(string wavFilePath)
        {
            // 1. 读取 16k 单声道 16bit PCM，按 Kaldi 约定使用原始 int16 幅值
            float[] samples = ReadWavInt16AsFloat(wavFilePath);
            if (samples.Length == 0)
            {
                return string.Empty;
            }

            // 2. FBANK(80) + LFR(7,6) -> 560 维特征
            var fbank = _fbank.Compute(samples);
            if (fbank.Count == 0)
            {
                return string.Empty;
            }
            var lfr = ApplyLfr(fbank, 7, 6);
            int t = lfr.Count, d = lfr[0].Length;

            var feat = new DenseTensor<float>(new[] { 1, t, d });
            for (int i = 0; i < t; i++)
            {
                for (int j = 0; j < d; j++)
                {
                    feat[0, i, j] = lfr[i][j];
                }
            }
            var mask = new DenseTensor<float>(new[] { 1, t });
            for (int i = 0; i < t; i++)
            {
                mask[0, i] = 1f;
            }

            // 3. encoder 推理
            using var encResult = _encoder.Run(new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("lfr_feat", feat),
                NamedOnnxValue.CreateFromTensor("mask", mask),
            });
            var encOut = encResult.First(x => x.Name == "enc_output").AsTensor<float>();
            int encT = encOut.Dimensions[1], encD = encOut.Dimensions[2];
            var encDense = new DenseTensor<float>(new[] { 1, encT, encD });
            for (int i = 0; i < encT; i++)
            {
                for (int j = 0; j < encD; j++)
                {
                    encDense[0, i, j] = encOut[0, i, j];
                }
            }

            // 4. ctc 推理（输出每帧 top-30 的索引，取 top-1 做贪心解码）
            using var ctcResult = _ctc.Run(new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("enc_output", encDense),
            });
            var indices = ctcResult.First(x => x.Name == "topk_indices").AsTensor<int>();
            int frames = indices.Dimensions[1];

            // 5. CTC 贪心解码：去重 + 去 blank
            var ids = new List<int>();
            int prev = -1;
            for (int i = 0; i < frames; i++)
            {
                int id = indices[0, i, 0];
                if (id != BlankId && id != prev)
                {
                    ids.Add(id);
                }
                prev = id;
            }

            // 6. token(base64 字节片) -> UTF-8 文本
            var bytes = new List<byte>();
            foreach (var id in ids)
            {
                if (_tokens.TryGetValue(id, out var b))
                {
                    bytes.AddRange(b);
                }
            }
            return Encoding.UTF8.GetString(bytes.ToArray()).Trim();
        }

        private static Dictionary<int, byte[]> LoadTokens(string path)
        {
            var dict = new Dictionary<int, byte[]>();
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                int sp = line.LastIndexOf(' ');
                if (sp < 0)
                {
                    continue;
                }
                string tok = line.Substring(0, sp);
                if (!int.TryParse(line.AsSpan(sp + 1), out int id))
                {
                    continue;
                }
                try
                {
                    dict[id] = Convert.FromBase64String(tok);
                }
                catch (FormatException)
                {
                    dict[id] = Encoding.UTF8.GetBytes(tok);
                }
            }
            return dict;
        }

        /// <summary>
        /// LFR（Low Frame Rate）：左侧 padding (m-1)/2 帧，按步长 n 堆叠 m 帧。
        /// </summary>
        private static List<float[]> ApplyLfr(List<float[]> inputs, int m, int n)
        {
            int frameCount = inputs.Count;
            int dim = inputs[0].Length;
            int leftPad = (m - 1) / 2;

            var padded = new List<float[]>(frameCount + leftPad);
            for (int i = 0; i < leftPad; i++)
            {
                padded.Add(inputs[0]);
            }
            padded.AddRange(inputs);
            int paddedCount = padded.Count;

            int outCount = (int)Math.Ceiling(frameCount / (double)n);
            var output = new List<float[]>(outCount);
            for (int i = 0; i < outCount; i++)
            {
                var f = new float[m * dim];
                for (int k = 0; k < m; k++)
                {
                    int srcIdx = i * n + k;
                    float[] src = srcIdx < paddedCount ? padded[srcIdx] : padded[paddedCount - 1];
                    Array.Copy(src, 0, f, k * dim, dim);
                }
                output.Add(f);
            }
            return output;
        }

        private static float[] ReadWavInt16AsFloat(string path)
        {
            using var reader = new WaveFileReader(path);
            int bytesPerSample = reader.WaveFormat.BitsPerSample / 8;
            int channels = reader.WaveFormat.Channels;
            var buffer = new byte[reader.Length];
            int read = reader.Read(buffer, 0, buffer.Length);

            int totalSamples = read / bytesPerSample;
            int frameCount = totalSamples / channels;
            var samples = new float[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                // 仅取第一个声道（输入已是单声道，这里做兜底）
                int offset = i * channels * bytesPerSample;
                samples[i] = BitConverter.ToInt16(buffer, offset);
            }
            return samples;
        }

        public void Dispose()
        {
            _encoder?.Dispose();
            _ctc?.Dispose();
        }
    }
}
