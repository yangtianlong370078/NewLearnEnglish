using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 考试记录 Repository 实现
    /// </summary>
    public class ExamRecordRepository : DapperRepository<ExamRecord>, IExamRecordRepository
    {
        protected override string TableName => "examrecord";

        public ExamRecordRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<ExamRecord>> GetByExamIdAsync(int examId)
        {
            const string sql = "SELECT * FROM `examrecord` WHERE examid = @ExamId";
            return await QueryAsync<ExamRecord>(sql, new { ExamId = examId });
        }

        public async Task<ExamRecord?> GetByExamIdAndTypeAsync(int examId, int type)
        {
            const string sql = "SELECT * FROM `examrecord` WHERE examid = @ExamId AND type = @Type LIMIT 1";
            return await QueryFirstOrDefaultAsync<ExamRecord>(sql, new { ExamId = examId, Type = type });
        }

        public async Task<int> CreateAsync(ExamRecord record)
        {
            const string sql = @"INSERT INTO `examrecord` (examid, type, limittime, score, createtime) 
                VALUES (@ExamId, @Type, @LimitTime, @Score, @CreateTime)";
            return await InsertAsync(sql, record);
        }

        public async Task DeleteByExamIdAsync(int examId)
        {
            const string sql = "DELETE FROM `examrecord` WHERE examid = @ExamId";
            await ExecuteAsync(sql, new { ExamId = examId });
        }

        public async Task DeleteByExamIdAndTypeAsync(int examId, int type)
        {
            const string sql = "DELETE FROM `examrecord` WHERE examid = @ExamId AND type = @Type";
            await ExecuteAsync(sql, new { ExamId = examId, Type = type });
        }
    }
}
