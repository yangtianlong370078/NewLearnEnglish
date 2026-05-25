using LearnEnglish.Application.Dtos.Auth;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Exceptions;
using LearnEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 登录控制器（重构版，使用 IAuthService + BCrypt）
    /// </summary>
    public class LoginController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public LoginController(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// 微信登录页面
        /// </summary>
        public IActionResult Index()
        {
            var appid = WXApi.AppID;
            var timestamp = WXApi.GetTimestamp().ToString();
            var nonceStr = WXApi.GenerateNonceStr();
            var token = WXApi.GetToken();

            ViewData["appId"] = appid;
            ViewData["timestamp"] = timestamp;
            ViewData["nonceStr"] = nonceStr;

            var jsApiTicket = WXApi.GetJsApiTicket(token);
            var signature = WXApi.GetSignature(
                "8888888888888888888888888888888888888888888888888888888888",
                jsApiTicket, nonceStr, timestamp);

            ViewData["signature"] = signature;
            return View();
        }

        /// <summary>
        /// 获取微信 OpenId
        /// </summary>
        public JsonResult GetOpenId(string code)
        {
            var openid = WXApi.GetOpenID(code);
            return Json(new { msg = "操作成功", succss = true, daea = openid });
        }

        /// <summary>
        /// 登录页面
        /// </summary>
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 注册页面
        /// </summary>
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// 用户注册（使用 BCrypt 加密，通过 IAuthService 处理）
        /// </summary>
        public async Task<JsonResult> SetRegister(string loginID, string password, string phone, string name)
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
                return Json(new { success = true, msg = "注册成功，现在就开始学习吧！" });
            }
            catch (ValidationException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, msg = "注册失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 修改密码（使用 BCrypt，支持 MD5 旧密码兼容验证）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> ModifyPassword(string OldPwd, string NewPwd)
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
                return Json(new { success = true });
            }
            catch (ValidationException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, msg = "修改密码失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 用户登录（使用 BCrypt 验证，支持 MD5 旧密码自动迁移）
        /// </summary>
        public async Task<IActionResult> LoginResult(string loginID, string password, int sb)
        {
            try
            {
                var request = new LoginRequestDto
                {
                    Name = loginID,
                    Password = password
                };

                var result = await _authService.LoginAsync(request);

                // 从 JWT Token 解析用户信息，保持前端兼容（前端需要 user.id 和 user.courseId）
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

                var user = new Models.User
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

                return Json(new
                {
                    token = result.AccessToken,
                    msg = string.Empty,
                    user = user,
                    success = true
                });
            }
            catch (NotFoundException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, msg = "登录失败，请稍后重试" });
            }
        }
    }
}
