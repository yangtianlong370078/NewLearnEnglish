using LearnEnglish.Application.Dtos.Auth;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Exceptions;
using LearnEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 登录/注册 API（对应原 MVC LoginController 的接口部分）
    /// </summary>
    [Route("api/[controller]")]
    public class LoginController : ApiControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public LoginController(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        /// <summary>获取微信 OpenId</summary>
        [HttpGet("GetOpenId")]
        public IActionResult GetOpenId(string code)
        {
            var openid = WXApi.GetOpenID(code);
            return Ok(new { msg = "操作成功", succss = true, daea = openid });
        }

        /// <summary>用户注册</summary>
        [HttpPost("SetRegister")]
        public async Task<IActionResult> SetRegister([FromForm] string loginID, [FromForm] string password, [FromForm] string? phone, [FromForm] string name)
        {
            try
            {
                var request = new RegisterRequestDto
                {
                    Name = name,
                    LoginID = loginID,
                    Password = password,
                    Phone = phone ?? string.Empty
                };

                await _authService.RegisterAsync(request);
                return Ok(new { success = true, msg = "注册成功，现在就开始学习吧！" });
            }
            catch (ValidationException ex)
            {
                return Ok(new { success = false, msg = ex.Message });
            }
            catch (Exception)
            {
                return Ok(new { success = false, msg = "注册失败，请稍后重试" });
            }
        }

        /// <summary>修改密码</summary>
        [HttpPost("ModifyPassword")]
        [Authorize]
        public async Task<IActionResult> ModifyPassword([FromForm] string OldPwd, [FromForm] string NewPwd)
        {
            try
            {
                var userId = _currentUserService.UserId
                    ?? throw new UnauthorizedAccessException("用户未登录");

                var request = new ChangePasswordRequestDto
                {
                    OldPassword = OldPwd,
                    NewPassword = NewPwd
                };

                await _authService.ChangePasswordAsync(userId, request);
                return Ok(new { success = true });
            }
            catch (ValidationException ex)
            {
                return Ok(new { success = false, msg = ex.Message });
            }
            catch (Exception)
            {
                return Ok(new { success = false, msg = "修改密码失败，请稍后重试" });
            }
        }

        /// <summary>用户登录（表单/查询参数形式，兼容旧前端）</summary>
        [HttpPost("LoginResult")]
        public async Task<IActionResult> LoginResult([FromForm] string loginID, [FromForm] string password, [FromForm] int sb)
        {
            try
            {
                var request = new LoginRequestDto
                {
                    Name = loginID,
                    Password = password
                };

                var result = await _authService.LoginAsync(request);

                // 从 JWT Token 解析用户信息，保持前端兼容
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(result.AccessToken);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userid")?.Value ?? "0";
                var courseIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "courseId")?.Value ?? "0";
                var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "";
                var ageClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "age")?.Value ?? "0";
                var loginidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "loginid")?.Value ?? "";
                var phoneClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "phone")?.Value ?? "";
                var statusClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "status")?.Value ?? "0";
                var startdateClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "startdate")?.Value;
                var enddateClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "enddate")?.Value;

                var user = new User
                {
                    id = int.Parse(userIdClaim),
                    courseId = int.Parse(courseIdClaim),
                    name = usernameClaim,
                    age = int.Parse(ageClaim),
                    loginid = loginidClaim,
                    phone = phoneClaim,
                    status = int.Parse(statusClaim),
                    startdate = startdateClaim != null ? DateTime.Parse(startdateClaim) : DateTime.Now,
                    enddate = enddateClaim != null ? DateTime.Parse(enddateClaim) : DateTime.Now
                };

                return Ok(new
                {
                    token = result.AccessToken,
                    msg = string.Empty,
                    user,
                    success = true
                });
            }
            catch (NotFoundException ex)
            {
                return Ok(new { success = false, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Ok(new { success = false, msg = ex.Message });
            }
            catch (Exception)
            {
                return Ok(new { success = false, msg = "登录失败，请稍后重试" });
            }
        }
    }
}
