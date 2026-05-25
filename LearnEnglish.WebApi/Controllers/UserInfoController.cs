using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 用户身份信息接口
    /// </summary>
    [Route("api/[controller]")]
    public class UserInfoController : ApiControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            var claimsPrincipal = User;
            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                var usernameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name);
                var username = usernameClaim?.Value ?? "Unknown";
                return Ok(new { username });
            }
            return Forbid();
        }
    }
}
