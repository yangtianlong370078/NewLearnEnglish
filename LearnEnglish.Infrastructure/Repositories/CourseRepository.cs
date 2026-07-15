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
                //sql = @"SELECT ca.id AS Id, ca.name AS Name, co.userId AS UserId, 
                //    co.id AS CourseId, co.name AS CourseName, 
                //    CASE WHEN mc.id IS NOT NULL THEN 1 ELSE 0 END AS IsMyCourse
                //    FROM category ca
                //    LEFT JOIN course co ON ca.id = co.categoryId 
                //        AND co.id IN (SELECT courseid FROM mycourse WHERE userid = @UserId)
                //    LEFT JOIN mycourse mc ON co.id = mc.courseid AND mc.userid = @UserId 
                //    WHERE ca.Type = @type
                //    ORDER BY ca.id, co.id";
                //我的课程
                sql = @"SELECT ca.id AS Id, ca.name AS Name, co.userId AS UserId, 
                    co.id AS CourseId, co.name AS CourseName, 
                    CASE WHEN mc.id IS NOT NULL THEN 1 ELSE 0 END AS IsMyCourse,
                    lc.courseId AS LastCourseId
                    FROM category ca 
                    JOIN course co ON ca.id = co.categoryId and ca.Type = @type
                    JOIN mycourse mc ON co.id = mc.courseid AND mc.userid = @UserId  
                    LEFT JOIN lastcourse lc ON lc.userId = @UserId
                    ORDER BY ca.id, co.id";
            }
            else
            {
                //sql = @"SELECT ca.id AS Id, ca.name AS Name, co.userId AS UserId, 
                //    co.id AS CourseId, co.name AS CourseName, 
                //    CASE WHEN mc.id IS NOT NULL THEN 1 ELSE 0 END AS IsMyCourse
                //    FROM category ca
                //    LEFT JOIN course co ON ca.id = co.categoryId
                //    LEFT JOIN mycourse mc ON co.id = mc.courseid AND mc.userid = @UserId
                //    WHERE ca.Type = @type
                //    ORDER BY ca.id, co.id";

                sql = @$"SELECT ca.id AS Id, ca.name AS Name, co.userId AS UserId, 
                    co.id AS CourseId, co.name AS CourseName
                    FROM category ca
                     JOIN course co ON ca.id = co.categoryId
										  WHERE ca.Type = @type and ca.id != 9 AND
										  NOT EXISTS (			 
										 select 1 from mycourse mc WHERE co.id = mc.courseid and mc.userid = @UserId
										 )
                    ORDER BY ca.id, co.id";
            }
            return await QueryAsync<Application.Dtos.Course.CategoryDto>(sql, new { UserId = userId ,type });
        }

        public async Task<IEnumerable<Application.Dtos.Course.CourseCountDto>> GetDoneCountsAsync(int userId, int status)
        {
           
            var sql = $@"SELECT cc.courseId AS CourseId, COUNT(DISTINCT ml.lexiconId) AS Count
                FROM coursecontent cc
                JOIN lexicon t2 ON cc.lexiconId = t2.id
                JOIN mylexicon ml ON cc.lexiconId = ml.lexiconId AND ml.userId = @UserId AND ml.status = @status
				JOIN mycourse mc on cc.courseId  = mc.courseid and mc.userid = @UserId
                GROUP BY cc.courseId";
            return await QueryAsync<Application.Dtos.Course.CourseCountDto>(sql, new { UserId = userId , status });
        }

        public async Task<IEnumerable<Application.Dtos.Course.CourseCountDto>> GetUndoneCountsAsync(int userId)
        {
           
            var sql = $@"SELECT cc.courseId AS CourseId, COUNT(*) AS Count
                FROM coursecontent cc 
                JOIN lexicon t2 ON cc.lexiconId = t2.id
				JOIN mycourse mc on cc.courseId  = mc.courseid and mc.userid = @UserId
                GROUP BY cc.courseId";
            return await QueryAsync<Application.Dtos.Course.CourseCountDto>(sql, new { UserId = userId });
        }


        public async Task<IEnumerable<Application.Dtos.Course.CourseCountDto>> GetCourseCountsAsync(List<int> courseIds)
        {
            if (courseIds == null || courseIds.Count == 0)
            {
                return Enumerable.Empty<Application.Dtos.Course.CourseCountDto>();
            }

            const string sql = @"SELECT cc.courseId AS CourseId, COUNT(*) AS Count
                FROM coursecontent cc 
                WHERE cc.courseId IN @courseIds
                GROUP BY cc.courseId;";
            return await QueryAsync<Application.Dtos.Course.CourseCountDto>(sql, new { courseIds });
        }
    }
}
