using LearnEnglish.Application.Dtos.Auth;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// API 认证控制器（提供 JSON API 形式的登录接口）
    /// <para>与 LoginController（MVC 视图登录）共存，面向前端 AJAX / 移动端调用</para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 用户登录（API 接口）
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(new { success = true, token = result.AccessToken, refreshToken = result.RefreshToken, expiresAt = result.ExpiresAt });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 刷新 Token（API 接口）
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);
                return Ok(new { success = true, token = result.AccessToken, refreshToken = result.RefreshToken, expiresAt = result.ExpiresAt });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }
    }
}
