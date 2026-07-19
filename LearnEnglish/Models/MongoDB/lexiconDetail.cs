using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace LearnEnglish.Models.MongoDB
{
    public class lexicondetail
    {
        [BsonId] // 如果你使用特性来标识_id作为主键  
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId _id { get; set; }
        public string word { get; set; }
        public int frequence { get; set; }
        public List<sampleSentence>? sampleSentences { get; set; } = new List<sampleSentence>();

        public string phonetic { get; set; }
        public string britishPhonetic { get; set; }
        public string americanPhonetic { get; set; }
        public List<string> definition { get; set; }
        public List<string> translation { get; set; }
        public List<string> tag { get; set; }

        /// <summary>
        /// 音节拆分
        /// </summary>
        public List<string> Syllables { get; set; }

        /// <summary>
        /// 自然拼读拆分为字母组合
        /// </summary>
        public List<PhonicsSplit> PhonicsSplits { get; set; }
    }

    /// <summary>
    /// 字母组合
    /// </summary>
    public class PhonicsSplit
    {
        /// <summary>
        /// 字母组合
        /// </summary>
        public string LetterCombine { get; set; }
        /// <summary>
        /// 对应音标
        /// </summary>
        public string PhoneticSymbol { get; set; }
    }


    public class sampleSentence
    {
        public string en { get; set; }
        public string cn { get; set; }

    }



    public class lexicondetailtwo
    {
        [BsonId] // 如果你使用特性来标识_id作为主键  
        public ObjectId _id { get; set; }
        public string word { get; set; }
        public int frequence { get; set; }
        public List<string>? sampleSentences { get; set; } = new List<string>();
        public string phonetic { get; set; }
        public string britishPhonetic { get; set; }
        public string americanPhonetic { get; set; }
        public List<string> definition { get; set; }
        public List<string> translation { get; set; }
        public List<string> tag { get; set; }
    }

    
}
