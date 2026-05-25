using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Models.ErrorHelper
{
    public class ErrorHandlerService
    {
        public IActionResult HandleInvalidUserViewModel(InvalidUserException ex)
        {
            // 返回一个包含错误信息的ViewModel，供控制器处理
            return new JsonResult(new { Error = ex.Message, ErrorCode = 2 }) { StatusCode = StatusCodes.Status403Forbidden };
        }

        public IActionResult HandleInvalidUserException(InvalidUserException ex)
        {
            // 返回一个包含错误信息的ViewModel，用于传递错误信息给控制器处理
            var viewModel = new UserErrorViewModel { Msg = ex.Message };
            if (viewModel.Msg.Equals("账户已过期"))
            {
                viewModel.Type = 1;
            }
            else
            {
                viewModel.Type = 2;
            }

            return new ObjectResult(viewModel) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
