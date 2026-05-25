namespace LearnEnglish.Domain.Common
{
    /// <summary>
    /// 统一 API 响应封装
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> Fail(string message, string? errorCode = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    /// <summary>
    /// 无数据的 API 响应
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }

        public static ApiResponse Ok(string? message = null)
        {
            return new ApiResponse { Success = true, Message = message };
        }

        public static ApiResponse Fail(string message, string? errorCode = null)
        {
            return new ApiResponse { Success = false, Message = message, ErrorCode = errorCode };
        }
    }
}
