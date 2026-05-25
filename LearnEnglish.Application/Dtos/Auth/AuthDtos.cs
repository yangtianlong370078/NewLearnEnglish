namespace LearnEnglish.Application.Dtos.Auth
{
    /// <summary>
    /// 登录请求DTO
    /// </summary>
    public class LoginRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录响应DTO
    /// </summary>
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// 注册请求DTO
    /// </summary>
    public class RegisterRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string LoginID { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    /// <summary>
    /// 修改密码请求DTO
    /// </summary>
    public class ChangePasswordRequestDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token 刷新请求DTO
    /// </summary>
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// 当前用户信息DTO
    /// </summary>
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int Status { get; set; }
    }
}
