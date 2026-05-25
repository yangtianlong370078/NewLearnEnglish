using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LearnEnglish.Models.ErrorHelper
{
    /// <summary>
    /// 无效用户异常过滤器：处理 InvalidUserException 并重定向到对应页面
    /// <para>账户过期 → 重定向到 /Error/UserError</para>
    /// <para>未登录/Token失效 → 重定向到 /Login/Login</para>
    /// </summary>
    public class InvalidUserExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ErrorHandlerService _errorHandlerService;

        public InvalidUserExceptionFilter(ErrorHandlerService errorHandlerService)
        {
            _errorHandlerService = errorHandlerService;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is InvalidUserException ex)
            {
                IActionResult result = _errorHandlerService.HandleInvalidUserException(ex);

                if (result is RedirectToActionResult redirectToActionResult)
                {
                    context.Result = redirectToActionResult;
                }
                else if (result is ObjectResult objectResult && objectResult.Value is UserErrorViewModel model)
                {
                    if (model.Type == 2)
                    {
                        // 未登录/Token失效 → 重定向到登录页
                        context.Result = new RedirectToActionResult("Login", "Login", null);
                    }
                    else
                    {
                        // 账户过期等 → 重定向到错误页面
                        var routeValues = new Microsoft.AspNetCore.Routing.RouteValueDictionary
                        {
                            { "msg", model.Msg },
                            { "type", model.Type }
                        };
                        context.Result = new RedirectToActionResult("UserError", "Error", routeValues);
                    }
                }

                context.ExceptionHandled = true;
            }
        }
    }
}
