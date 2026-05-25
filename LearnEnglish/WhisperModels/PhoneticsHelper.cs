namespace LearnEnglish.WhisperModels
{
    public class PhoneticsHelper
    {
        private readonly Dictionary<string, string[]> _wordToPhonemes = new();

        public PhoneticsHelper(string cmuDictPath)
        {
            LoadCmuDict(cmuDictPath);
        }

        private void LoadCmuDict(string path)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith(";;;")) continue; // 注释

                var parts = line.Split(' ', '\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                var word = parts[0].Trim().ToLowerInvariant();
                // 去除 (2) (3) 等重音标记
                if (word.Contains('(')) word = word.Split('(')[0];

                var phonemes = parts.Skip(1).ToArray();

                _wordToPhonemes[word] = phonemes;
            }
        }

        public string[] GetPhonemes(string word)
        {
            return _wordToPhonemes.GetValueOrDefault(word.ToLowerInvariant());
        }

        public List<(string word, int distance)> FindSimilarPhoneticWords(string word, int top = 5)
        {
            var targetPhonemes = GetPhonemes(word);
            if (targetPhonemes == null) return new List<(string, int)>();

            return _wordToPhonemes
                .Select(kvp => (kvp.Key, EditDistance(targetPhonemes, kvp.Value)))
                .OrderBy(x => x.Item2)
                .ThenBy(x => x.Key.Length)
                .Take(top)
                .ToList();
        }

        private int EditDistance(string[] a, string[] b)
        {
            int n = a.Length, m = b.Length;
            var dp = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) dp[i, 0] = i;
            for (int j = 0; j <= m; j++) dp[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    if (a[i - 1] == b[j - 1])
                        dp[i, j] = dp[i - 1, j - 1];
                    else
                        dp[i, j] = 1 + Math.Min(Math.Min(dp[i - 1, j], dp[i, j - 1]), dp[i - 1, j - 1]);
                }
            }

            return dp[n, m];
        }
    }
}
