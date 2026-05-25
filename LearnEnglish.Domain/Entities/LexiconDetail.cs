using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 单词详情 MongoDB 文档（包含例句对象）
    /// </summary>
    public class LexiconDetail
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("word")]
        public string Word { get; set; } = string.Empty;

        [BsonElement("frequence")]
        public int Frequence { get; set; }

        [BsonElement("sampleSentences")]
        public List<SampleSentence>? SampleSentences { get; set; } = new();

        [BsonElement("phonetic")]
        public string Phonetic { get; set; } = string.Empty;

        [BsonElement("britishPhonetic")]
        public string BritishPhonetic { get; set; } = string.Empty;

        [BsonElement("americanPhonetic")]
        public string AmericanPhonetic { get; set; } = string.Empty;

        [BsonElement("definition")]
        public List<string> Definition { get; set; } = new();

        [BsonElement("translation")]
        public List<string> Translation { get; set; } = new();

        [BsonElement("tag")]
        public List<string> Tag { get; set; } = new();
    }

    /// <summary>
    /// 例句
    /// </summary>
    public class SampleSentence
    {
        [BsonElement("en")]
        public string En { get; set; } = string.Empty;

        [BsonElement("cn")]
        public string Cn { get; set; } = string.Empty;
    }

    /// <summary>
    /// 单词详情 MongoDB 文档（例句为字符串列表版本）
    /// </summary>
    public class LexiconDetailSimple
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("word")]
        public string Word { get; set; } = string.Empty;

        [BsonElement("frequence")]
        public int Frequence { get; set; }

        [BsonElement("sampleSentences")]
        public List<string>? SampleSentences { get; set; } = new();

        [BsonElement("phonetic")]
        public string Phonetic { get; set; } = string.Empty;

        [BsonElement("britishPhonetic")]
        public string BritishPhonetic { get; set; } = string.Empty;

        [BsonElement("americanPhonetic")]
        public string AmericanPhonetic { get; set; } = string.Empty;

        [BsonElement("definition")]
        public List<string> Definition { get; set; } = new();

        [BsonElement("translation")]
        public List<string> Translation { get; set; } = new();

        [BsonElement("tag")]
        public List<string> Tag { get; set; } = new();
    }
}
