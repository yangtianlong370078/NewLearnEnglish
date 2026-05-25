using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 用户 Repository 实现
    /// </summary>
    public class UserRepository : DapperRepository<User>, IUserRepository
    {
        protected override string TableName => "users";

        public UserRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<User?> GetByNameAsync(string name)
        {
            const string sql = "SELECT * FROM `users` WHERE name = @Name LIMIT 1";
            return await QueryFirstOrDefaultAsync<User>(sql, new { Name = name });
        }

        public async Task<User?> GetByLoginIdAsync(string loginId)
        {
            const string sql = "SELECT * FROM `users` WHERE loginid = @LoginId LIMIT 1";
            return await QueryFirstOrDefaultAsync<User>(sql, new { LoginId = loginId });
        }

        public async Task<int> CreateAsync(User user)
        {
            const string sql = @"INSERT INTO `users` 
                (name, age, loginid, phone, password, courseId, status, startdate, enddate, passwordversion) 
                VALUES (@Name, @Age, @LoginId, @Phone, @Password, @CourseId, @Status, @StartDate, @EndDate, @PasswordVersion)";
            return await InsertAsync(sql, user);
        }

        public async Task UpdateAsync(User user)
        {
            const string sql = @"UPDATE `users` SET 
                name = @Name, age = @Age, loginid = @LoginId, phone = @Phone, 
                password = @Password, courseId = @CourseId, status = @Status, 
                startdate = @StartDate, enddate = @EndDate, passwordversion = @PasswordVersion 
                WHERE id = @Id";
            await ExecuteAsync(sql, user);
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash, int passwordVersion)
        {
            try
            {
                const string sql = "UPDATE `users` SET password = @Password, passwordversion = @PasswordVersion WHERE id = @UserId";
                await ExecuteAsync(sql, new { UserId = userId, Password = passwordHash, PasswordVersion = passwordVersion });
            }
            catch (Exception e)
            {

                throw e;
            }
          
        }

        public async Task UpdateCourseAsync(int userId, int courseId)
        {
            const string sql = "UPDATE `users` SET courseId = @CourseId WHERE id = @UserId";
            await ExecuteAsync(sql, new { UserId = userId, CourseId = courseId });
        }
    }
}
