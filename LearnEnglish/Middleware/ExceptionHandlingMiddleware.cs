using System.Net;
using System.Text.Json;
using LearnEnglish.Domain.Common;
using LearnEnglish.Domain.Exceptions;

namespace LearnEnglish.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
    /// 统一捕获所有未处理异常，根据异常类型返回合适的 HTTP 状态码和响应
    /// 支持 API（JSON）和 MVC（页面重定向）两种响应模式
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 判断请求是否期望 JSON 响应（API 请求）
            var isApiRequest = IsApiRequest(context);

            switch (exception)
            {
                case InvalidUserException invalidUserEx:
                    await HandleInvalidUserException(context, invalidUserEx, isApiRequest);
                    break;

                case UnauthorizedException unauthorizedEx:
                    _logger.LogWarning(unauthorizedEx, "未授权访问: {Message}", unauthorizedEx.Message);
                    if (isApiRequest)
                    {
                        await WriteJsonResponse(context, HttpStatusCode.Unauthorized,
                            ApiResponse.Fail(unauthorizedEx.Message, "UNAUTHORIZED"));
                    }
                    else
                    {
                        context.Response.Redirect("/Login/Login");
                    }
                    break;

                case NotFoundException notFoundEx:
                    _logger.LogWarning(notFoundEx, "资源未找到: {Message}", notFoundEx.Message);
                    if (isApiRequest)
                    {
                        await WriteJsonResponse(context, HttpStatusCode.NotFound,
                            ApiResponse.Fail(notFoundEx.Message, "NOT_FOUND"));
                    }
                    else
                    {
                        context.Response.Redirect("/Error/NotFound");
                    }
                    break;

                case ValidationException validationEx:
                    _logger.LogWarning(validationEx, "验证失败: {Message}", validationEx.Message);
                    await WriteJsonResponse(context, HttpStatusCode.BadRequest,
                        ApiResponse.Fail(validationEx.Message, "VALIDATION_ERROR"));
                    break;

                default:
                    _logger.LogError(exception, "未处理的异常: {Message}", exception.Message);
                    if (isApiRequest)
                    {
                        await WriteJsonResponse(context, HttpStatusCode.InternalServerError,
                            ApiResponse.Fail("服务器内部错误，请稍后重试。", "INTERNAL_ERROR"));
                    }
                    else
                    {
                        context.Response.Redirect("/Home/Error");
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理用户无效异常（账户过期/需重新登录）
        /// </summary>
        private async Task HandleInvalidUserException(HttpContext context, InvalidUserException ex, bool isApiRequest)
        {
            _logger.LogWarning(ex, "用户无效: Type={ErrorType}, Message={Message}", ex.ErrorType, ex.Message);

            if (isApiRequest)
            {
                var statusCode = ex.ErrorType == InvalidUserException.UserErrorType.Expired
                    ? HttpStatusCode.Conflict        // 409 - 账户过期
                    : HttpStatusCode.Gone;           // 410 - 需重新登录

                await WriteJsonResponse(context, statusCode,
                    ApiResponse.Fail(ex.Message, $"USER_{ex.ErrorType.ToString().ToUpperInvariant()}"));
            }
            else
            {
                // MVC 页面：根据错误类型重定向
                if (ex.ErrorType == InvalidUserException.UserErrorType.Unauthorized)
                {
                    context.Response.Redirect("/Login/Login");
                }
                else
                {
                    // 账户过期，重定向到错误页面
                    context.Response.Redirect($"/Error/UserError?msg={Uri.EscapeDataString(ex.Message)}&type={ex.ErrorTypeCode}");
                }
            }
        }

        /// <summary>
        /// 判断是否为 API 请求（非 MVC 页面请求）
        /// </summary>
        private static bool IsApiRequest(HttpContext context)
        {
            // Accept 头包含 application/json
            if (context.Request.Headers.Accept.Any(h =>
                h != null && h.Contains("application/json", StringComparison.OrdinalIgnoreCase)))
                return true;

            // X-Requested-With: XMLHttpRequest（AJAX 请求）
            if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
                return true;

            // 路径以 /api/ 开头
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// 写入 JSON 格式的错误响应
        /// </summary>
        private static async Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, ApiResponse response)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>
    /// 中间件扩展方法
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// 注册全局异常处理中间件
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
