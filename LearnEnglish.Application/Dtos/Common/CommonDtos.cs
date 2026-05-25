namespace LearnEnglish.Application.Dtos.Common
{
    /// <summary>
    /// 错误视图模型
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    /// <summary>
    /// 用户错误视图模型
    /// </summary>
    public class UserErrorViewModel
    {
        public string? RequestId { get; set; }
        public string Msg { get; set; } = string.Empty;
        /// <summary>
        /// 错误类型（1=账户过期，2=需要重新登录）
        /// </summary>
        public int Type { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
