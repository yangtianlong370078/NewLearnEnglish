namespace LearnEnglish.Models
{
    /// <summary>
    /// 百度语音识别请求参数模型
    /// </summary>
    public class RecognitionRequest
    {
        public string Audio { get; set; } = string.Empty;
        public int Rate { get; set; }
        public int Channel { get; set; }
        public string Lan { get; set; } = string.Empty;
        public string Cuid { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// 百度 API 令牌响应模型
    /// </summary>
    public class BaiduTokenModel
    {
        public string? refresh_token { get; set; }
        public string? expires_in { get; set; }
        public string? session_key { get; set; }
        public string? access_token { get; set; }
        public string? scope { get; set; }
        public string? session_secret { get; set; }
    }

    /// <summary>
    /// 百度语音识别结果模型
    /// </summary>
    public class BaiduResultModel
    {
        public string? corpus_no { get; set; }
        public string? err_msg { get; set; }
        public string? err_no { get; set; }
        public List<string>? result { get; set; }
        public string? sn { get; set; }
    }

    /// <summary>
    /// 听力练习数据项
    /// </summary>
    public class HearingItem
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string? Value { get; set; }
        public string? ValueCN { get; set; }
    }
}
