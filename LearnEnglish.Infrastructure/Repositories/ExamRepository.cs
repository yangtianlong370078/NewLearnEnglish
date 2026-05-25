using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 考试 Repository 实现
    /// </summary>
    public class ExamRepository : DapperRepository<Exam>, IExamRepository
    {
        protected override string TableName => "exam";

        public ExamRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public new async Task<Exam?> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Exam>> GetPagedByUserIdAsync(int userId, int pageIndex, int pageSize)
        {
            const string sql = @"SELECT id, name, userid, count, limittime, createdate 
                FROM `exam` WHERE userId = @UserId 
                ORDER BY createdate DESC 
                LIMIT @Offset, @PageSize";
            return await QueryAsync<Exam>(sql, new
            {
                UserId = userId,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<int> CreateAsync(Exam exam)
        {
            const string sql = @"INSERT INTO `exam` (name, userid, count, limittime, createdate) 
                VALUES (@Name, @UserId, @Count, @LimitTime, @CreateDate)";
            return await InsertAsync(sql, exam);
        }

        public async Task UpdateCountAsync(int examId, int count)
        {
            const string sql = "UPDATE `exam` SET count = @Count WHERE id = @ExamId";
            await ExecuteAsync(sql, new { ExamId = examId, Count = count });
        }

        public new async Task DeleteAsync(int id)
        {
            await base.DeleteAsync(id);
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM `exam` WHERE userId = @UserId";
            var result = await ExecuteScalarAsync<int>(sql, new { UserId = userId });
            return result;
        }

        public async Task<IEnumerable<Exam>> SearchPagedByUserIdAsync(int userId, string? name, int pageIndex, int pageSize)
        {
            var whereSql = "WHERE userId = @UserId";
            if (!string.IsNullOrWhiteSpace(name))
            {
                whereSql += " AND name LIKE CONCAT('%', @Name, '%')";
            }
            var sql = $@"SELECT id, name, userid, count, limittime, createdate 
                FROM `exam` {whereSql}
                ORDER BY createdate DESC 
                LIMIT @Offset, @PageSize";
            return await QueryAsync<Exam>(sql, new
            {
                UserId = userId,
                Name = name,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<int> SearchCountByUserIdAsync(int userId, string? name)
        {
            var whereSql = "WHERE userId = @UserId";
            if (!string.IsNullOrWhiteSpace(name))
            {
                whereSql += " AND name LIKE CONCAT('%', @Name, '%')";
            }
            var sql = $"SELECT COUNT(1) FROM `exam` {whereSql}";
            var result = await ExecuteScalarAsync<int>(sql, new { UserId = userId, Name = name });
            return result;
        }
    }
}
