using LearnEnglish.Models;

namespace LearnEnglish.WhisperModels
{
    public static class ProgramExtensions
    {
        public static void AddWhisperTranscription(this IServiceCollection services, IConfiguration configuration)
        {
            var modelPath = configuration["Whisper:ModelPath"];
            var cmuDictPath = configuration["Whisper:CmuDictPath"];
            services.AddSingleton<ITranscriptionService>(provider =>
            {
                // 使用绝对路径更可靠
                var fullModelPath = Path.Combine(
                    AppContext.BaseDirectory,
                    modelPath
                );

                return new WhisperTranscriptionService(fullModelPath, cmuDictPath);
            });
        }
    }
}
