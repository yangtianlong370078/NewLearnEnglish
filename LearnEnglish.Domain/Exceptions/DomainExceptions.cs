namespace LearnEnglish.Domain.Exceptions
{
    /// <summary>
    /// 资源未找到异常
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string name, object key) : base($"实体 \"{name}\" ({key}) 未找到。") { }
    }

    /// <summary>
    /// 业务验证异常
    /// </summary>
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors) : base("发生一个或多个验证错误。")
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// 未授权异常
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    /// <summary>
    /// 用户无效异常（账户过期等）
    /// </summary>
    public class InvalidUserException : Exception
    {
        /// <summary>
        /// 用户错误类型
        /// </summary>
        public enum UserErrorType
        {
            /// <summary>
            /// 账户过期
            /// </summary>
            Expired = 1,

            /// <summary>
            /// 需要重新登录
            /// </summary>
            Unauthorized = 2
        }

        /// <summary>
        /// 错误类型
        /// </summary>
        public UserErrorType ErrorType { get; }

        /// <summary>
        /// 兼容旧代码的 int 型错误类型
        /// </summary>
        public int ErrorTypeCode => (int)ErrorType;

        public InvalidUserException(string message, UserErrorType errorType = UserErrorType.Unauthorized) : base(message)
        {
            ErrorType = errorType;
        }
    }
}
