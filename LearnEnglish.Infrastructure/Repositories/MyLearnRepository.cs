using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

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
                WHERE userId = @UserId AND updatedate >= @StartDate 
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
            const string sql = @"INSERT IGNORE INTO `mylearn` (lexiconId, userId, updatetime, updatedate) 
                VALUES (@LexiconId, @UserId, @UpdateTime, @UpdateDate)";
            await ExecuteAsync(sql, myLearn);
        }

        public async Task DeleteByUserAndLexiconAsync(int userId, int lexiconId)
        {
            const string sql = "DELETE FROM `mylearn` WHERE userId = @UserId AND lexiconId = @LexiconId";
            await ExecuteAsync(sql, new { UserId = userId, LexiconId = lexiconId });
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
