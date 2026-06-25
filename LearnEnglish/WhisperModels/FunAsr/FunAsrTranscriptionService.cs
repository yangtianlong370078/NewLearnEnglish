using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NAudio.Wave;

namespace LearnEnglish.WhisperModels.FunAsr
{
    /// <summary>
    /// 基于 FunASR-CTC-Nano (Fun-ASR-Nano) INT8 ONNX 模型的本地语音转文字服务。
    /// 推理管线：16k 单声道 PCM -> FBANK(80) -> LFR(m=7,n=6,560维) -> encoder -> ctc -> CTC 贪心解码 -> 文本。
    /// 识别结果仅保留英文（过滤掉中文等非英文字符），只输出英文文本。
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

        // CTC 解码参数：
        // _blankPenalty：从每帧 blank 的 log 概率中扣除的惩罚值。该 CTC 头存在强烈的 blank 偏置，
        //   导致逐帧贪心(argmax)会把读音较弱的音节(如 configuration 中的 -uration)整段丢弃，
        //   只输出半个单词。对 blank 施加惩罚可把这些被淹没的音节召回。约 1.0 为较优经验值。
        // _beamSize：CTC 前缀 beam search 的束宽，用于正确折叠重复并配合 blank 惩罚得到完整词。
        private readonly float _blankPenalty;
        private readonly int _beamSize;

        public FunAsrTranscriptionService(string modelDir, float blankPenalty = 1.0f, int beamSize = 10)
        {
            var encoderPath = Path.Combine(modelDir, "encoder.int8.onnx");
            var ctcPath = Path.Combine(modelDir, "ctc.int8.onnx");
            var tokensPath = Path.Combine(modelDir, "tokens.txt");

            if (!File.Exists(encoderPath) || !File.Exists(ctcPath) || !File.Exists(tokensPath))
            {
                throw new FileNotFoundException(
                    $"FunASR 模型文件缺失，请检查目录: {modelDir}（需要 encoder.int8.onnx / ctc.int8.onnx / tokens.txt）");
            }

            _blankPenalty = blankPenalty < 0f ? 0f : blankPenalty;
            _beamSize = beamSize < 1 ? 1 : beamSize;

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

            // 4. ctc 推理（输出每帧 top-30 的 log 概率与索引）
            using var ctcResult = _ctc.Run(new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("enc_output", encDense),
            });
            var logProbs = ctcResult.First(x => x.Name == "topk_log_probs").AsTensor<float>();
            var indices = ctcResult.First(x => x.Name == "topk_indices").AsTensor<int>();
            int frames = indices.Dimensions[1];
            int topK = indices.Dimensions[2];

            // 5. CTC 前缀 beam search 解码（对 blank 施加惩罚以召回被淹没的弱读音节）
            var ids = BeamSearchDecode(logProbs, indices, frames, topK);

            // 5b. 折叠相邻重复的 token：缓慢/拖长发音时，同一音节会被切成两段分离的相同 token
            //     （如 configuration 读作 con-fig…fig 时输出 "figfig"），折叠后得到 "fig"，
            //     既消除重复又仍是目标词的子串，可被发音容错匹配正确命中。
            ids = CollapseRepeatedTokens(ids);

            // 6. token(base64 字节片) -> UTF-8 文本
            var bytes = new List<byte>();
            foreach (var id in ids)
            {
                if (_tokens.TryGetValue(id, out var b))
                {
                    bytes.AddRange(b);
                }
            }
            var text = Encoding.UTF8.GetString(bytes.ToArray());

            // 7. 仅保留英文：过滤掉中文等非英文字符，只输出英文文本
            text = KeepEnglishOnly(text);

            // 8. 合并相邻的重复/前缀词：拖长某个音节时，模型会把它当成一个独立的词再输出一次
            //    （如把第一个音拖长，configuration 被识别成 "con configuration"）。
            //    单词练习场景下输出本应只有一个词，因此把相邻且互为前缀/后缀的词合并为更长的那个。
            return MergeAdjacentWordArtifacts(text);
        }

        /// <summary>
        /// 合并相邻的重复或前缀/后缀包含的词，保留更长的一个。
        /// 用于消除“拖长发音导致首/尾音节被当成独立词重复输出”的情况，如 "con configuration" -> "configuration"。
        /// 仅适用于单词识别场景（输出本应为单个词）。
        /// </summary>
        private static string MergeAdjacentWordArtifacts(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                return text;
            }

            var result = new List<string>(words.Length);
            foreach (var w in words)
            {
                if (result.Count > 0)
                {
                    var prev = result[result.Count - 1];
                    // 相邻词互为前缀或后缀（含完全相同）：视为同一词的拖音重复，保留更长的
                    if (prev == w ||
                        w.StartsWith(prev, StringComparison.OrdinalIgnoreCase) ||
                        prev.StartsWith(w, StringComparison.OrdinalIgnoreCase) ||
                        w.EndsWith(prev, StringComparison.OrdinalIgnoreCase) ||
                        prev.EndsWith(w, StringComparison.OrdinalIgnoreCase))
                    {
                        if (w.Length > prev.Length)
                        {
                            result[result.Count - 1] = w;
                        }
                        continue;
                    }
                }
                result.Add(w);
            }
            return string.Join(" ", result);
        }

        /// <summary>
        /// 折叠相邻重复的 token id：把连续出现的相同 token 合并为一个。
        /// 解决拖长/缓慢发音时同一音节被切成多段、beam 输出重复 token（如 "figfig"）的问题。
        /// </summary>
        private static List<int> CollapseRepeatedTokens(List<int> ids)
        {
            var result = new List<int>(ids.Count);
            foreach (var id in ids)
            {
                if (result.Count == 0 || result[result.Count - 1] != id)
                {
                    result.Add(id);
                }
            }
            return result;
        }

        /// <summary>
        /// CTC 前缀 beam search 解码。
        /// 每帧使用模型导出的 top-K（log 概率 + 索引），并对 blank 的 log 概率扣除 _blankPenalty，
        /// 以抵消该 CTC 头的强 blank 偏置（否则逐帧贪心会丢弃弱读音节，只得到半个单词）。
        /// beam search 负责正确折叠 CTC 的重复 token。
        /// </summary>
        private List<int> BeamSearchDecode(Tensor<float> logProbs, Tensor<int> indices, int frames, int topK)
        {
            const float NegInf = float.NegativeInfinity;

            // 前缀 -> (pBlank, pNonBlank)，均为 log 概率。空前缀以 pBlank=0 起步。
            var beams = new Dictionary<string, BeamEntry>
            {
                [string.Empty] = new BeamEntry(new List<int>(), 0f, NegInf),
            };

            for (int t = 0; t < frames; t++)
            {
                var next = new Dictionary<string, BeamEntry>();

                foreach (var beam in beams.Values)
                {
                    float pb = beam.PBlank;
                    float pnb = beam.PNonBlank;
                    float pTot = LogSumExp(pb, pnb);
                    int last = beam.Tokens.Count > 0 ? beam.Tokens[beam.Tokens.Count - 1] : -1;

                    for (int k = 0; k < topK; k++)
                    {
                        int s = indices[0, t, k];
                        float lp = logProbs[0, t, k];
                        if (s == BlankId)
                        {
                            lp -= _blankPenalty;
                            var e = GetOrAdd(next, beam.Tokens, beam.Key);
                            e.PBlank = LogSumExp(e.PBlank, pTot + lp);
                        }
                        else if (s == last)
                        {
                            // 与前一 token 相同：经 blank 分隔才算新一次出现
                            var eExt = GetOrAddExtended(next, beam.Tokens, s);
                            eExt.PNonBlank = LogSumExp(eExt.PNonBlank, pb + lp);
                            // 直接重复则折叠回原前缀
                            var eSame = GetOrAdd(next, beam.Tokens, beam.Key);
                            eSame.PNonBlank = LogSumExp(eSame.PNonBlank, pnb + lp);
                        }
                        else
                        {
                            var eExt = GetOrAddExtended(next, beam.Tokens, s);
                            eExt.PNonBlank = LogSumExp(eExt.PNonBlank, pTot + lp);
                        }
                    }
                }

                // 剪枝：按总 log 概率保留前 _beamSize 个前缀
                beams = next.Count <= _beamSize
                    ? next
                    : next.OrderByDescending(kv => LogSumExp(kv.Value.PBlank, kv.Value.PNonBlank))
                          .Take(_beamSize)
                          .ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            BeamEntry? best = null;
            float bestScore = NegInf;
            foreach (var e in beams.Values)
            {
                float score = LogSumExp(e.PBlank, e.PNonBlank);
                if (best is null || score > bestScore)
                {
                    best = e;
                    bestScore = score;
                }
            }
            return best?.Tokens ?? new List<int>();
        }

        private sealed class BeamEntry
        {
            public List<int> Tokens;
            public string Key;
            public float PBlank;
            public float PNonBlank;

            public BeamEntry(List<int> tokens, float pBlank, float pNonBlank)
            {
                Tokens = tokens;
                Key = string.Join(",", tokens);
                PBlank = pBlank;
                PNonBlank = pNonBlank;
            }
        }

        private static BeamEntry GetOrAdd(Dictionary<string, BeamEntry> map, List<int> tokens, string key)
        {
            if (!map.TryGetValue(key, out var e))
            {
                e = new BeamEntry(tokens, float.NegativeInfinity, float.NegativeInfinity);
                map[key] = e;
            }
            return e;
        }

        private static BeamEntry GetOrAddExtended(Dictionary<string, BeamEntry> map, List<int> tokens, int token)
        {
            string key = tokens.Count == 0 ? token.ToString() : string.Join(",", tokens) + "," + token;
            if (!map.TryGetValue(key, out var e))
            {
                var newTokens = new List<int>(tokens.Count + 1);
                newTokens.AddRange(tokens);
                newTokens.Add(token);
                e = new BeamEntry(newTokens, float.NegativeInfinity, float.NegativeInfinity);
                map[key] = e;
            }
            return e;
        }

        private static float LogSumExp(float a, float b)
        {
            if (float.IsNegativeInfinity(a))
            {
                return b;
            }
            if (float.IsNegativeInfinity(b))
            {
                return a;
            }
            float m = Math.Max(a, b);
            return m + (float)Math.Log(Math.Exp(a - m) + Math.Exp(b - m));
        }

        /// <summary>
        /// 仅保留英文相关字符（英文字母、数字、空白及常见英文标点），
        /// 过滤掉中文等非英文字符，并将连续空白合并为单个空格。
        /// </summary>
        private static string KeepEnglishOnly(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ||
                    c == ' ' || c == '\'' || c == '-')
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append(' ');
                }
                // 其余字符（中文等非英文字符）直接丢弃
            }

            // 合并连续空白为单个空格
            var result = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "\\s+", " ");
            return result.Trim();
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
