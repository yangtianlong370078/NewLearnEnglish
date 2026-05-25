using LearnEnglish.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LearnEnglish.Infrastructure.MongoDB
{
    /// <summary>
    /// MongoDB 单词详情 Repository 实现
    /// </summary>
    public class LexiconDetailRepository : ILexiconDetailRepository
    {
        private readonly IMongoCollection<LexiconDetail> _collection;
        private readonly IMongoCollection<LexiconDetailSimple> _simpleCollection;
        private readonly IMongoDatabase _database;

        public LexiconDetailRepository(IOptions<MongoDbOptions> options)
        {
            var mongoOptions = options.Value;
            var client = new MongoClient(mongoOptions.ConnectionString);
            _database = client.GetDatabase(mongoOptions.DatabaseName);
            _collection = _database.GetCollection<LexiconDetail>(mongoOptions.LexiconCollectionName);
            _simpleCollection = _database.GetCollection<LexiconDetailSimple>(mongoOptions.LexiconCollectionName);
        }

        public async Task<LexiconDetail?> GetByWordAsync(string word)
        {
            var filter = Builders<LexiconDetail>.Filter.Eq(x => x.Word, word);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<LexiconDetailSimple?> GetSimpleByWordAsync(string word)
        {
            var filter = Builders<LexiconDetailSimple>.Filter.Eq(x => x.Word, word);
            return await _simpleCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LexiconDetail>> GetByWordsAsync(IEnumerable<string> words)
        {
            var filter = Builders<LexiconDetail>.Filter.In(x => x.Word, words);
            return await _collection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// 插入单条词典详情
        /// </summary>
        public async Task InsertAsync(LexiconDetail detail)
        {
            await _collection.InsertOneAsync(detail);
        }

        /// <summary>
        /// 批量插入词典详情
        /// </summary>
        public async Task BulkInsertAsync(IEnumerable<LexiconDetail> details)
        {
            var list = details.ToList();
            if (list.Count > 0)
            {
                await _collection.InsertManyAsync(list);
            }
        }

        /// <summary>
        /// 获取所有词典详情
        /// </summary>
        public async Task<IEnumerable<LexiconDetail>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
    }
}
