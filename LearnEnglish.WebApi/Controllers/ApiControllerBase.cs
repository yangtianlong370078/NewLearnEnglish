using LearnEnglish.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// Web API 控制器基类，提供统一的响应辅助方法。 测试提交22
    /// 用于与主项目 MVC BaseController 的 ApiOk/ApiFail 语义一致，方便前端兼容。
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>成功响应</summary>
        protected IActionResult ApiOk(object? data = null, string? message = null)
        {
            return Ok(new ApiResponse<object?>
            {
                Success = true,
                Data = data,
                Message = message
            });
        }

        /// <summary>失败响应</summary>
        protected IActionResult ApiFail(string message, string? errorCode = null)
        {
            return Ok(new ApiResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            });
        }

        /// <summary>分页成功响应</summary>
        protected IActionResult ApiOkPaged<T>(PagedList<T> pagedList)
        {
            return Ok(new
            {
                success = true,
                data = pagedList.Items,
                total = pagedList.TotalCount,
                pageIndex = pagedList.PageIndex,
                pageSize = pagedList.PageSize,
                pageCount = pagedList.PageCount
            });
        }
    }
}
