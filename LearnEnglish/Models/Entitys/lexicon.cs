using System.ComponentModel.DataAnnotations;

namespace LearnEnglish.Models.Entitys
{
    /// <summary>
    /// 单词表
    /// </summary>
    public class lexicon
    {
        /// <summary>
        /// 单词Id
        /// </summary>
        [IsEffective(false)]
        public int id { get; set; }
        /// <summary>
        /// 中文
        /// </summary>
        public string cn { get; set; }
        /// <summary>
        /// 英文
        /// </summary>
        [Key("en")]
        public string en { get; set; }
    }

    /// <summary>
    /// 课程表
    /// </summary>
    public class course
    {
        /// <summary>
        /// 课程
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 课程名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 所属人员
        /// </summary>
        public int userId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createdate { get; set; }
    }

    /// <summary>
    /// 课程详情
    /// </summary>
    public class coursecontent
    {
        /// <summary>
        /// 详情Id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 课程Id
        /// </summary>
        public int courseId { get; set; }
        /// <summary>
        /// 单词Id
        /// </summary>
        public int lexiconId { get; set; }
        ///// <summary>
        ///// 状态
        ///// </summary>
        //public int zt { get; set; }
        ///// <summary>
        ///// 成功数量记录
        ///// </summary>
        //public int number { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createdate { get; set; }
    }

    public class learn
    {
        public int id { get; set; }
        /// <summary>
        /// 课程内容Id(可以根据这个对应到具体的单词)
        /// </summary>
        public int coursecontentId { get; set; }

        /// <summary>
        /// 学习状态（2未牢记，3为已掌握）
        /// </summary>
        public  int status { get; set; }

        /// <summary>
        /// 错误次数统计
        /// </summary>
        public int number { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// 是否收藏
        /// </summary>
        public int iscollect { get; set; }
       
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createdate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime updatetime { get; set; }

        /// <summary>
        /// 修改日期，没有时分秒
        /// </summary>
        public DateTime updatedate { get; set; }
    }

    public class mylearn
    {
        /// <summary>
        /// 用户
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int lexiconId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime updatetime { get; set; }

        /// <summary>
        /// 修改日期，没有时分秒
        /// </summary>
        public DateTime updatedate { get; set; }
    }

    /// <summary>
    /// 用户单词扩展表
    /// </summary>
    public class mylexicon
    {
        public int id { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int lexiconId { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// 中文
        /// </summary>
        public string cn { get; set; }
    }

    public class users
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public string phone { get; set; }
        public string password { get; set; }
    }
}
