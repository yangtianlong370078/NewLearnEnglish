using System.Data;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace LearnEnglish.Infrastructure.Data
{
    /// <summary>
    /// MySQL 数据库连接工厂
    /// </summary>
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IOptions<DatabaseOptions> options)
        {
            _connectionString = options.Value.GetConnectionString();
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
