using Dapper;
using LearnEnglish.Application.Dtos.Word;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 单词列表查询 Repository 实现（复杂联表查询专用）
    /// </summary>
    public class WordQueryRepository : DapperRepository<object>, IWordQueryRepository
    {
        protected override string TableName => "lexicon";

        public WordQueryRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ShowTranslateDto> items, int total, int newCount, int learningCount, int masteredCount)> GetCourseWordsPagedAsync(
            int userId, int courseId, int status, string? name, int pageIndex, int pageSize, string orderBy)
        {
            using var connection = _connectionFactory.CreateConnection();

            // 基础 WHERE 条件
            var whereClause = "WHERE t1.courseId = @CourseId";
            if (!string.IsNullOrWhiteSpace(name))
            {
                whereClause += " AND (t2.en LIKE CONCAT('%', @Name, '%') OR t2.cn LIKE CONCAT('%', @Name, '%') OR COALESCE(t4.cn, t2.cn) LIKE CONCAT('%', @Name, '%'))";
            }

            // 状态过滤
            var statusFilter = status switch
            {
                1 => " AND (t4.status IS NULL OR t4.status = 1)", // 未认识
                2 => " AND t4.status = 2",                        // 未牢记
                3 => " AND t4.status = 3",                        // 已掌握
                _ => ""                                            // 全部
            };
            whereClause += statusFilter;

            // 验证 orderBy 安全性（白名单）
            var safeOrderBy = ValidateOrderBy(orderBy);

            // 查询数据
            var dataSql = $@"SELECT 
                t1.id AS CourseContentId, t2.id AS LexiconId, t2.en AS En, 
                COALESCE(t4.cn, t2.cn) AS Cn, t2.userId AS UserId,
                t2.isenaudio AS IsEnAudio, t2.isusaudio AS IsUsAudio,
                COALESCE(t4.status, 1) AS Zt,
                COALESCE(t4.iscollect, 0) AS IsCollect,
                COALESCE(t4.numbersum, 0) AS NumberSum,
                COALESCE(t4.zynumber, 0) AS ZyNumber,
                COALESCE(t4.yznumber, 0) AS YzNumber,
                COALESCE(t4.txnumber, 0) AS TxNumber,
                COALESCE(t4.fynumber, 0) AS FyNumber
                FROM coursecontent t1
                INNER JOIN lexicon t2 ON t1.lexiconId = t2.id
                LEFT JOIN mylexicon t4 ON t2.id = t4.lexiconId AND t4.userId = @UserId
                {whereClause}
                ORDER BY {safeOrderBy}
                LIMIT @Offset, @PageSize";

            var items = await connection.QueryAsync<ShowTranslateDto>(dataSql, new
            {
                CourseId = courseId,
                UserId = userId,
                Name = name,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            });

            // 查询总数
            var countSql = $@"SELECT COUNT(*) FROM coursecontent t1
                INNER JOIN lexicon t2 ON t1.lexiconId = t2.id
                LEFT JOIN mylexicon t4 ON t2.id = t4.lexiconId AND t4.userId = @UserId
                {whereClause}";

            var total = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                CourseId = courseId,
                UserId = userId,
                Name = name
            });

            // 查询各状态数量
            var statusCountSql = $@"SELECT 
                SUM(CASE WHEN COALESCE(t4.status, 1) = 1 THEN 1 ELSE 0 END) AS NewCount,
                SUM(CASE WHEN t4.status = 2 THEN 1 ELSE 0 END) AS LearningCount,
                SUM(CASE WHEN t4.status = 3 THEN 1 ELSE 0 END) AS MasteredCount
                FROM coursecontent t1
                INNER JOIN lexicon t2 ON t1.lexiconId = t2.id
                LEFT JOIN mylexicon t4 ON t2.id = t4.lexiconId AND t4.userId = @UserId
                WHERE t1.courseId = @CourseId";

            if (!string.IsNullOrWhiteSpace(name))
            {
                statusCountSql += " AND (t2.en LIKE CONCAT('%', @Name, '%') OR t2.cn LIKE CONCAT('%', @Name, '%') OR COALESCE(t4.cn, t2.cn) LIKE CONCAT('%', @Name, '%'))";
            }

            var statusCounts = await connection.QueryFirstOrDefaultAsync<(int NewCount, int LearningCount, int MasteredCount)>(
                statusCountSql, new { CourseId = courseId, UserId = userId, Name = name });

            return (items, total, statusCounts.NewCount, statusCounts.LearningCount, statusCounts.MasteredCount);
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ShowTranslateDto> items, int total)> GetFavoriteWordsPagedAsync(
            int userId, string? name, int pageIndex, int pageSize)
        {
            using var connection = _connectionFactory.CreateConnection();

            var whereClause = "WHERE t4.userId = @UserId AND t4.iscollect = 1";
            if (!string.IsNullOrWhiteSpace(name))
            {
                whereClause += " AND (t2.en LIKE CONCAT('%', @Name, '%') OR t2.cn LIKE CONCAT('%', @Name, '%'))";
            }

            var dataSql = $@"SELECT 
                0 AS CourseContentId, t2.id AS LexiconId, t2.en AS En, 
                COALESCE(t4.cn, t2.cn) AS Cn, t2.userId AS UserId,
                t2.isenaudio AS IsEnAudio, t2.isusaudio AS IsUsAudio,
                COALESCE(t4.status, 1) AS Zt,
                t4.iscollect AS IsCollect,
                COALESCE(t4.numbersum, 0) AS NumberSum,
                COALESCE(t4.zynumber, 0) AS ZyNumber,
                COALESCE(t4.yznumber, 0) AS YzNumber,
                COALESCE(t4.txnumber, 0) AS TxNumber,
                COALESCE(t4.fynumber, 0) AS FyNumber
                FROM mylexicon t4
                INNER JOIN lexicon t2 ON t4.lexiconId = t2.id
                {whereClause}
                ORDER BY t4.updatetime DESC
                LIMIT @Offset, @PageSize";

            var items = await connection.QueryAsync<ShowTranslateDto>(dataSql, new
            {
                UserId = userId,
                Name = name,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            });

            var countSql = $@"SELECT COUNT(*) FROM mylexicon t4
                INNER JOIN lexicon t2 ON t4.lexiconId = t2.id
                {whereClause}";

            var total = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                UserId = userId,
                Name = name
            });

            return (items, total);
        }

        /// <inheritdoc/>
        public async Task<(bool exists, int isEnAudio, int isUsAudio)> CheckWordExistsAsync(int courseId, string en)
        {
            const string sql = @"SELECT l.isenaudio AS IsEnAudio, l.isusaudio AS IsUsAudio 
                FROM coursecontent cc
                INNER JOIN lexicon l ON cc.lexiconId = l.id
                WHERE cc.courseId = @CourseId AND l.en = @En
                LIMIT 1";

            var result = await QueryFirstOrDefaultAsync<(int IsEnAudio, int IsUsAudio)?>(sql, new { CourseId = courseId, En = en });
            if (result == null)
                return (false, 0, 0);

            return (true, result.Value.IsEnAudio, result.Value.IsUsAudio);
        }

        /// <summary>
        /// 验证并清理 orderBy 字段（防止 SQL 注入）
        /// </summary>
        private static string ValidateOrderBy(string orderBy)
        {
            // 允许的排序字段白名单
            var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "t1.id", "t2.frequency", "t2.en", "t2.cn", "t4.updatetime",
                "t4.numbersum", "t4.zynumber", "t4.yznumber", "t4.txnumber", "t4.fynumber"
            };

            var parts = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validParts = new List<string>();

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 1 && allowedFields.Contains(tokens[0]))
                {
                    var direction = tokens.Length >= 2 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
                    validParts.Add($"{tokens[0]} {direction}");
                }
            }

            return validParts.Count > 0 ? string.Join(", ", validParts) : "t1.id DESC";
        }
    }
}
