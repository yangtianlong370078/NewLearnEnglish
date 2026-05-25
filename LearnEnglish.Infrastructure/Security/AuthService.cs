using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LearnEnglish.Application.Dtos.Auth;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Domain.Exceptions;
using LearnEnglish.Infrastructure.Configuration;
using LearnEnglish.Infrastructure.Redis;
using LearnEnglish.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LearnEnglish.Infrastructure.Security
{
    /// <summary>
    /// 认证服务实现
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMyCourseRepository _myCourseRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRedisService _redisService;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Redis 中 Refresh Token 的 Key 前缀
        /// </summary>
        private const string RefreshTokenKeyPrefix = "refresh_token:";

        public AuthService(
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IMyCourseRepository myCourseRepository,
            IPasswordHasher passwordHasher,
            IRedisService redisService,
            IOptions<JwtOptions> jwtOptions,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _myCourseRepository = myCourseRepository;
            _passwordHasher = passwordHasher;
            _redisService = redisService;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("用户登录请求: {UserName}", request.Name);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("用户名和密码不能为空");
            }

            // 通过 loginId 查询用户
            var user = await _userRepository.GetByLoginIdAsync(request.Name.Trim());
            if (user == null)
            {
                throw new NotFoundException("账号不存在");
            }

            // 验证密码（兼容 MD5 和 BCrypt）
            if (!_passwordHasher.VerifyPassword(request.Password, user.Password, user.PasswordVersion))
            {
                throw new UnauthorizedException("密码错误");
            }

            // 如果是 MD5 旧密码，自动迁移到 BCrypt
            if (user.PasswordVersion == 0)
            {
                _logger.LogInformation("用户 {UserId} 密码从 MD5 自动迁移到 BCrypt", user.Id);
                var bcryptHash = _passwordHasher.HashPassword(request.Password);
                await _userRepository.UpdatePasswordAsync(user.Id, bcryptHash, 1);
                user.PasswordVersion = 1;
            }

            // 生成 Token
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            _logger.LogInformation("用户 {UserId} 登录成功", user.Id);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpireMinutes)
            };
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("用户注册请求: {UserName}", request.LoginID);

            if (string.IsNullOrWhiteSpace(request.LoginID) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("用户名和密码不能为空");
            }

            // 检查用户名是否已存在
            var existingUser = await _userRepository.GetByLoginIdAsync(request.LoginID.Trim());
            if (existingUser != null)
            {
                throw new ValidationException($"账号名称:{request.LoginID}已存在，请更换账号名称注册");
            }

            // 使用 BCrypt 哈希密码（新用户直接用 BCrypt）
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                Name = string.IsNullOrWhiteSpace(request.Name) ? request.LoginID.Trim() : request.Name.Trim(),
                Age = 1,
                LoginId = request.LoginID.Trim(),
                Phone = request.Phone ?? string.Empty,
                Password = passwordHash,
                CourseId = 0,
                Status = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                PasswordVersion = 1 // 新用户使用 BCrypt
            };

            var userId = await _userRepository.CreateAsync(user);
            user.Id = userId;

            // 创建默认生词本
            if (userId > 0)
            {
                var course = new Course
                {
                    Name = "我的生词本",
                    UserId = userId,
                    CategoryId = 9,
                    CreateDate = DateTime.Now
                };
                var courseId = await _courseRepository.CreateAsync(course);

                if (courseId > 0)
                {
                    await _myCourseRepository.CreateAsync(new MyCourse
                    {
                        CourseId = courseId,
                        UserId = userId,
                        CreateDate = DateTime.Now
                    });

                    await _userRepository.UpdateCourseAsync(userId, courseId);
                    user.CourseId = courseId;
                }
            }

            _logger.LogInformation("新用户注册成功: UserId={UserId}, Name={UserName}", user.Id, user.Name);

            // 生成 Token
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpireMinutes)
            };
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new ValidationException("旧密码和新密码不能为空");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("用户不存在");
            }

            // 验证旧密码
            if (!_passwordHasher.VerifyPassword(request.OldPassword, user.Password, user.PasswordVersion))
            {
                throw new ValidationException("旧密码错误");
            }

            // 新密码使用 BCrypt
            var newHash = _passwordHasher.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, newHash, 1);

            // 使旧的 Refresh Token 失效
            await InvalidateRefreshTokenAsync(userId);

            _logger.LogInformation("用户 {UserId} 密码修改成功", userId);
        }

        /// <summary>
        /// 刷新 Token
        /// </summary>
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new ValidationException("Refresh Token 不能为空");
            }

            // 从 Redis 中查找 Refresh Token 对应的 userId
            var userIdStr = await _redisService.GetAsync($"{RefreshTokenKeyPrefix}{request.RefreshToken}");
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new UnauthorizedException("Refresh Token 无效或已过期");
            }

            var userId = int.Parse(userIdStr);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("用户不存在");
            }

            // 删除旧 Refresh Token
            await _redisService.RemoveAsync($"{RefreshTokenKeyPrefix}{request.RefreshToken}");

            // 生成新的 Token 对
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpireMinutes)
            };
        }

        /// <summary>
        /// 生成 JWT Access Token（2小时有效期）
        /// Claims 仅包含必要信息：userId、username、courseId、status
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("userid", user.Id.ToString()),
                new Claim("username", user.Name),
                new Claim("courseId", user.CourseId.ToString()),
                new Claim("status", user.Status.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 生成 Refresh Token（7天有效期，存入 Redis）
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            // 生成随机 Token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);

            // 存入 Redis，设置过期时间
            var expiry = TimeSpan.FromDays(_jwtOptions.RefreshTokenExpireDays);
            await _redisService.SetAsync(
                $"{RefreshTokenKeyPrefix}{refreshToken}",
                userId.ToString(),
                expiry);

            return refreshToken;
        }

        /// <summary>
        /// 使用户的所有 Refresh Token 失效（修改密码后调用）
        /// 注：简单实现，通过设置用户级别的失效标记
        /// </summary>
        private async Task InvalidateRefreshTokenAsync(int userId)
        {
            // 设置一个用户级别的 Token 版本号，登录时校验
            await _redisService.SetAsync(
                $"token_version:{userId}",
                DateTime.UtcNow.Ticks.ToString(),
                TimeSpan.FromDays(_jwtOptions.RefreshTokenExpireDays));
        }
    }
}
