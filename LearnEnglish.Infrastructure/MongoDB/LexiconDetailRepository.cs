using LearnEnglish.Domain.Entities;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
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

        /// <summary>
        /// 创建索引，确保 Word 字段使用忽略大小写的 collation 索引
        /// </summary>
        public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
        {
            var collation = new Collation("en", strength: CollationStrength.Secondary);
            var indexModel = new CreateIndexModel<LexiconDetail>(
                Builders<LexiconDetail>.IndexKeys.Ascending(x => x.Word),
                new CreateIndexOptions { Collation = collation, Name = "ix_word_ci" });
            await _collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
        }

        public async Task<LexiconDetail?> GetByWordAsync(string word)
        {
            var filter = Builders<LexiconDetail>.Filter.Eq(x => x.Word, word);

            var collation = new Collation("en", strength: CollationStrength.Secondary);
            var options = new FindOptions { Collation = collation };
            return await _collection.Find(filter, options).FirstOrDefaultAsync();

        }

        public async Task<LexiconDetailSimple?> GetSimpleByWordAsync(string word)
        {
            var filter = Builders<LexiconDetailSimple>.Filter.Eq(x => x.Word, word);
            var collation = new Collation("en", strength: CollationStrength.Secondary);
            var options = new FindOptions { Collation = collation };
            return await _simpleCollection.Find(filter, options).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LexiconDetail>> GetByWordsAsync(IEnumerable<string> words)
        {
            var filter = Builders<LexiconDetail>.Filter.In(x => x.Word, words);
            var collation = new Collation("en", strength: CollationStrength.Secondary);
            var options = new FindOptions { Collation = collation };

            return await _collection.Find(filter, options).ToListAsync();
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
