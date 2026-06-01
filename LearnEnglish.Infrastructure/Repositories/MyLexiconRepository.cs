using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 用户自定义单词 Repository 实现
    /// </summary>
    public class MyLexiconRepository : DapperRepository<MyLexicon>, IMyLexiconRepository
    {
        protected override string TableName => "mylexicon";

        public MyLexiconRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<MyLexicon?> GetByUserAndLexiconAsync(int userId, int lexiconId)
        {
            const string sql = @"SELECT * FROM `mylexicon` 
                WHERE userId = @UserId AND lexiconId = @LexiconId LIMIT 1";
            return await QueryFirstOrDefaultAsync<MyLexicon>(sql, new { UserId = userId, LexiconId = lexiconId });
        }

        public async Task<int> CreateOrUpdateAsync(MyLexicon myLexicon)
        {
            const string sql = @"INSERT INTO `mylexicon` 
                (lexiconId, userId, cn, status, zynumber, yznumber, txnumber, fynumber, numbersum, iscollect, ishand, updatetime, updatedate) 
                VALUES (@LexiconId, @UserId, @Cn, @Status, @ZyNumber, @YzNumber, @TxNumber, @FyNumber, @NumberSum, @IsCollect, @IsHand, @UpdateTime, @UpdateDate) 
                ON DUPLICATE KEY UPDATE 
                status = VALUES(status), zynumber = VALUES(zynumber), yznumber = VALUES(yznumber), 
                txnumber = VALUES(txnumber), fynumber = VALUES(fynumber), numbersum = VALUES(numbersum), 
                iscollect = VALUES(iscollect), ishand = VALUES(ishand), updatetime = VALUES(updatetime), updatedate = VALUES(updatedate)";
            return await InsertAsync(sql, myLexicon);
        }

        public async Task UpdateCnAsync(int userId, int lexiconId, string cn)
        {
            // 先检查是否存在
            var exists = await GetByUserAndLexiconAsync(userId, lexiconId);
            if (exists != null)
            {
                const string updateSql = "UPDATE `mylexicon` SET cn = @Cn WHERE userId = @UserId AND lexiconId = @LexiconId";
                await ExecuteAsync(updateSql, new { UserId = userId, LexiconId = lexiconId, Cn = cn });
            }
            else
            {
                const string insertSql = @"INSERT INTO `mylexicon` (lexiconId, userId, cn) 
                    VALUES (@LexiconId, @UserId, @Cn)";
                await ExecuteAsync(insertSql, new { LexiconId = lexiconId, UserId = userId, Cn = cn });
            }
        }

        public async Task SetCollectAsync(int userId, int lexiconId, int isCollect)
        {
            const string sql = @"INSERT INTO `mylexicon` (lexiconId, userId, iscollect) 
                VALUES (@LexiconId, @UserId, @IsCollect) 
                ON DUPLICATE KEY UPDATE iscollect = VALUES(iscollect)";
            await ExecuteAsync(sql, new { LexiconId = lexiconId, UserId = userId, IsCollect = isCollect });
        }

        public async Task UpdateStatusAsync(int userId, int lexiconId, int status)
        {
            const string sql = @"INSERT INTO `mylexicon` 
                (lexiconId, userId, status, ishand, updatetime, updatedate) 
                VALUES (@LexiconId, @UserId, @Status, 1, @UpdateTime, @UpdateDate) 
                ON DUPLICATE KEY UPDATE 
                status = VALUES(status), ishand = 1, updatetime = VALUES(updatetime), updatedate = VALUES(updatedate)";
            await ExecuteAsync(sql, new
            {
                LexiconId = lexiconId,
                UserId = userId,
                Status = status,
                UpdateTime = DateTime.Now,
                UpdateDate = DateTime.Today
            });
        }

        public async Task UpsertOrInsertNumberAsync(int userId, int lexiconId,  int status)
        {
            (int no, string zynumber, string yznumber, string txnumber, string fynumber) = status switch
            {
                1 => (0, "CASE WHEN zynumber >0 THEN 0 ELSE zynumber END", "CASE WHEN yznumber >0 THEN 0 ELSE yznumber END", "CASE WHEN txnumber >0 THEN 0 ELSE txnumber END", "CASE WHEN fynumber >0 THEN 0 ELSE fynumber END"),
                2 => (5, "CASE WHEN zynumber <> 5 THEN 5 ELSE zynumber END", "CASE WHEN yznumber <> 5 THEN 5 ELSE yznumber END", "CASE WHEN txnumber <> 5 THEN 5 ELSE txnumber END", "CASE WHEN fynumber <> 5 THEN 5 ELSE fynumber END"),
                3 => (10, "CASE WHEN zynumber < 10 THEN 10 ELSE zynumber END", "CASE WHEN yznumber < 10 THEN 10 ELSE yznumber END", "CASE WHEN txnumber < 10 THEN 10 ELSE txnumber END", "CASE WHEN fynumber < 10 THEN 10 ELSE fynumber END"),
                _ => (0, "zynumber", "yznumber", "txnumber", "fynumber")
            };

            string instLexicon = $@"INSERT INTO mylexicon (userId, lexiconId, zynumber,yznumber,txnumber,fynumber,updatetime,ishand,status)
                                    VALUES ({userId},@lexiconId,@number,@number,@number,@number,now(),1,{status})
                                    ON DUPLICATE KEY UPDATE  
                                    zynumber = {zynumber},
                                    yznumber = {yznumber},
                                    txnumber = {txnumber},
                                    fynumber = {fynumber},
                                    updatetime=now(),
                                    ishand =1,
                                    status = {status}; ";

             await ExecuteAsync(instLexicon, new { number = no, lexiconId = lexiconId });
        }

        public async Task UpsertNumberAsync(int userId, int lexiconId, string numberField, int number, int status)
        {
            var validFields = new HashSet<string> { "zynumber", "yznumber", "txnumber", "fynumber" };
            if (!validFields.Contains(numberField.ToLowerInvariant()))
            {
                throw new ArgumentException($"无效的字段名: {numberField}");
            }

            var sql = $@"INSERT INTO `mylexicon` 
                (lexiconId, userId, status, {numberField}, ishand, updatetime) 
                VALUES (@LexiconId, @UserId, @Status, @NumberValue, 1, @UpdateTime) 
                ON DUPLICATE KEY UPDATE 
                status = VALUES(status), {numberField} = VALUES({numberField}), 
                ishand = 1, updatetime = VALUES(updatetime)";
            await ExecuteAsync(sql, new
            {
                LexiconId = lexiconId,
                UserId = userId,
                Status = status,
                NumberValue = number,
                UpdateTime = DateTime.Now
            });
        }

        public async Task BatchUpsertNumbersAsync(int userId, Dictionary<int, int> lexiconNumbers, string numberField)
        {
            var validFields = new HashSet<string> { "zynumber", "yznumber", "txnumber", "fynumber" };
            if (!validFields.Contains(numberField.ToLowerInvariant()))
            {
                throw new ArgumentException($"无效的字段名: {numberField}");
            }

            var sql = $@"INSERT INTO `mylexicon` (userId, lexiconId, {numberField}, numbersum, updatetime) 
                VALUES (@UserId, @LexiconId, @NumberValue, 1, @UpdateTime) 
                ON DUPLICATE KEY UPDATE 
                {numberField} = @NumberValue, numbersum = numbersum + 1, updatetime = VALUES(updatetime)";

            using var connection = _connectionFactory.CreateConnection();
            foreach (var (lexiconId, numberValue) in lexiconNumbers)
            {
                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    LexiconId = lexiconId,
                    NumberValue = numberValue,
                    UpdateTime = DateTime.Now
                });
            }
        }

        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM `mylexicon` WHERE userId = @UserId AND iscollect = 1";
            var result = await ExecuteScalarAsync<int>(sql, new { UserId = userId });
            return result;
        }

        public async Task<IEnumerable<(int LexiconId, int OldStatus, int NewStatus)>> GetCalibrationChangesAsync(int userId)
        {
            // 根据各维度得分计算新状态与当前状态比较
            const string sql = @"SELECT lexiconId AS LexiconId, status AS OldStatus, 
                CASE 
                    WHEN zynumber >= 10 AND yznumber >= 10 AND txnumber >= 10 AND fynumber >= 10 THEN 3
                    WHEN zynumber > 0 OR yznumber > 0 OR txnumber > 0 OR fynumber > 0 THEN 2
                    ELSE 1
                END AS NewStatus
                FROM `mylexicon` 
                WHERE userId = @UserId 
                HAVING OldStatus <> NewStatus";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<(int LexiconId, int OldStatus, int NewStatus)>(sql, new { UserId = userId });
            return result;
        }

        public async Task ApplyCalibrationAsync(int userId, IEnumerable<(int LexiconId, int NewStatus)> changes)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var (lexiconId, newStatus) in changes)
                {
                    const string sql = @"UPDATE `mylexicon` SET 
                        status = @Status, ishand = 0, updatetime = @UpdateTime
                        WHERE userId = @UserId AND lexiconId = @LexiconId";
                    await connection.ExecuteAsync(sql, new
                    {
                        LexiconId = lexiconId,
                        UserId = userId,
                        Status = newStatus,
                        UpdateTime = DateTime.Now
                    }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task ResetExamProficiencyAsync(int examId, int type, string numberField)
        {
            var validFields = new HashSet<string> { "zynumber", "yznumber", "txnumber", "fynumber" };
            if (!validFields.Contains(numberField.ToLowerInvariant()))
            {
                throw new ArgumentException($"无效的字段名: {numberField}");
            }

            // 考试提交后，将答错的单词对应维度的次数重置为0，并累加总练习次数
            var sql = $@"UPDATE `mylexicon` 
                INNER JOIN `examdetail` t1 ON mylexicon.id = t1.learnid
                INNER JOIN `examnswer` t2 ON t2.examdetailid = t1.id
                SET mylexicon.{numberField} = 0, mylexicon.numbersum = mylexicon.numbersum + 1
                WHERE t2.examid = @ExamId AND t2.type = @Type";
            await ExecuteAsync(sql, new { ExamId = examId, Type = type });
        }
    }
}
