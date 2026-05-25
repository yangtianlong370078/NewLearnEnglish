using LearnEnglish.Domain.Common;
using LearnEnglish.Models.ErrorHelper;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 控制器基类，提供统一的 API 响应辅助方法
    /// </summary>
    [ServiceFilter(typeof(InvalidUserExceptionFilter))]
    public class BaseController : Controller
    {
        /// <summary>
        /// 返回成功的 JSON 响应
        /// </summary>
        protected JsonResult ApiOk(object? data = null, string? message = null)
        {
            return Json(new ApiResponse<object?>
            {
                Success = true,
                Data = data,
                Message = message
            });
        }

        /// <summary>
        /// 返回失败的 JSON 响应
        /// </summary>
        protected JsonResult ApiFail(string message, string? errorCode = null)
        {
            return Json(new ApiResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            });
        }

        /// <summary>
        /// 返回带分页信息的成功 JSON 响应
        /// </summary>
        protected JsonResult ApiOkPaged<T>(PagedList<T> pagedList)
        {
            return Json(new
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
