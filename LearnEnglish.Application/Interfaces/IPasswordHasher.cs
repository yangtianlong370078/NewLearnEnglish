namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 密码哈希服务接口
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// 使用 BCrypt 哈希密码
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// 验证密码（自动兼容 MD5 和 BCrypt）
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="hash">存储的哈希值</param>
        /// <param name="passwordVersion">密码版本（0=MD5, 1=BCrypt）</param>
        /// <returns>是否匹配</returns>
        bool VerifyPassword(string password, string hash, int passwordVersion);

        /// <summary>
        /// 计算 MD5 哈希（仅用于旧密码兼容验证）
        /// </summary>
        string ComputeMd5Hash(string input);
    }
}
