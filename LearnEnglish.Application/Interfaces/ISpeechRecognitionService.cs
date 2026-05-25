namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 语音识别服务接口
    /// </summary>
    public interface ISpeechRecognitionService
    {
        /// <summary>
        /// 使用在线 API 识别语音
        /// </summary>
        Task<string> RecognizeAsync(byte[] audioData, string format, int sampleRate);
    }

    /// <summary>
    /// 本地语音转写服务接口（Whisper）
    /// </summary>
    public interface ITranscriptionService
    {
        /// <summary>
        /// 转写音频文件
        /// </summary>
        Task<string> TranscribeAudioAsync(string audioFilePath);
    }
}
