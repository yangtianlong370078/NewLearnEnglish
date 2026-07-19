using LearnEnglish.Application.Interfaces;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 基于 Orton-Gillingham 常用字母组合的自然拼读拆分器。
    /// 字母组合按四、三、二字符顺序贪心匹配；未命中的字符再按单字母规则处理。
    /// </summary>
    public sealed class PhonicsService : IPhonicsService
    {
        private const string Silent = "∅";

        private static readonly (string Text, string PhoneticSymbol)[] Prefixes =
        [
            ("dis", "dɪs"),
            ("un", "ʌn"),
            ("re", "riː"),
            ("in", "ɪn")
        ];

        private static readonly IReadOnlyDictionary<string, string> FourLetterPatterns =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["eigh"] = "eɪ",
                ["beau"] = "bjuː",
                ["augh"] = "ɔː",
                ["ough"] = "ʌf",
                ["tion"] = "ʃən",
                ["sion"] = "ʒən"
            };

        private static readonly IReadOnlyDictionary<string, string> ThreeLetterPatterns =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["tch"] = "tʃ",
                ["dge"] = "dʒ",
                ["igh"] = "aɪ",
                ["tea"] = "tiː",
                ["sch"] = "sk",
                ["cir"] = "sɜːr",
                ["cle"] = "kəl",
                ["ple"] = "pəl",
                ["ful"] = "fəl",
                ["hap"] = "hæp",
                ["air"] = "ɛr",
                ["are"] = "ɛr",
                ["ear"] = "ɪr",
                ["eer"] = "ɪr",
                ["ure"] = "jʊr"
            };

        private static readonly IReadOnlyDictionary<string, string> TwoLetterPatterns =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ai"] = "eɪ", ["ay"] = "eɪ", ["ea"] = "iː", ["ee"] = "iː",
                ["ei"] = "eɪ", ["ie"] = "iː", ["oa"] = "oʊ", ["oe"] = "oʊ",
                ["oi"] = "ɔɪ", ["oy"] = "ɔɪ", ["oo"] = "uː", ["ou"] = "aʊ",
                ["ow"] = "aʊ", ["au"] = "ɔː", ["aw"] = "ɔː", ["ew"] = "juː",
                ["ui"] = "uː", ["ue"] = "uː", ["ar"] = "ɑːr", ["er"] = "ɜːr",
                ["ir"] = "ɜːr", ["or"] = "ɔːr", ["ur"] = "ɜːr", ["sh"] = "ʃ",
                ["ch"] = "tʃ", ["th"] = "θ", ["wh"] = "w", ["ph"] = "f",
                ["ck"] = "k", ["ng"] = "ŋ", ["nk"] = "ŋk", ["qu"] = "kw",
                ["wr"] = "r", ["kn"] = "n", ["gn"] = "n", ["mb"] = "m",
                ["gh"] = Silent, ["an"] = "æn", ["en"] = "ɛn", ["in"] = "ɪn",
                ["on"] = "ɒn", ["un"] = "ʌn", ["ac"] = "æk", ["es"] = "z",
                ["ti"] = "tɪ",
                ["bl"] = "bl", ["br"] = "br", ["cl"] = "kl", ["cr"] = "kr",
                ["dr"] = "dr", ["fl"] = "fl", ["fr"] = "fr", ["gl"] = "ɡl",
                ["gr"] = "ɡr", ["pl"] = "pl", ["pr"] = "pr", ["sc"] = "sk",
                ["sk"] = "sk", ["sl"] = "sl", ["sm"] = "sm", ["sn"] = "sn",
                ["sp"] = "sp", ["st"] = "st", ["sw"] = "sw", ["tr"] = "tr",
                ["tw"] = "tw", ["ts"] = "ts"
            };

        private static readonly IReadOnlyDictionary<char, string> SingleLetterPatterns =
            new Dictionary<char, string>
            {
                ['a'] = "æ", ['b'] = "b", ['c'] = "k", ['d'] = "d", ['e'] = "ɛ",
                ['f'] = "f", ['g'] = "g", ['h'] = "h", ['i'] = "ɪ", ['j'] = "dʒ",
                ['k'] = "k", ['l'] = "l", ['m'] = "m", ['n'] = "n", ['o'] = "ɒ",
                ['p'] = "p", ['q'] = "k", ['r'] = "r", ['s'] = "s", ['t'] = "t",
                ['u'] = "ʌ", ['v'] = "v", ['w'] = "w", ['x'] = "ks", ['y'] = "j",
                ['z'] = "z"
            };

        /// <inheritdoc />
        public IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> Split(string word)
        {
            var normalizedWord = NormalizeWord(word);
            return normalizedWord.Length == 0
                ? Array.Empty<(string LetterCombine, string PhoneticSymbol)>()
                : SplitNormalized(normalizedWord, true, false);
            }

        /// <inheritdoc />
        public IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> Split(string word, IReadOnlyList<string> syllables)
        {
            var normalizedWord = NormalizeWord(word);
            if (normalizedWord.Length == 0 || syllables.Count == 0)
            {
                return Split(word);
            }

            var normalizedSyllables = syllables
                .Select(NormalizeWord)
                .Where(syllable => syllable.Length > 0)
                .ToList();
            if (string.Concat(normalizedSyllables) != normalizedWord)
            {
                return Split(normalizedWord);
            }

            return normalizedSyllables
                .SelectMany((syllable, index) => SplitNormalized(syllable, index == 0, true))
                .ToList();
        }

        private static IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> SplitNormalized(
            string normalizedWord,
            bool allowPrefix,
            bool collapseShortSyllable)
        {
            var prefix = Prefixes.FirstOrDefault(prefix =>
                allowPrefix &&
                normalizedWord.Length > prefix.Text.Length &&
                normalizedWord.StartsWith(prefix.Text, StringComparison.Ordinal));
            var result = new List<(string LetterCombine, string PhoneticSymbol)>();
            var index = 0;
            if (!string.IsNullOrEmpty(prefix.Text))
            {
                result.Add(prefix);
                index = prefix.Text.Length;
            }

            var remainingSegments = SplitGreedily(normalizedWord, index);
            var mergedSegments = MergeAdjacentSingleLetterSegments(remainingSegments);
            result.AddRange(collapseShortSyllable
                ? CollapseShortSyllable(normalizedWord[index..], mergedSegments)
                : mergedSegments);
            return result;
        }

        private static IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> CollapseShortSyllable(
            string syllable,
            IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> segments)
        {
            if (syllable.Length is > 0 and <= 3 && segments.All(segment => segment.LetterCombine.Length == 1))
            {
                return [(syllable, string.Concat(segments.Select(segment => segment.PhoneticSymbol)))];
            }

            return segments;
        }

        private static List<(string LetterCombine, string PhoneticSymbol)> SplitGreedily(string word, int index)
        {
            var result = new List<(string LetterCombine, string PhoneticSymbol)>();
            for (; index < word.Length;)
            {
                if (TryMatch(word, index, 4, FourLetterPatterns, out var phoneticSymbol, out var length) ||
                    TryMatch(word, index, 3, ThreeLetterPatterns, out phoneticSymbol, out length) ||
                    TryMatch(word, index, 2, TwoLetterPatterns, out phoneticSymbol, out length))
                {
                    result.Add((word.Substring(index, length), phoneticSymbol));
                    index += length;
                    continue;
                }

                var letter = word[index];
                result.Add((letter.ToString(), GetSingleLetterPhonetic(word, index)));
                index++;
            }

            return result;
        }

        private static IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> MergeAdjacentSingleLetterSegments(
            IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> segments)
        {
            var result = new List<(string LetterCombine, string PhoneticSymbol)>();
            for (var index = 0; index < segments.Count; index++)
            {
                var current = segments[index];
                var next = index + 1 < segments.Count ? segments[index + 1] : default;

                // 单辅音 + 短单元音（body 的 bo）；不能跨入已识别的拼读组合或 magic-e 组合。
                if (IsSingleConsonant(current) &&
                    IsShortSingleVowel(next) &&
                    (index + 2 == segments.Count || segments[index + 2].LetterCombine.Length == 1))
                {
                    result.Add((current.LetterCombine + next.LetterCombine, current.PhoneticSymbol + next.PhoneticSymbol));
                    index++;
                    continue;
                }

                // 单辅音 + r 控制元音（under 的 der）。
                if (IsSingleConsonant(current) && next.LetterCombine is "ar" or "er" or "ir" or "or" or "ur")
                {
                    result.Add((current.LetterCombine + next.LetterCombine, current.PhoneticSymbol + next.PhoneticSymbol));
                    index++;
                    continue;
                }

                // 词尾辅音 + y 作为一个末尾拼读块（body 的 dy）。
                if (IsSingleConsonant(current) && next.LetterCombine == "y" && index + 2 == segments.Count)
                {
                    result.Add((current.LetterCombine + next.LetterCombine, current.PhoneticSymbol + next.PhoneticSymbol));
                    index++;
                    continue;
                }

                result.Add(current);
            }

            return result;
        }

        private static bool IsSingleConsonant((string LetterCombine, string PhoneticSymbol) segment) =>
            segment.LetterCombine.Length == 1 && segment.LetterCombine[0] is not 'a' and not 'e' and not 'i' and not 'o' and not 'u' and not 'y';

        private static bool IsShortSingleVowel((string LetterCombine, string PhoneticSymbol) segment) =>
            segment.LetterCombine is { Length: 1 } &&
            segment.LetterCombine[0] is 'a' or 'e' or 'i' or 'o' or 'u' &&
            segment.PhoneticSymbol is "æ" or "ɛ" or "ɪ" or "ɒ" or "ʌ";

        private static bool TryMatch(
            string word,
            int index,
            int length,
            IReadOnlyDictionary<string, string> patterns,
            out string phoneticSymbol,
            out int matchedLength)
        {
            if (index + length <= word.Length && patterns.TryGetValue(word.Substring(index, length), out phoneticSymbol!))
            {
                matchedLength = length;
                return true;
            }

            phoneticSymbol = string.Empty;
            matchedLength = 0;
            return false;
        }

        private static string GetSingleLetterPhonetic(string word, int index)
        {
            var letter = word[index];

            if (letter == 'e' && index == word.Length - 1 && index > 0)
            {
                return Silent;
            }

            if (letter is 'a' or 'e' or 'i' or 'o' or 'u' && IsMagicEPattern(word, index))
            {
                return letter switch
                {
                    'a' => "eɪ",
                    'e' => "iː",
                    'i' => "aɪ",
                    'o' => "oʊ",
                    _ => "juː"
                };
            }

            if (letter == 'c' && HasFollowingSoftVowel(word, index))
            {
                return "s";
            }

            if (letter == 'g' && HasFollowingSoftVowel(word, index))
            {
                return "dʒ";
            }

            if (letter == 'y' && index == word.Length - 1)
            {
                return "iː";
            }

            return SingleLetterPatterns[letter];
        }

        private static bool IsMagicEPattern(string word, int vowelIndex) =>
            vowelIndex + 2 == word.Length - 1 &&
            !(word[vowelIndex + 1] is 'a' or 'e' or 'i' or 'o' or 'u') &&
            word[^1] == 'e';

        private static bool HasFollowingSoftVowel(string word, int index) =>
            index + 1 < word.Length && word[index + 1] is 'e' or 'i' or 'y';

        private static string NormalizeWord(string word) => string.IsNullOrWhiteSpace(word)
            ? string.Empty
            : new string(word
                .Trim()
                .ToLowerInvariant()
                .Where(character => character is >= 'a' and <= 'z')
                .ToArray());
    }
}