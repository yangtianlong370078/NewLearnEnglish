using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 用户课程关联 Repository 实现
    /// </summary>
    public class MyCourseRepository : DapperRepository<MyCourse>, IMyCourseRepository
    {
        protected override string TableName => "mycourse";

        public MyCourseRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<int> CreateAsync(MyCourse myCourse)
        {
            const string sql = @"INSERT INTO `mycourse` (courseid, userid, createdate) 
                VALUES (@CourseId, @UserId, @CreateDate)";
            return await InsertAsync(sql, myCourse);
        }

        public async Task DeleteByUserAndCourseAsync(int userId, int courseId)
        {
            const string sql = "DELETE FROM `mycourse` WHERE userid = @UserId AND courseid = @CourseId";
            await ExecuteAsync(sql, new { UserId = userId, CourseId = courseId });
        }

        public async Task<bool> ExistsAsync(int userId, int courseId)
        {
            const string sql = "SELECT COUNT(1) FROM `mycourse` WHERE userid = @UserId AND courseid = @CourseId";
            var count = await ExecuteScalarAsync<int>(sql, new { UserId = userId, CourseId = courseId });
            return count > 0;
        }

        public async Task DeleteByCourseIdAsync(int courseId)
        {
            const string sql = "DELETE FROM `mycourse` WHERE courseid = @CourseId";
            await ExecuteAsync(sql, new { CourseId = courseId });
        }
    }
}
