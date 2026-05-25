using System.Data;

namespace LearnEnglish.Infrastructure.Data
{
    /// <summary>
    /// 数据库连接工厂接口
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// 创建新的数据库连接
        /// </summary>
        IDbConnection CreateConnection();
    }
}
