using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 学习任务 Repository 实现
    /// </summary>
    public class LearnTaskRepository : DapperRepository<LearnTask>, ILearnTaskRepository
    {
        protected override string TableName => "learntask";

        public LearnTaskRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<LearnTask?> GetByUserIdAndMonthAsync(int userId, int year, int month)
        {
            const string sql = @"SELECT * FROM `learntask` 
                WHERE userid = @UserId AND YEAR(startdate) = @Year AND MONTH(startdate) = @Month 
                LIMIT 1";
            return await QueryFirstOrDefaultAsync<LearnTask>(sql,
                new { UserId = userId, Year = year, Month = month });
        }

        public async Task<IEnumerable<LearnTask>> GetByUserIdSinceDateAsync(int userId, DateTime startDate)
        {
            const string sql = @"SELECT * FROM `learntask` 
                WHERE userid = @UserId AND startdate >= @StartDate 
                ORDER BY startdate";
            return await QueryAsync<LearnTask>(sql, new { UserId = userId, StartDate = startDate });
        }

        public async Task<int> CreateAsync(LearnTask task)
        {
            const string sql = @"INSERT INTO `learntask` (userid, count, startdate, enddate, weekend) 
                VALUES (@UserId, @Count, @StartDate, @EndDate, @Weekend)";
            return await InsertAsync(sql, task);
        }

        public async Task UpdateAsync(LearnTask task)
        {
            const string sql = @"UPDATE `learntask` SET 
                count = @Count, startdate = @StartDate, enddate = @EndDate, weekend = @Weekend 
                WHERE id = @Id";
            await ExecuteAsync(sql, task);
        }
    }
}
