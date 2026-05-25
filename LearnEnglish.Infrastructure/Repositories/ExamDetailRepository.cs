using Dapper;
using LearnEnglish.Application.Dtos.Exam;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 考试详情 Repository 实现
    /// </summary>
    public class ExamDetailRepository : DapperRepository<ExamDetail>, IExamDetailRepository
    {
        protected override string TableName => "examdetail";

        public ExamDetailRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public new async Task<ExamDetail?> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ExamDetail>> GetByExamIdAsync(int examId)
        {
            const string sql = @"SELECT ed.*, l.en, l.cn 
                FROM `examdetail` ed 
                INNER JOIN `lexicon` l ON ed.lexiconId = l.id 
                WHERE ed.examid = @ExamId";
            return await QueryAsync<ExamDetail>(sql, new { ExamId = examId });
        }

        public async Task BulkInsertAsync(IEnumerable<ExamDetail> details)
        {
            const string sql = @"INSERT INTO `examdetail` (examid, learnid, lexiconId) 
                VALUES (@ExamId, @LearnId, @LexiconId)";
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, details);
        }

        public async Task DeleteByExamIdAsync(int examId)
        {
            const string sql = "DELETE FROM `examdetail` WHERE examid = @ExamId";
            await ExecuteAsync(sql, new { ExamId = examId });
        }

        public async Task<IEnumerable<ExamDetail>> SelectWordsForExamAsync(int userId, int count)
        {
            // 从已掌握(status=3)的 mylexicon 中选取今日未考过的单词
            // 按 numbersum 升序（优先选择练习少的）、frequency 降序排序
            const string sql = @"SELECT t.id AS LearnId, t.lexiconId AS LexiconId 
                FROM `mylexicon` t
                INNER JOIN `lexicon` t2 ON t.lexiconId = t2.id
                WHERE t.userId = @UserId AND t.status = 3 
                AND NOT EXISTS (
                    SELECT 1 FROM `examdetail` c 
                    INNER JOIN `exam` b ON c.examid = b.id
                    WHERE c.learnid = t.id AND b.createdate > @Today
                )
                ORDER BY t.numbersum ASC, t2.frequency DESC 
                LIMIT @Count";
            return await QueryAsync<ExamDetail>(sql, new { UserId = userId, Count = count, Today = DateTime.Now.Date });
        }

        public async Task<IEnumerable<ExamContentQueryDto>> GetExamContentWithDetailsAsync(int userId, int examId, int type)
        {
            // 联表查询考试内容：examdetail + lexicon + examnswer + mylexicon
            var sql = $@"SELECT 
                t1.id AS Id,
                t1.learnid AS LearnId,
                t2.isenaudio AS IsEnAudio,
                t2.isusaudio AS IsUsAudio,
                t1.lexiconid AS LexiconId,
                t2.en AS En,
                COALESCE(t4.cn, t2.cn) AS Cn,
                t1.examid AS ExamId,
                t3.type AS Type,
                CASE WHEN t3.isok IS NOT NULL THEN 0 ELSE 1 END AS IsOk,
                CASE WHEN t3.isok IS NOT NULL THEN t3.answer ELSE CASE WHEN @Type = 2 THEN t2.cn ELSE t2.en END END AS Answer
                FROM `examdetail` t1
                INNER JOIN `lexicon` t2 ON t1.lexiconId = t2.id 
                LEFT JOIN `examnswer` t3 ON t3.examdetailid = t1.id AND t3.type = @Type
                LEFT JOIN `mylexicon` t4 ON t4.lexiconId = t2.id AND t4.userId = @UserId 
                WHERE t1.examid = @ExamId";
            return await QueryAsync<ExamContentQueryDto>(sql, new { UserId = userId, ExamId = examId, Type = type });
        }
    }
}
