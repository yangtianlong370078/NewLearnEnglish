using System.Data;
using Dapper;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// Dapper 泛型 Repository 基类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class DapperRepository<T> where T : class
    {
        protected readonly Data.IDbConnectionFactory _connectionFactory;
        protected abstract string TableName { get; }

        protected DapperRepository(Data.IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 根据Id查询
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"SELECT * FROM `{TableName}` WHERE id = @Id";
            return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
        }

        /// <summary>
        /// 查询所有
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"SELECT * FROM `{TableName}`";
            return await connection.QueryAsync<T>(sql);
        }

        /// <summary>
        /// 插入并返回自增Id
        /// </summary>
        public virtual async Task<int> InsertAsync(string sql, object param)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql + "; SELECT LAST_INSERT_ID();", param);
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        public virtual async Task<int> UpdateAsync(string sql, object param)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, param);
        }

        /// <summary>
        /// 执行删除
        /// </summary>
        public virtual async Task<int> DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"DELETE FROM `{TableName}` WHERE id = @Id";
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        /// <summary>
        /// 查询（通用）
        /// </summary>
        protected async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? param = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<TResult>(sql, param);
        }

        /// <summary>
        /// 查询单条（通用）
        /// </summary>
        protected async Task<TResult?> QueryFirstOrDefaultAsync<TResult>(string sql, object? param = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TResult>(sql, param);
        }

        /// <summary>
        /// 执行命令（通用）
        /// </summary>
        protected async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, param);
        }

        /// <summary>
        /// 查询标量值（通用）
        /// </summary>
        protected async Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object? param = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<TResult>(sql, param);
        }
    }
}
