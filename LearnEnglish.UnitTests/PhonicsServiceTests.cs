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
        [InlineData("giraffe", new[] { "gi", "raffe" }, new[] { "gi", "ra", "ffe" })]
        [InlineData("lion", new[] { "li", "on" }, new[] { "li", "on" })]
        [InlineData("tomato", new[] { "to", "ma", "to" }, new[] { "to", "ma", "to" })]
        [InlineData("potato", new[] { "po", "ta", "to" }, new[] { "po", "ta", "to" })]
        [InlineData("pants", new[] { "pants" }, new[] { "pan", "ts" })]
        [InlineData("goat", new[] { "goat" }, new[] { "goat" })]
        public void Split_UsesGreedyPhonicsWithinCmuTolerance(string word, string[] syllables, string[] expected)
        {
            var result = new PhonicsService().Split(word, syllables);

            Assert.Equal(expected, result.Select(segment => segment.LetterCombine));
        }

        [Fact]
        public void Split_MergesFromEndWhenGreedyPartsExceedTolerance()
        {
            var result = new PhonicsService().Split("clothes", ["clothes"]);

            Assert.Equal(["cl", "othes"], result.Select(segment => segment.LetterCombine));
        }

        [Theory]
        [InlineData("rabbit", new[] { "rab", "bit" }, new[] { "ra", "bb", "it" })]
        [InlineData("bird", new[] { "bird" }, new[] { "bird" })]
        [InlineData("miss", new[] { "miss" }, new[] { "mi", "ss" })]
        [InlineData("ruler", new[] { "ru", "ler" }, new[] { "ru", "ler" })]
        [InlineData("pencil", new[] { "pen", "cil" }, new[] { "pen", "cil" })]
        [InlineData("where", new[] { "where" }, new[] { "wh", "ere" })]
        [InlineData("today", new[] { "to", "day" }, new[] { "to", "day" })]
        [InlineData("grape", new[] { "grape" }, new[] { "gr", "ape" })]
        [InlineData("five", new[] { "five" }, new[] { "five" })]
        public void Split_PreservesBeginnerPhonicsUnits(string word, string[] syllables, string[] expected)
        {
            var result = new PhonicsService().Split(word, syllables);

            Assert.Equal(expected, result.Select(segment => segment.LetterCombine));
        }

        [Fact]
        public void Split_UsesStrictSyllableModeWhenToleranceIsZero()
        {
            var result = new PhonicsService(0).Split("goat", ["goat"]);

            Assert.Equal(["goat"], result.Select(segment => segment.LetterCombine));
        }
    }
}