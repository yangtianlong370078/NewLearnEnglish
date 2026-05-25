using System.Security.Cryptography;
using System.Text;
using LearnEnglish.Application.Interfaces;

namespace LearnEnglish.Infrastructure.Security
{
    /// <summary>
    /// 密码哈希服务实现
    /// 支持 BCrypt（新密码）和 MD5（旧密码兼容验证）
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// 使用 BCrypt 哈希密码（用于新注册和密码迁移）
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// 验证密码，根据 passwordVersion 自动选择验证方式：
        /// - Version 0: MD5 验证（旧用户）
        /// - Version 1: BCrypt 验证（新用户/已迁移用户）
        /// </summary>
        public bool VerifyPassword(string password, string hash, int passwordVersion)
        {
            return passwordVersion switch
            {
                0 => VerifyMd5(password, hash),
                1 => VerifyBCrypt(password, hash),
                _ => false
            };
        }

        /// <summary>
        /// 计算 MD5 哈希（仅用于旧密码兼容验证）
        /// </summary>
        public string ComputeMd5Hash(string input)
        {
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes);
        }

        private bool VerifyMd5(string password, string hash)
        {
            var computed = ComputeMd5Hash(password);
            return string.Equals(computed, hash, StringComparison.OrdinalIgnoreCase);
        }

        private static bool VerifyBCrypt(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}
