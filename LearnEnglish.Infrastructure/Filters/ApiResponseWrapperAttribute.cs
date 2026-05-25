using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LearnEnglish.Domain.Common;

namespace LearnEnglish.Infrastructure.Filters
{
    /// <summary>
    /// API 响应统一包装过滤器
    /// 将控制器返回的原始 JSON 数据自动包装为 ApiResponse&lt;T&gt; 格式
    /// 仅对标记了 [ApiResponseWrapper] 的 Action 或 Controller 生效
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiResponseWrapperAttribute : Attribute, IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            // 仅处理成功的 ObjectResult / JsonResult
            if (context.Result is ObjectResult objectResult && objectResult.StatusCode is null or (>= 200 and < 300))
            {
                var value = objectResult.Value;

                // 如果已经是 ApiResponse 类型，不重复包装
                if (value is ApiResponse || IsGenericApiResponse(value))
                    return;

                objectResult.Value = new ApiResponse<object?>
                {
                    Success = true,
                    Data = value,
                    Message = null
                };
            }
            else if (context.Result is JsonResult jsonResult)
            {
                var value = jsonResult.Value;

                // 如果已经是 ApiResponse 类型或匿名 { success = ... } 对象，不重复包装
                if (value is ApiResponse || IsGenericApiResponse(value) || HasSuccessProperty(value))
                    return;

                jsonResult.Value = new ApiResponse<object?>
                {
                    Success = true,
                    Data = value,
                    Message = null
                };
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // 不需要后处理
        }

        /// <summary>
        /// 检查是否为 ApiResponse&lt;T&gt; 泛型类型
        /// </summary>
        private static bool IsGenericApiResponse(object? value)
        {
            if (value == null) return false;
            var type = value.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
        }

        /// <summary>
        /// 检查对象是否已包含 success 属性（兼容匿名对象 new { success = true, ... }）
        /// </summary>
        private static bool HasSuccessProperty(object? value)
        {
            if (value == null) return false;
            var type = value.GetType();
            return type.GetProperty("success") != null || type.GetProperty("Success") != null;
        }
    }
}
