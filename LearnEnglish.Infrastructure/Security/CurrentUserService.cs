using System.Security.Claims;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LearnEnglish.Infrastructure.Security
{
    /// <summary>
    /// 当前用户上下文服务实现
    /// 从 JWT Claims 中提取当前登录用户信息
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取当前登录用户Id
        /// </summary>
        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("userid");
                return userIdClaim != null ? int.Parse(userIdClaim) : null;
            }
        }

        /// <summary>
        /// 获取当前登录用户名
        /// </summary>
        public string? UserName
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue("username");
            }
        }

        /// <summary>
        /// 获取当前用户的服务起始日期（来自 JWT Claims）
        /// </summary>
        public DateTime? StartDate
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue("startdate");
                if (string.IsNullOrEmpty(value)) return null;
                return DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
                    ? dt
                    : (DateTime?)null;
            }
        }

        /// <summary>
        /// 获取当前用户信息（包含有效性校验）
        /// 如果未登录或已过期，抛出异常
        /// </summary>
        public User GetValidUser()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedException("请重新登录");

            var userIdStr = httpContext.User.FindFirstValue("userid");
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new InvalidUserException("请重新登录", InvalidUserException.UserErrorType.Unauthorized);
            }

            var user = new User
            {
                Id = int.Parse(userIdStr),
                Name = httpContext.User.FindFirstValue("username") ?? string.Empty,
                CourseId = int.Parse(httpContext.User.FindFirstValue("courseId") ?? "0"),
                Status = int.Parse(httpContext.User.FindFirstValue("status") ?? "0"),
            };

            if (user.Status == 2)
            {
                throw new InvalidUserException("账户已过期", InvalidUserException.UserErrorType.Expired);
            }

            return user;
        }
    }
}
