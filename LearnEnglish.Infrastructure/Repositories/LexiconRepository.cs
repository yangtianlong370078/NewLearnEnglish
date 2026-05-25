using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 单词 Repository 实现
    /// </summary>
    public class LexiconRepository : DapperRepository<Lexicon>, ILexiconRepository
    {
        protected override string TableName => "lexicon";

        public LexiconRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public new async Task<Lexicon?> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public async Task<Lexicon?> GetByEnAsync(string en)
        {
            const string sql = "SELECT * FROM `lexicon` WHERE en = @En LIMIT 1";
            return await QueryFirstOrDefaultAsync<Lexicon>(sql, new { En = en });
        }

        public async Task<IEnumerable<Lexicon>> GetByIdsAsync(IEnumerable<int> ids)
        {
            const string sql = "SELECT * FROM `lexicon` WHERE id IN @Ids";
            return await QueryAsync<Lexicon>(sql, new { Ids = ids });
        }

        public async Task<int> CreateAsync(Lexicon lexicon)
        {
            const string sql = @"INSERT INTO `lexicon` (en, cn, userId, isenaudio, isusaudio, frequency) 
                VALUES (@En, @Cn, @UserId, @IsEnAudio, @IsUsAudio, @Frequency)";
            return await InsertAsync(sql, lexicon);
        }

        public async Task UpdateAsync(Lexicon lexicon)
        {
            const string sql = @"UPDATE `lexicon` SET 
                en = @En, cn = @Cn, userId = @UserId, 
                isenaudio = @IsEnAudio, isusaudio = @IsUsAudio, frequency = @Frequency 
                WHERE id = @Id";
            await ExecuteAsync(sql, lexicon);
        }

        public async Task<IEnumerable<Lexicon>> GetWithoutAudioAsync(int limit)
        {
            const string sql = "SELECT en FROM `lexicon` WHERE isenaudio = 0 LIMIT @Limit";
            return await QueryAsync<Lexicon>(sql, new { Limit = limit });
        }

        public async Task UpdateEnCnAsync(int id, int userId, string en, string cn)
        {
            const string sql = "UPDATE `lexicon` SET en = @En, cn = @Cn WHERE id = @Id AND userId = @UserId";
            await ExecuteAsync(sql, new { Id = id, UserId = userId, En = en, Cn = cn });
        }
    }
}
