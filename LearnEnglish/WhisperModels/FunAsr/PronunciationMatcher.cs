using System.Text;

namespace LearnEnglish.WhisperModels.FunAsr
{
    /// <summary>
    /// 发音容错匹配器：用于在语音识别结果与目标单词之间做“宽松”匹配，
    /// 即使发音不太标准、识别结果有偏差，只要足够接近也判定为正确。
    ///
    /// 匹配策略（取多种相似度的最大值）：
    /// 1. 子串包含（识别结果里出现了目标词，或反之）。
    /// 2. 字母层面的归一化编辑距离相似度（Levenshtein）。
    /// 3. Double Metaphone 音码相似度（对“听起来像”的拼写错误更鲁棒）。
    /// </summary>
    public sealed class PronunciationMatcher
    {
        /// <summary>默认相似度阈值（0~1，越小越宽松）。0.55 表示允许约 45% 的差异，偏宽松。</summary>
        public const double DefaultThreshold = 0.55;

        private readonly double _threshold;

        public PronunciationMatcher(double threshold = DefaultThreshold)
        {
            _threshold = Math.Clamp(threshold, 0d, 1d);
        }

        /// <summary>
        /// 判断识别结果是否与目标单词“足够接近”。
        /// </summary>
        public bool IsMatch(string recognized, string target)
            => Similarity(recognized, target) >= _threshold;

        /// <summary>
        /// 在一批候选词中，只要有一个与目标足够接近即视为命中。
        /// </summary>
        public bool AnyMatch(IEnumerable<string> candidates, string target)
        {
            foreach (var c in candidates)
            {
                if (IsMatch(c, target))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算综合相似度（0~1），越大越相似。
        /// </summary>
        public double Similarity(string a, string b)
        {
            a = Normalize(a);
            b = Normalize(b);
            if (a.Length == 0 || b.Length == 0)
            {
                return 0d;
            }
            if (a == b)
            {
                return 1d;
            }

            // 子串包含：发音多/少一点（如吞音、连读）时给高分
            if (a.Contains(b) || b.Contains(a))
            {
                return 0.95d;
            }

            double letterSim = LevenshteinSimilarity(a, b);
            double phoneticSim = PhoneticSimilarity(a, b);

            return Math.Max(letterSim, phoneticSim);
        }

        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s.ToLowerInvariant())
            {
                if (char.IsLetter(ch))
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        /// <summary>字母层面的归一化编辑距离相似度。</summary>
        private static double LevenshteinSimilarity(string a, string b)
        {
            int distance = Levenshtein(a, b);
            int maxLen = Math.Max(a.Length, b.Length);
            return maxLen == 0 ? 1d : 1d - (double)distance / maxLen;
        }

        /// <summary>基于 Double Metaphone 主码/次码的相似度。</summary>
        private static double PhoneticSimilarity(string a, string b)
        {
            var (aPrimary, aAlternate) = DoubleMetaphone.Encode(a);
            var (bPrimary, bAlternate) = DoubleMetaphone.Encode(b);

            double best = 0d;
            best = Math.Max(best, CodeSimilarity(aPrimary, bPrimary));
            best = Math.Max(best, CodeSimilarity(aPrimary, bAlternate));
            best = Math.Max(best, CodeSimilarity(aAlternate, bPrimary));
            best = Math.Max(best, CodeSimilarity(aAlternate, bAlternate));
            return best;
        }

        private static double CodeSimilarity(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return 0d;
            }
            if (a == b)
            {
                return 1d;
            }
            int distance = Levenshtein(a, b);
            int maxLen = Math.Max(a.Length, b.Length);
            return maxLen == 0 ? 1d : 1d - (double)distance / maxLen;
        }

        private static int Levenshtein(string a, string b)
        {
            int n = a.Length, m = b.Length;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }

            var prev = new int[m + 1];
            var curr = new int[m + 1];
            for (int j = 0; j <= m; j++)
            {
                prev[j] = j;
            }

            for (int i = 1; i <= n; i++)
            {
                curr[0] = i;
                for (int j = 1; j <= m; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    curr[j] = Math.Min(
                        Math.Min(prev[j] + 1, curr[j - 1] + 1),
                        prev[j - 1] + cost);
                }
                (prev, curr) = (curr, prev);
            }
            return prev[m];
        }
    }
}
