using System;
using System.Collections.Generic;
using System.Text;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 微软语音
    /// </summary>
    public interface IEdgeTtsService
    {
        Task<byte[]> GetAudioBytesAsync(string text, string voice);
    }
}
