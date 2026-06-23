using System.Text;

namespace LearnEnglish.WhisperModels.FunAsr
{
    /// <summary>
    /// 轻量级 Double Metaphone 实现（Lawrence Philips 算法的简化版），
    /// 用于将英文单词编码为“发音音码”，便于做发音相似度比较。
    /// 返回主码与次码（次码用于处理多音/多种发音情况）。
    /// </summary>
    internal static class DoubleMetaphone
    {
        private const int MaxLength = 6;

        public static (string Primary, string Alternate) Encode(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return (string.Empty, string.Empty);
            }

            string word = input.ToUpperInvariant();
            var primary = new StringBuilder();
            var alternate = new StringBuilder();
            int length = word.Length;
            int last = length - 1;
            int current = 0;

            // 跳过部分词首静音组合
            if (StartsWith(word, current, "GN", "KN", "PN", "WR", "PS"))
            {
                current++;
            }

            // 词首 X 读作 S（如 XAVIER）
            if (CharAt(word, 0) == 'X')
            {
                Add(primary, alternate, "S");
                current++;
            }

            while (current < length && (primary.Length < MaxLength || alternate.Length < MaxLength))
            {
                char c = CharAt(word, current);
                switch (c)
                {
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'O':
                    case 'U':
                    case 'Y':
                        if (current == 0)
                        {
                            Add(primary, alternate, "A");
                        }
                        current++;
                        break;

                    case 'B':
                        Add(primary, alternate, "P");
                        current += CharAt(word, current + 1) == 'B' ? 2 : 1;
                        break;

                    case 'Ç':
                        Add(primary, alternate, "S");
                        current++;
                        break;

                    case 'C':
                        current = EncodeC(word, current, primary, alternate, last);
                        break;

                    case 'D':
                        if (StringAt(word, current, 2, "DG"))
                        {
                            if (Contains("IEY", CharAt(word, current + 2)))
                            {
                                Add(primary, alternate, "J");
                                current += 3;
                            }
                            else
                            {
                                Add(primary, alternate, "TK");
                                current += 2;
                            }
                        }
                        else
                        {
                            Add(primary, alternate, "T");
                            current += StringAt(word, current, 2, "DT", "DD") ? 2 : 1;
                        }
                        break;

                    case 'F':
                        Add(primary, alternate, "F");
                        current += CharAt(word, current + 1) == 'F' ? 2 : 1;
                        break;

                    case 'G':
                        current = EncodeG(word, current, primary, alternate);
                        break;

                    case 'H':
                        if ((current == 0 || IsVowel(CharAt(word, current - 1))) && IsVowel(CharAt(word, current + 1)))
                        {
                            Add(primary, alternate, "H");
                            current += 2;
                        }
                        else
                        {
                            current++;
                        }
                        break;

                    case 'J':
                        Add(primary, alternate, "J");
                        current += CharAt(word, current + 1) == 'J' ? 2 : 1;
                        break;

                    case 'K':
                        Add(primary, alternate, "K");
                        current += CharAt(word, current + 1) == 'K' ? 2 : 1;
                        break;

                    case 'L':
                        Add(primary, alternate, "L");
                        current += CharAt(word, current + 1) == 'L' ? 2 : 1;
                        break;

                    case 'M':
                        Add(primary, alternate, "M");
                        current += CharAt(word, current + 1) == 'M' ? 2 : 1;
                        break;

                    case 'N':
                        Add(primary, alternate, "N");
                        current += CharAt(word, current + 1) == 'N' ? 2 : 1;
                        break;

                    case 'Ñ':
                        Add(primary, alternate, "N");
                        current++;
                        break;

                    case 'P':
                        if (CharAt(word, current + 1) == 'H')
                        {
                            Add(primary, alternate, "F");
                            current += 2;
                        }
                        else
                        {
                            Add(primary, alternate, "P");
                            current += Contains("PB", CharAt(word, current + 1)) ? 2 : 1;
                        }
                        break;

                    case 'Q':
                        Add(primary, alternate, "K");
                        current += CharAt(word, current + 1) == 'Q' ? 2 : 1;
                        break;

                    case 'R':
                        Add(primary, alternate, "R");
                        current += CharAt(word, current + 1) == 'R' ? 2 : 1;
                        break;

                    case 'S':
                        current = EncodeS(word, current, primary, alternate, last);
                        break;

                    case 'T':
                        current = EncodeT(word, current, primary, alternate);
                        break;

                    case 'V':
                        Add(primary, alternate, "F");
                        current += CharAt(word, current + 1) == 'V' ? 2 : 1;
                        break;

                    case 'W':
                        if (CharAt(word, current + 1) == 'H')
                        {
                            Add(primary, alternate, "A");
                            current += 2;
                        }
                        else if (IsVowel(CharAt(word, current + 1)))
                        {
                            Add(primary, alternate, "A");
                            current++;
                        }
                        else
                        {
                            current++;
                        }
                        break;

                    case 'X':
                        Add(primary, alternate, "KS");
                        current += StringAt(word, current, 2, "XX") ? 2 : 1;
                        break;

                    case 'Z':
                        Add(primary, alternate, "S");
                        current += CharAt(word, current + 1) == 'Z' ? 2 : 1;
                        break;

                    default:
                        current++;
                        break;
                }
            }

            string p = Trim(primary);
            string a = Trim(alternate);
            return (p, string.IsNullOrEmpty(a) ? p : a);
        }

        private static int EncodeC(string word, int current, StringBuilder primary, StringBuilder alternate, int last)
        {
            if (current > 1 && !IsVowel(CharAt(word, current - 2))
                && StringAt(word, current - 1, 3, "ACH")
                && CharAt(word, current + 2) != 'I'
                && CharAt(word, current + 2) != 'E')
            {
                Add(primary, alternate, "K");
                return current + 2;
            }

            if (StringAt(word, current, 2, "CH"))
            {
                Add(primary, alternate, "X"); // CH -> X（如 CHURCH）
                return current + 2;
            }

            if (StringAt(word, current, 2, "CC") && !(current == 1 && CharAt(word, 0) == 'M'))
            {
                if (Contains("IEH", CharAt(word, current + 2)) && !StringAt(word, current + 2, 2, "HU"))
                {
                    Add(primary, alternate, "KS");
                    return current + 3;
                }
                Add(primary, alternate, "K");
                return current + 2;
            }

            if (StringAt(word, current, 2, "CK", "CG", "CQ"))
            {
                Add(primary, alternate, "K");
                return current + 2;
            }

            if (StringAt(word, current, 2, "CI", "CE", "CY"))
            {
                Add(primary, alternate, "S");
                return current + 2;
            }

            Add(primary, alternate, "K");
            return current + 1;
        }

        private static int EncodeG(string word, int current, StringBuilder primary, StringBuilder alternate)
        {
            if (CharAt(word, current + 1) == 'H')
            {
                if (current > 0 && !IsVowel(CharAt(word, current - 1)))
                {
                    Add(primary, alternate, "K");
                    return current + 2;
                }
                // GH 多数情况静音
                return current + 2;
            }

            if (CharAt(word, current + 1) == 'N')
            {
                // GN 词中静音 G（如 SIGN）
                Add(primary, alternate, "N");
                return current + 2;
            }

            if (Contains("IEY", CharAt(word, current + 1)))
            {
                Add(primary, alternate, "J");
                return current + 2;
            }

            Add(primary, alternate, "K");
            return current + (CharAt(word, current + 1) == 'G' ? 2 : 1);
        }

        private static int EncodeS(string word, int current, StringBuilder primary, StringBuilder alternate, int last)
        {
            if (StringAt(word, current, 2, "SH"))
            {
                Add(primary, alternate, "X");
                return current + 2;
            }

            if (StringAt(word, current, 3, "SIO", "SIA"))
            {
                Add(primary, alternate, "X", "S");
                return current + 3;
            }

            if (StringAt(word, current, 2, "SC"))
            {
                if (CharAt(word, current + 2) == 'H')
                {
                    Add(primary, alternate, "SK");
                    return current + 3;
                }
                if (Contains("IEY", CharAt(word, current + 2)))
                {
                    Add(primary, alternate, "S");
                    return current + 3;
                }
                Add(primary, alternate, "SK");
                return current + 3;
            }

            Add(primary, alternate, "S");
            return current + (Contains("SZ", CharAt(word, current + 1)) ? 2 : 1);
        }

        private static int EncodeT(string word, int current, StringBuilder primary, StringBuilder alternate)
        {
            if (StringAt(word, current, 3, "TIO", "TIA"))
            {
                Add(primary, alternate, "X");
                return current + 3;
            }

            if (StringAt(word, current, 2, "TH"))
            {
                Add(primary, alternate, "0"); // TH 音
                return current + 2;
            }

            Add(primary, alternate, "T");
            return current + (Contains("TD", CharAt(word, current + 1)) ? 2 : 1);
        }

        // ---------- helpers ----------

        private static char CharAt(string s, int index)
            => index >= 0 && index < s.Length ? s[index] : '\0';

        private static bool IsVowel(char c) => "AEIOUY".IndexOf(c) >= 0;

        private static bool Contains(string set, char c) => c != '\0' && set.IndexOf(c) >= 0;

        private static bool StartsWith(string word, int start, params string[] options)
        {
            foreach (var opt in options)
            {
                if (StringAt(word, start, opt.Length, opt))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool StringAt(string word, int start, int length, params string[] options)
        {
            if (start < 0 || start + length > word.Length)
            {
                return false;
            }
            string sub = word.Substring(start, length);
            foreach (var opt in options)
            {
                if (sub == opt)
                {
                    return true;
                }
            }
            return false;
        }

        private static void Add(StringBuilder primary, StringBuilder alternate, string value)
        {
            primary.Append(value);
            alternate.Append(value);
        }

        private static void Add(StringBuilder primary, StringBuilder alternate, string primaryValue, string alternateValue)
        {
            primary.Append(primaryValue);
            alternate.Append(alternateValue);
        }

        private static string Trim(StringBuilder sb)
        {
            if (sb.Length > MaxLength)
            {
                sb.Length = MaxLength;
            }
            return sb.ToString();
        }
    }
}
