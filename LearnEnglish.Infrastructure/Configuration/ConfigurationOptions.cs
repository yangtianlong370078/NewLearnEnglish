namespace LearnEnglish.Infrastructure.Configuration
{
    /// <summary>
    /// JWT 配置选项
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// 签名密钥
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 签发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 接收者
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access Token 过期时间（分钟）
        /// </summary>
        public int AccessTokenExpireMinutes { get; set; } = 120;

        /// <summary>
        /// Refresh Token 过期时间（天）
        /// </summary>
        public int RefreshTokenExpireDays { get; set; } = 7;
    }

    /// <summary>
    /// 百度语音 API 配置
    /// </summary>
    public class BaiduOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }

    /// <summary>
    /// 微信 API 配置
    /// </summary>
    public class WeChatOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
    }

    /// <summary>
    /// Whisper 语音模型配置
    /// </summary>
    public class WhisperOptions
    {
        public string ModelPath { get; set; } = string.Empty;
        public string CmuDictPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 讯飞语音 API 配置
    /// </summary>
    public class XfyunOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string AsrMode { get; set; } = string.Empty;
    }

    /// <summary>
    /// CORS 配置选项
    /// </summary>
    public class CorsOptions
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}
