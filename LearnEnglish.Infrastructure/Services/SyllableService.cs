using System.Text.RegularExpressions;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 基于 CMU 发音词典和英文拼写规则拆分单词音节。
    /// </summary>
    public sealed class SyllableService : ISyllableService
    {
        private static readonly Regex AlternatePronunciationSuffix = new(@"\(\d+\)$", RegexOptions.Compiled);
        private static readonly HashSet<string> VowelPhonemes = new(StringComparer.Ordinal)
        {
            "AA", "AE", "AH", "AO", "AW", "AY", "EH", "ER", "EY", "IH", "IY", "OW", "OY", "UH", "UW"
        };
        private static readonly HashSet<string> ConsonantDigraphs = new(StringComparer.Ordinal)
        {
            "ch", "sh", "th", "ph", "wh", "qu", "ck", "ng"
        };
        private static readonly string[] CompoundSuffixes = ["thing", "line"];

        private readonly IReadOnlyDictionary<string, string[]> _phonemesByWord;

        /// <summary>
        /// 创建服务并一次性将 CMU 词典加载到内存。
        /// </summary>
        public SyllableService(IOptions<SyllableOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var dictionaryPath = options.Value.CmuDictionaryPath;
            if (string.IsNullOrWhiteSpace(dictionaryPath))
            {
                throw new InvalidOperationException("未配置 CMU 词典路径。");
            }

            var fullPath = ResolveDictionaryPath(dictionaryPath);
            if (fullPath == null)
            {
                throw new FileNotFoundException("未找到 CMU 词典文件。", Path.GetFullPath(dictionaryPath));
            }

            _phonemesByWord = LoadDictionary(fullPath);
        }

        private static string? ResolveDictionaryPath(string configuredPath)
        {
            var configuredFullPath = Path.GetFullPath(configuredPath);
            if (File.Exists(configuredFullPath))
            {
                return configuredFullPath;
            }

            // 发布时词典会被复制到应用输出目录下的 CMU 文件夹。
            var publishedPath = Path.Combine(AppContext.BaseDirectory, "CMU", Path.GetFileName(configuredPath));
            return File.Exists(publishedPath) ? publishedPath : null;
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSyllables(string word)
        {
            var normalizedWord = NormalizeWord(word);
            if (normalizedWord.Length == 0)
            {
                return Array.Empty<string>();
            }

            if (_phonemesByWord.TryGetValue(normalizedWord, out var phonemes))
            {
                var cmuSyllableCount = phonemes.Count(IsVowelPhoneme);
                if (cmuSyllableCount > 0)
                {
                    return SplitUsingSpellingRules(normalizedWord, cmuSyllableCount);
                }
            }

            return SplitUsingSpellingRules(normalizedWord, null);
        }

        private static IReadOnlyDictionary<string, string[]> LoadDictionary(string path)
        {
            var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";;;", StringComparison.Ordinal))
                {
                    continue;
                }

                var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                var word = AlternatePronunciationSuffix.Replace(parts[0], string.Empty).ToLowerInvariant();
                if (word.Length == 0 || result.ContainsKey(word))
                {
                    continue;
                }

                result[word] = parts[1..];
            }

            return result;
        }

        private static IReadOnlyList<string> SplitUsingSpellingRules(string word, int? targetSyllableCount)
        {
            if (targetSyllableCount == 2 && TrySplitCommonCompound(word, out var compoundSyllables))
            {
                return compoundSyllables;
            }

            var vowelGroups = FindVowelGroups(word);
            if (vowelGroups.Count == 0)
            {
                return [word];
            }

            var syllables = new List<string>();
            var segmentStart = 0;

            for (var i = 0; i < vowelGroups.Count - 1; i++)
            {
                var currentGroup = vowelGroups[i];
                var nextGroup = vowelGroups[i + 1];
                var consonantStart = currentGroup.End + 1;
                var consonantCount = nextGroup.Start - consonantStart;

                var boundary = consonantCount switch
                {
                    <= 0 => nextGroup.Start,
                    1 => consonantStart,
                    _ when ConsonantDigraphs.Contains(word.Substring(consonantStart, consonantCount)) => consonantStart,
                    _ => nextGroup.Start - 1
                };

                syllables.Add(word[segmentStart..boundary]);
                segmentStart = boundary;
            }

            syllables.Add(word[segmentStart..]);
            return AdjustSyllableCount(syllables, targetSyllableCount);
        }

        private static bool TrySplitCommonCompound(string word, out IReadOnlyList<string> syllables)
        {
            foreach (var suffix in CompoundSuffixes)
            {
                if (word.Length > suffix.Length && word.EndsWith(suffix, StringComparison.Ordinal))
                {
                    syllables = [word[..^suffix.Length], suffix];
                    return true;
                }
            }

            syllables = Array.Empty<string>();
            return false;
        }

        private static List<string> AdjustSyllableCount(List<string> syllables, int? targetSyllableCount)
        {
            if (targetSyllableCount is null or <= 0)
            {
                return syllables;
            }

            while (syllables.Count > targetSyllableCount.Value)
            {
                var lastIndex = syllables.Count - 1;
                syllables[lastIndex - 1] += syllables[lastIndex];
                syllables.RemoveAt(lastIndex);
            }

            while (syllables.Count < targetSyllableCount.Value)
            {
                var longestIndex = syllables
                    .Select((syllable, index) => (syllable, index))
                    .OrderByDescending(item => item.syllable.Length)
                    .First().index;
                var syllable = syllables[longestIndex];

                if (syllable.Length < 2)
                {
                    break;
                }

                var splitAt = Math.Max(1, syllable.Length / 2);
                syllables[longestIndex] = syllable[..splitAt];
                syllables.Insert(longestIndex + 1, syllable[splitAt..]);
            }

            return syllables;
        }

        private static List<(int Start, int End)> FindVowelGroups(string word)
        {
            var groups = new List<(int Start, int End)>();

            for (var index = 0; index < word.Length; index++)
            {
                if (!IsVowel(word[index]))
                {
                    continue;
                }

                var start = index;
                while (index + 1 < word.Length && IsVowel(word[index + 1]))
                {
                    index++;
                }

                groups.Add((start, index));
            }

            if (groups.Count > 1 && groups[^1].Start == word.Length - 1 && word[^1] == 'e')
            {
                groups.RemoveAt(groups.Count - 1);
            }

            return groups;
        }

        private static bool IsVowel(char character) => character is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';

        private static bool IsVowelPhoneme(string phoneme) => VowelPhonemes.Contains(phoneme.TrimEnd('0', '1', '2'));

        private static string NormalizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return string.Empty;
            }

            var normalized = word.Trim().ToLowerInvariant();
            return normalized.All(character => character is >= 'a' and <= 'z') ? normalized : string.Empty;
        }
    }
}
