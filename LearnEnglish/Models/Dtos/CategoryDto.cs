namespace LearnEnglish.Models.Dtos
{
    public class CategoryDto
    {

        public int id { get; set; }

        public string name { get; set; }

        public int userId { get; set; }

        public int courseId { get; set; }

        public string courseName { get; set; }

        public bool ismycourse { get; set; }

    }

    public class counrsCountDto
    {
        public int count { get; set; }

        public int courseId { get; set; }
    }

    public class CategoryInfo
    {
        public int id { get; set; }

        public string name { get; set; }
        public bool ismy { get; set; }
      
        public bool islearn { get; set; }

        public List<courseInfo> courseInfos = new List<courseInfo>();
    }

    public class courseInfo
    {
        public int courseId { get; set; }

        public string courseName { get; set; }
        public bool ismycourse { get; set; }
        /// <summary>
        /// 单词数量
        /// </summary>
        public int WordsCount { get; set; }

        /// <summary>
        /// 已完成数量
        /// </summary>
        public int DoneCount { get; set; }

        /// <summary>
        /// 完成百分比
        /// </summary>
        public string Percentage { get; set; }


    }

    public class MyCategoryInfo {

        public List<CategoryInfo> CategoryInfos { get; set; } = new List<CategoryInfo>();

        public List<CategoryInfo> MyCategoryInfos { get; set; } = new List<CategoryInfo>();

    }


}
