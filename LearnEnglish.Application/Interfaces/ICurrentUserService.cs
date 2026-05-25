using LearnEnglish.Domain.Entities;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 当前用户上下文服务接口
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// 获取当前登录用户Id
        /// </summary>
        int? UserId { get; }

        /// <summary>
        /// 获取当前登录用户名
        /// </summary>
        string? UserName { get; }

        /// <summary>
        /// 获取当前用户信息（包含有效性校验）
        /// </summary>
        User GetValidUser();
    }
}
