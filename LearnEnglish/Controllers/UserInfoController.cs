using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnEnglish.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInfoController : ControllerBase
    {
        [HttpGet]
        [Authorize] // 确保请求已认证  
        public IActionResult GetUserInfo()
        {
            // 从User属性中获取当前用户的ClaimsPrincipal  
            var claimsPrincipal = User;

            // 检查用户是否已认证  
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                // 尝试从ClaimsPrincipal中提取用户名（这取决于你的JWT中包含了哪些声明）  
                var usernameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name);
                if (usernameClaim != null)
                {
                    var username = usernameClaim.Value;
                    return Ok(new { username });
                }
                else
                {
                    // 如果没有找到用户名声明，返回错误或默认用户名  
                    return Ok(new { username = "Unknown" });
                }
            }
            else
            {
                // 如果用户未认证，返回未授权错误  
                return Forbid();
            }
        }
    }
}
