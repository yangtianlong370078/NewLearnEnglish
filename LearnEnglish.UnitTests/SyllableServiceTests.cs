using LearnEnglish.Infrastructure.Configuration;
using LearnEnglish.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace LearnEnglish.UnitTests
{
    public class SyllableServiceTests : IDisposable
    {
        private readonly string _dictionaryPath;

        public SyllableServiceTests()
        {
            _dictionaryPath = Path.Combine(Path.GetTempPath(), $"cmudict-{Guid.NewGuid():N}.txt");
            File.WriteAllText(_dictionaryPath, ";;; test dictionary\nSOMETHING  S AH1 M TH IH0 NG\n");
        }

        [Fact]
        public void GetSyllables_UsesCmuVowelCountForKnownWord()
        {
            var service = CreateService();

            var result = service.GetSyllables("something");

            Assert.Equal(2, result.Count);
            Assert.Equal("something", string.Concat(result));
        }

        [Fact]
        public void GetSyllables_PreservesPipelineCompoundBoundary()
        {
            File.AppendAllText(_dictionaryPath, "PIPELINE  P AY1 P L AY2 N\n");
            var service = CreateService();

            var result = service.GetSyllables("pipeline");

            Assert.Equal(["pipe", "line"], result);
        }

        [Fact]
        public void GetSyllables_UsesRulesWhenWordIsNotInCmu()
        {
            var service = CreateService();

            var result = service.GetSyllables("elephant");

            Assert.True(result.Count > 1);
            Assert.Equal("elephant", string.Concat(result));
        }

        [Fact]
        public void GetSyllables_ReturnsEmptyForBlankInput()
        {
            var service = CreateService();

            var result = service.GetSyllables("   ");

            Assert.Empty(result);
        }

        public void Dispose()
        {
            File.Delete(_dictionaryPath);
        }

        private SyllableService CreateService()
        {
            var options = Options.Create(new SyllableOptions
            {
                CmuDictionaryPath = _dictionaryPath
            });

            return new SyllableService(options);
        }
    }
}
