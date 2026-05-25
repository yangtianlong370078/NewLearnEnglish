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
        public List<sampleSentence>? sampleSentences = new List<sampleSentence>();

        public string phonetic { get; set; }
        public string britishPhonetic { get; set; }
        public string americanPhonetic { get; set; }
        public List<string> definition { get; set; }
        public List<string> translation { get; set; }
        public List<string> tag { get; set; }
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
        public List<string>? sampleSentences = new List<string>();
        public string phonetic { get; set; }
        public string britishPhonetic { get; set; }
        public string americanPhonetic { get; set; }
        public List<string> definition { get; set; }
        public List<string> translation { get; set; }
        public List<string> tag { get; set; }
    }

    
}
