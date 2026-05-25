using LearnEnglish.Application.Dtos.Auth;
using LearnEnglish.Domain.Entities;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// 用户注册
        /// </summary>
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// 修改密码
        /// </summary>
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

        /// <summary>
        /// 刷新 Token
        /// </summary>
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// 生成 JWT Token
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// 生成 Refresh Token
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(int userId);
    }
}
