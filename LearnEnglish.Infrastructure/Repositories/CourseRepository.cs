using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 课程 Repository 实现
    /// </summary>
    public class CourseRepository : DapperRepository<Course>, ICourseRepository
    {
        protected override string TableName => "course";

        public CourseRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public new async Task<Course?> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public new async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task<IEnumerable<Course>> GetByUserIdAsync(int userId)
        {
            const string sql = "SELECT * FROM `course` WHERE userId = @UserId";
            return await QueryAsync<Course>(sql, new { UserId = userId });
        }

        public async Task<int> CreateAsync(Course course)
        {
            const string sql = @"INSERT INTO `course` (name, userId, categoryId, createdate) 
                VALUES (@Name, @UserId, @CategoryId, @CreateDate)";
            return await InsertAsync(sql, course);
        }

        public async Task UpdateNameAsync(int id, string name)
        {
            const string sql = "UPDATE `course` SET name = @Name WHERE id = @Id";
            await ExecuteAsync(sql, new { Id = id, Name = name });
        }

        public new async Task DeleteAsync(int id)
        {
            await base.DeleteAsync(id);
        }

        public async Task DeleteContentByCourseIdAsync(int courseId)
        {
            const string sql = "DELETE FROM `coursecontent` WHERE courseId = @CourseId";
            await ExecuteAsync(sql, new { CourseId = courseId });
        }

        public async Task DeleteLearnByCourseIdAsync(int courseId)
        {
            const string sql = @"DELETE FROM `learn` 
                WHERE coursecontentId IN (SELECT id FROM `coursecontent` WHERE courseId = @CourseId)";
            await ExecuteAsync(sql, new { CourseId = courseId });
        }

        public async Task<IEnumerable<Application.Dtos.Course.CategoryDto>> GetCategoriesWithCoursesAsync(int userId, int type, bool onlyMy)
        {
            string sql;
            if (onlyMy)
            {
                sql = @"SELECT ca.id AS Id, ca.name AS Name, co.userId AS UserId, 
                    co.id AS CourseId, co.name AS CourseName, 
                    CASE WHEN mc.id IS NOT NULL THEN 1 ELSE 0 END AS IsMyCourse
                    FROM category ca
                    LEFT JOIN course co ON ca.id = co.categoryId 
                        AND co.id IN (SELECT courseid FROM mycourse WHERE userid = @UserId)
                    LEFT JOIN mycourse mc ON co.id = mc.courseid AND mc.userid = @UserId
                    ORDER BY ca.id, co.id";
            }
            else
            {
                sql = @"SELECT ca.id AS Id, ca.name AS Name, co.userId AS UserId, 
                    co.id AS CourseId, co.name AS CourseName, 
                    CASE WHEN mc.id IS NOT NULL THEN 1 ELSE 0 END AS IsMyCourse
                    FROM category ca
                    LEFT JOIN course co ON ca.id = co.categoryId
                    LEFT JOIN mycourse mc ON co.id = mc.courseid AND mc.userid = @UserId
                    ORDER BY ca.id, co.id";
            }
            return await QueryAsync<Application.Dtos.Course.CategoryDto>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Application.Dtos.Course.CourseCountDto>> GetDoneCountsAsync(int userId, bool onlyMyCourse)
        {
            var filterSql = onlyMyCourse 
                ? "AND cc.courseId IN (SELECT courseid FROM mycourse WHERE userid = @UserId)" 
                : "";
            var sql = $@"SELECT cc.courseId AS CourseId, COUNT(DISTINCT ml.lexiconId) AS Count
                FROM coursecontent cc
                INNER JOIN mylexicon ml ON cc.lexiconId = ml.lexiconId AND ml.userId = @UserId AND ml.status = 3
                WHERE 1=1 {filterSql}
                GROUP BY cc.courseId";
            return await QueryAsync<Application.Dtos.Course.CourseCountDto>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Application.Dtos.Course.CourseCountDto>> GetUndoneCountsAsync(int userId, bool onlyMyCourse)
        {
            var filterSql = onlyMyCourse 
                ? "AND cc.courseId IN (SELECT courseid FROM mycourse WHERE userid = @UserId)" 
                : "";
            var sql = $@"SELECT cc.courseId AS CourseId, COUNT(*) AS Count
                FROM coursecontent cc
                WHERE 1=1 {filterSql}
                GROUP BY cc.courseId";
            return await QueryAsync<Application.Dtos.Course.CourseCountDto>(sql, new { UserId = userId });
        }
    }
}
