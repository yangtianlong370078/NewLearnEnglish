namespace LearnEnglish.WhisperModels
{
    public interface ITranscriptionService
    {
        Task<string> TranscribeAudioAsync(string audioFilePath);
    }
}
