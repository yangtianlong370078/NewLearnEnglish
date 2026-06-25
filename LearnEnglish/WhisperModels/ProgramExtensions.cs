using LearnEnglish.Models;
using LearnEnglish.WhisperModels.FunAsr;

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

        /// <summary>
        /// 注册 FunASR-CTC-Nano 本地语音识别服务（type=4 使用）。
        /// </summary>
        public static void AddFunAsrTranscription(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IFunAsrTranscriptionService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                // 优先读取配置 FunAsr:ModelDir，缺省时使用项目根目录下的 AsrModel 文件夹
                var configuredDir = configuration["FunAsr:ModelDir"];
                var modelDir = string.IsNullOrWhiteSpace(configuredDir)
                    ? Path.Combine(env.ContentRootPath, "AsrModel")
                    : configuredDir;

                // CTC 解码参数：blank 惩罚（召回弱读音节）与 beam 束宽，缺省 1.0 / 10
                var blankPenalty = configuration.GetValue<float?>("FunAsr:BlankPenalty") ?? 1.0f;
                var beamSize = configuration.GetValue<int?>("FunAsr:BeamSize") ?? 10;

                return new FunAsrTranscriptionService(modelDir, blankPenalty, beamSize);
            });
        }
    }
}
