namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 用户实体
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 登录Id（微信OpenId等）
        /// </summary>
        public string LoginId { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 密码（哈希值）
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 当前课程Id
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// 账户状态（1=正常，2=过期）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 服务开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 服务到期日期
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 密码版本（0=MD5, 1=BCrypt）
        /// </summary>
        public int PasswordVersion { get; set; }
    }
}
