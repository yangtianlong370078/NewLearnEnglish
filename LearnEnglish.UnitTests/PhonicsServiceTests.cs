using LearnEnglish.Infrastructure.Services;

namespace LearnEnglish.UnitTests
{
    public class PhonicsServiceTests
    {
        [Fact]
        public void Split_PrioritizesFourLetterPattern()
        {
            var result = new PhonicsService().Split("eight");

            Assert.Equal([("eigh", "eɪ"), ("t", "t")], result);
        }

        [Fact]
        public void Split_PrioritizesThreeLetterPatternBeforeTwoLetterPattern()
        {
            var result = new PhonicsService().Split("catch");

            Assert.Equal([("c", "k"), ("a", "æ"), ("tch", "tʃ")], result);
        }

        [Fact]
        public void Split_UsesTwoLetterPatternsAndKeepsEveryLetter()
        {
            var result = new PhonicsService().Split("sheep");

            Assert.Equal([("sh", "ʃ"), ("ee", "iː"), ("p", "p")], result);
            Assert.Equal("sheep", string.Concat(result.Select(item => item.LetterCombine)));
        }

        [Fact]
        public void Split_RecognizesMagicEAndSilentE()
        {
            var result = new PhonicsService().Split("cake");

            Assert.Equal([("c", "k"), ("a", "eɪ"), ("k", "k"), ("e", "∅")], result);
        }

        [Theory]
        [InlineData("body", new[] { "bo", "dy" })]
        [InlineData("under", new[] { "un", "der" })]
        [InlineData("clothes", new[] { "cl", "o", "th", "es" })]
        [InlineData("pants", new[] { "p", "an", "ts" })]
        public void Split_RespectsPrefixesAndDoesNotBreakPhonicsCombinations(string word, string[] expected)
        {
            var result = new PhonicsService().Split(word);

            Assert.Equal(expected, result.Select(segment => segment.LetterCombine));
        }

        [Fact]
        public void Split_NormalizesCaseAndRemovesSymbols()
        {
            var result = new PhonicsService().Split("P-A.N!TS");

            Assert.Equal(["p", "an", "ts"], result.Select(segment => segment.LetterCombine));
        }

        [Theory]
        [InlineData("teacher", new[] { "tea", "cher" }, new[] { "tea", "ch", "er" })]
        [InlineData("apple", new[] { "ap", "ple" }, new[] { "ap", "ple" })]
        [InlineData("beautiful", new[] { "beau", "ti", "ful" }, new[] { "beau", "ti", "ful" })]
        [InlineData("unhappy", new[] { "un", "hap", "py" }, new[] { "un", "hap", "py" })]
        [InlineData("action", new[] { "ac", "tion" }, new[] { "ac", "tion" })]
        [InlineData("circle", new[] { "cir", "cle" }, new[] { "cir", "cle" })]
        [InlineData("school", new[] { "school" }, new[] { "sch", "oo", "l" })]
        public void Split_UsesSyllableBoundariesBeforeApplyingPhonicsRules(string word, string[] syllables, string[] expected)
        {
            var result = new PhonicsService().Split(word, syllables);

            Assert.Equal(expected, result.Select(segment => segment.LetterCombine));
        }
    }
}