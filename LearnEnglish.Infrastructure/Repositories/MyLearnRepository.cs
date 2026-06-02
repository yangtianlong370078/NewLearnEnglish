using System.Transactions;

using Dapper;

using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

using MongoDB.Driver;

using MySql.Data.MySqlClient;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 学习统计 Repository 实现
    /// </summary>
    public class MyLearnRepository : DapperRepository<MyLearn>, IMyLearnRepository
    {
        protected override string TableName => "mylearn";

        public MyLearnRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<int> GetCountByUserIdSinceDateAsync(int userId, DateTime startDate)
        {
            const string sql = @"SELECT COUNT(1) FROM `mylearn` 
                WHERE userId = @UserId AND updatedate >= @StartDate";
            var result = await ExecuteScalarAsync<int>(sql, new { UserId = userId, StartDate = startDate });
            return result;
        }

        public async Task<IEnumerable<(DateTime Date, int Count)>> GetDailyCountByUserIdAsync(
            int userId, DateTime startDate)
        {
            const string sql = @"SELECT updatedate AS `Date`, COUNT(1) AS `Count` 
                FROM `mylearn` 
                WHERE userId = @UserId AND updatedate > @StartDate 
                GROUP BY updatedate 
                ORDER BY updatedate";
            return await QueryAsync<(DateTime Date, int Count)>(sql,
                new { UserId = userId, StartDate = startDate });
        }

        public async Task<IEnumerable<(DateTime Date, int Count)>> GetDailyCountByUserIdAndDatesAsync(
            int userId, IEnumerable<DateTime> dates)
        {
            const string sql = @"SELECT updatedate AS `Date`, COUNT(1) AS `Count` 
                FROM `mylearn` 
                WHERE userId = @UserId AND updatedate IN @Dates 
                GROUP BY updatedate";
            return await QueryAsync<(DateTime Date, int Count)>(sql, new { UserId = userId, Dates = dates });
        }

        public async Task InsertIgnoreAsync(MyLearn myLearn)
        {
           
            const string sql = @"INSERT IGNORE INTO `mylearn` (lexiconId, userId, updatetime) 
                VALUES (@LexiconId, @UserId, @UpdateTime)";
            await ExecuteAsync(sql, myLearn);
        }

        public async Task DeleteByUserAndLexiconAsync(int userId, int lexiconId)
        {
            const string sql = "DELETE FROM `mylearn` WHERE userId = @UserId AND lexiconId = @LexiconId";
            await ExecuteAsync(sql, new { UserId = userId, LexiconId = lexiconId });
        }

        /// <summary>
        /// 查询此单词之前在哪一天学习过
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lexiconId"></param>
        /// <returns></returns>
        public async Task<DateTime?> QueryByUserAndLexiconAsync(int userId, int lexiconId)
        {
            const string sql = "SELECT updatedate  FROM `mylearn` WHERE userId = @UserId AND lexiconId = @LexiconId";
            return await QueryFirstOrDefaultAsync< DateTime?> (sql, new { UserId = userId, LexiconId = lexiconId });
        }

        public async Task BulkInsertIgnoreAsync(IEnumerable<MyLearn> records)
        {
            const string sql = @"INSERT IGNORE INTO `mylearn` (lexiconId, userId, updatetime, updatedate) 
                VALUES (@LexiconId, @UserId, @UpdateTime, @UpdateDate)";
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, records);
        }

        public async Task BulkDeleteByUserAndLexiconIdsAsync(int userId, IEnumerable<int> lexiconIds)
        {
            const string sql = "DELETE FROM `mylearn` WHERE userId = @UserId AND lexiconId IN @LexiconIds";
            await ExecuteAsync(sql, new { UserId = userId, LexiconIds = lexiconIds });
        }
    }
}
