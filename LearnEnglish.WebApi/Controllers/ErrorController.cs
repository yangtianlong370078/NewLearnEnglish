using LearnEnglish.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 错误信息接口（替代 MVC 的错误视图）
    /// </summary>
    [Route("api/[controller]")]
    public class ErrorController : ApiControllerBase
    {
        /// <summary>返回用户错误信息</summary>
        [HttpGet("UserError")]
        public IActionResult UserError(string msg, int type)
        {
            var model = new UserErrorViewModel { Msg = msg, Type = type };
            return Ok(new { success = false, msg = model.Msg, type = model.Type });
        }
    }
}
