using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 课程内容 Repository 实现
    /// </summary>
    public class CourseContentRepository : DapperRepository<CourseContent>, ICourseContentRepository
    {
        protected override string TableName => "coursecontent";

        public CourseContentRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<CourseContent>> GetByCourseIdAsync(int courseId)
        {
            const string sql = "SELECT * FROM `coursecontent` WHERE courseId = @CourseId";
            return await QueryAsync<CourseContent>(sql, new { CourseId = courseId });
        }

        public async Task<CourseContent?> GetByCourseAndLexiconAsync(int courseId, int lexiconId)
        {
            const string sql = @"SELECT * FROM `coursecontent` 
                WHERE courseId = @CourseId AND lexiconId = @LexiconId LIMIT 1";
            return await QueryFirstOrDefaultAsync<CourseContent>(sql, new { CourseId = courseId, LexiconId = lexiconId });
        }

        public async Task<int> CreateAsync(CourseContent content)
        {
            const string sql = @"INSERT INTO `coursecontent` (courseId, lexiconId, createdate) 
                VALUES (@CourseId, @LexiconId, @CreateDate)";
            return await InsertAsync(sql, content);
        }

        public async Task BulkInsertAsync(IEnumerable<CourseContent> contents)
        {
            const string sql = @"INSERT INTO `coursecontent` (courseId, lexiconId, createdate) 
                VALUES (@CourseId, @LexiconId, @CreateDate)";
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, contents);
        }

        public async Task<int> GetCountByCourseIdAsync(int courseId)
        {
            const string sql = "SELECT COUNT(1) FROM `coursecontent` WHERE courseId = @CourseId";
            var result = await ExecuteScalarAsync<int>(sql, new { CourseId = courseId });
            return result;
        }

        public new async Task DeleteAsync(int id)
        {
            await base.DeleteAsync(id);
        }

        public async Task DeleteByCourseIdAsync(int courseId)
        {
            const string sql = "DELETE FROM `coursecontent` WHERE courseId = @CourseId";
            await ExecuteAsync(sql, new { CourseId = courseId });
        }
    }
}
