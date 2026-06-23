namespace LearnEnglish.WhisperModels.FunAsr
{
    /// <summary>
    /// 与 Kaldi / FunASR (torchaudio.compliance.kaldi.fbank) 对齐的 80 维 FBANK 特征提取器。
    /// 参数：16kHz、25ms 帧长、10ms 帧移、Hamming 窗、预加重 0.97、去直流、log mel。
    /// 输入采样需为原始 int16 幅值（[-32768, 32767]），与 Kaldi 约定一致。
    /// </summary>
    public sealed class FunAsrFbank
    {
        private const int SampleRate = 16000;
        private const int NumMel = 80;
        private const float Preemph = 0.97f;
        private const float LowFreq = 20f;
        private const float Epsilon = 1.1920929e-7f; // float.Epsilon 对应 Kaldi 的下限

        private readonly int _frameLen;
        private readonly int _frameShift;
        private readonly int _fftSize;
        private readonly float[] _window;
        private readonly (int Offset, float[] Weights)[] _melBanks;

        public FunAsrFbank()
        {
            _frameLen = (int)(SampleRate * 0.025);   // 400
            _frameShift = (int)(SampleRate * 0.010); // 160
            _fftSize = 1;
            while (_fftSize < _frameLen)
            {
                _fftSize <<= 1; // 512
            }
            _window = Hamming(_frameLen);
            _melBanks = BuildMelBanks(NumMel, _fftSize, SampleRate, LowFreq, SampleRate / 2f);
        }

        public List<float[]> Compute(float[] wave)
        {
            int numFrames = wave.Length < _frameLen ? 0 : 1 + (wave.Length - _frameLen) / _frameShift;
            var output = new List<float[]>(numFrames);
            var frame = new float[_frameLen];
            var fftRe = new float[_fftSize];
            var fftIm = new float[_fftSize];

            for (int t = 0; t < numFrames; t++)
            {
                int start = t * _frameShift;
                for (int i = 0; i < _frameLen; i++)
                {
                    frame[i] = wave[start + i];
                }

                // 去直流
                float mean = 0;
                for (int i = 0; i < _frameLen; i++)
                {
                    mean += frame[i];
                }
                mean /= _frameLen;
                for (int i = 0; i < _frameLen; i++)
                {
                    frame[i] -= mean;
                }

                // 预加重
                for (int i = _frameLen - 1; i > 0; i--)
                {
                    frame[i] -= Preemph * frame[i - 1];
                }
                frame[0] -= Preemph * frame[0];

                // 加窗
                for (int i = 0; i < _frameLen; i++)
                {
                    frame[i] *= _window[i];
                }

                // FFT
                Array.Clear(fftRe, 0, _fftSize);
                Array.Clear(fftIm, 0, _fftSize);
                for (int i = 0; i < _frameLen; i++)
                {
                    fftRe[i] = frame[i];
                }
                Fft(fftRe, fftIm);

                int half = _fftSize / 2 + 1;
                var power = new float[half];
                for (int i = 0; i < half; i++)
                {
                    power[i] = fftRe[i] * fftRe[i] + fftIm[i] * fftIm[i];
                }

                // mel + log
                var mel = new float[NumMel];
                for (int m = 0; m < NumMel; m++)
                {
                    float e = 0;
                    var (offset, weights) = _melBanks[m];
                    for (int k = 0; k < weights.Length; k++)
                    {
                        e += power[offset + k] * weights[k];
                    }
                    mel[m] = (float)Math.Log(Math.Max(e, Epsilon));
                }
                output.Add(mel);
            }

            return output;
        }

        private static float[] Hamming(int n)
        {
            var w = new float[n];
            for (int i = 0; i < n; i++)
            {
                w[i] = (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (n - 1)));
            }
            return w;
        }

        private static float MelOf(float f) => 1127.0f * (float)Math.Log(1.0 + f / 700.0);

        private static (int Offset, float[] Weights)[] BuildMelBanks(int numBins, int fftSize, int sr, float lowFreq, float highFreq)
        {
            int numFftBins = fftSize / 2 + 1;
            float fftBinWidth = sr / (float)fftSize;
            float melLow = MelOf(lowFreq);
            float melHigh = MelOf(highFreq);
            float delta = (melHigh - melLow) / (numBins + 1);

            var banks = new (int, float[])[numBins];
            for (int m = 0; m < numBins; m++)
            {
                float leftMel = melLow + m * delta;
                float centerMel = melLow + (m + 1) * delta;
                float rightMel = melLow + (m + 2) * delta;

                var weights = new List<float>();
                int offset = -1;
                for (int k = 0; k < numFftBins; k++)
                {
                    float mel = MelOf(k * fftBinWidth);
                    if (mel > leftMel && mel < rightMel)
                    {
                        float w = mel <= centerMel
                            ? (mel - leftMel) / (centerMel - leftMel)
                            : (rightMel - mel) / (rightMel - centerMel);
                        if (offset < 0)
                        {
                            offset = k;
                        }
                        weights.Add(w);
                    }
                    else if (offset >= 0)
                    {
                        break;
                    }
                }
                banks[m] = (offset < 0 ? 0 : offset, weights.ToArray());
            }
            return banks;
        }

        /// <summary>原地迭代 FFT（输入长度必须为 2 的幂）。</summary>
        private static void Fft(float[] re, float[] im)
        {
            int n = re.Length;
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }
                j ^= bit;
                if (i < j)
                {
                    (re[i], re[j]) = (re[j], re[i]);
                    (im[i], im[j]) = (im[j], im[i]);
                }
            }

            for (int len = 2; len <= n; len <<= 1)
            {
                double ang = -2 * Math.PI / len;
                float wr = (float)Math.Cos(ang);
                float wi = (float)Math.Sin(ang);
                for (int i = 0; i < n; i += len)
                {
                    float cr = 1, ci = 0;
                    for (int k = 0; k < len / 2; k++)
                    {
                        float ur = re[i + k];
                        float ui = im[i + k];
                        float vr = re[i + k + len / 2] * cr - im[i + k + len / 2] * ci;
                        float vi = re[i + k + len / 2] * ci + im[i + k + len / 2] * cr;
                        re[i + k] = ur + vr;
                        im[i + k] = ui + vi;
                        re[i + k + len / 2] = ur - vr;
                        im[i + k + len / 2] = ui - vi;
                        float ncr = cr * wr - ci * wi;
                        float nci = cr * wi + ci * wr;
                        cr = ncr;
                        ci = nci;
                    }
                }
            }
        }
    }
}
