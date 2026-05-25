using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 考试答案 Repository 实现
    /// </summary>
    public class ExamAnswerRepository : DapperRepository<ExamAnswer>, IExamAnswerRepository
    {
        protected override string TableName => "examnswer";

        public ExamAnswerRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<ExamAnswer>> GetByExamIdAsync(int examId)
        {
            const string sql = "SELECT * FROM `examnswer` WHERE examid = @ExamId";
            return await QueryAsync<ExamAnswer>(sql, new { ExamId = examId });
        }

        public async Task<IEnumerable<ExamAnswer>> GetByExamIdAndTypeAsync(int examId, int type)
        {
            const string sql = "SELECT * FROM `examnswer` WHERE examid = @ExamId AND type = @Type";
            return await QueryAsync<ExamAnswer>(sql, new { ExamId = examId, Type = type });
        }

        public async Task<Dictionary<int, int>> CountByExamIdGroupByTypeAsync(int examId)
        {
            const string sql = "SELECT type AS Type, COUNT(id) AS Count FROM `examnswer` WHERE examid = @ExamId GROUP BY type";
            var results = await QueryAsync<(int Type, int Count)>(sql, new { ExamId = examId });
            return results.ToDictionary(r => r.Type, r => r.Count);
        }

        public async Task<int> CreateAsync(ExamAnswer answer)
        {
            const string sql = @"INSERT INTO `examnswer` (examid, examdetailid, type, isok, answer) 
                VALUES (@ExamId, @ExamDetailId, @Type, @IsOk, @Answer)";
            return await InsertAsync(sql, answer);
        }

        public async Task BulkInsertAsync(IEnumerable<ExamAnswer> answers)
        {
            const string sql = @"INSERT INTO `examnswer` (examid, examdetailid, type, isok, answer) 
                VALUES (@ExamId, @ExamDetailId, @Type, @IsOk, @Answer)";
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, answers);
        }

        public async Task DeleteByExamIdAsync(int examId)
        {
            const string sql = "DELETE FROM `examnswer` WHERE examid = @ExamId";
            await ExecuteAsync(sql, new { ExamId = examId });
        }

        public async Task DeleteByExamIdAndTypeAsync(int examId, int type)
        {
            const string sql = "DELETE FROM `examnswer` WHERE examid = @ExamId AND type = @Type";
            await ExecuteAsync(sql, new { ExamId = examId, Type = type });
        }
    }
}
