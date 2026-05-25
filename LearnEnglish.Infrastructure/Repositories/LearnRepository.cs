using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 学习记录 Repository 实现
    /// </summary>
    public class LearnRepository : DapperRepository<Learn>, ILearnRepository
    {
        protected override string TableName => "learn";

        public LearnRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public new async Task<Learn?> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Learn>> GetByUserIdAndCourseContentIdsAsync(int userId, IEnumerable<int> courseContentIds)
        {
            const string sql = @"SELECT * FROM `learn` 
                WHERE userId = @UserId AND coursecontentId IN @CourseContentIds";
            return await QueryAsync<Learn>(sql, new { UserId = userId, CourseContentIds = courseContentIds });
        }

        public async Task<int> CreateAsync(Learn learn)
        {
            const string sql = @"INSERT INTO `learn` 
                (coursecontentId, status, number, userId, iscollect, createdate, updatetime, updatedate) 
                VALUES (@CourseContentId, @Status, @Number, @UserId, @IsCollect, @CreateDate, @UpdateTime, @UpdateDate)";
            return await InsertAsync(sql, learn);
        }

        public async Task UpdateStatusAsync(int id, int status)
        {
            const string sql = "UPDATE `learn` SET status = @Status, updatetime = @UpdateTime WHERE id = @Id";
            await ExecuteAsync(sql, new { Id = id, Status = status, UpdateTime = DateTime.Now });
        }

        public async Task UpdateCollectAsync(int id, int isCollect)
        {
            const string sql = "UPDATE `learn` SET iscollect = @IsCollect WHERE id = @Id";
            await ExecuteAsync(sql, new { Id = id, IsCollect = isCollect });
        }

        public async Task UpdateNumberAsync(int id, int number)
        {
            const string sql = "UPDATE `learn` SET number = @Number, updatetime = @UpdateTime WHERE id = @Id";
            await ExecuteAsync(sql, new { Id = id, Number = number, UpdateTime = DateTime.Now });
        }

        public async Task DeleteByCourseContentIdAsync(int courseContentId)
        {
            const string sql = "DELETE FROM `learn` WHERE coursecontentId = @CourseContentId";
            await ExecuteAsync(sql, new { CourseContentId = courseContentId });
        }

        public async Task DeleteByCourseContentIdsAsync(IEnumerable<int> courseContentIds)
        {
            const string sql = "DELETE FROM `learn` WHERE coursecontentId IN @CourseContentIds";
            await ExecuteAsync(sql, new { CourseContentIds = courseContentIds });
        }
    }
}
