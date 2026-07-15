using Microsoft.AspNetCore.Http;

namespace LearnEnglish.Application.Dtos.Course
{
    /// <summary>
    /// 课程分类信息
    /// </summary>
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsMyCourse { get; set; }
        /// <summary>
        /// 用户最后学习的课程Id（随分类课程列表一并带出，仅"我的"查询有值）
        /// </summary>
        public int? LastCourseId { get; set; }
    }

    /// <summary>
    /// 课程单词数量统计
    /// </summary>
    public class CourseCountDto
    {
        public int Count { get; set; }
        public int CourseId { get; set; }
    }

    /// <summary>
    /// 课程分类详细信息（含课程列表）
    /// </summary>
    public class CategoryInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsMy { get; set; }
        public bool IsLearn { get; set; }
        public List<CourseInfoDto> CourseInfos { get; set; } = new();
    }

    

    /// <summary>
    /// 课程信息
    /// </summary>
    public class CourseInfoDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsMyCourse { get; set; }
        /// <summary>
        /// 单词数量
        /// </summary>
        public int WordsCount { get; set; }

        /// <summary>
        /// 未牢记
        /// </summary>
        public int NotDoneCount { get; set; }

        /// <summary>
        /// 已完成数量
        /// </summary>
        public int DoneCount { get; set; }

        /// <summary>
        /// 未学习
        /// </summary>
        public int NotLearned { get; set; }

        /// <summary>
        /// 完成百分比
        /// </summary>
        public string Percentage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 未添加课程分类详细信息（含课程列表）
    /// </summary>
    public class NotAddCategoryInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<NotAddCourseInfoDto> CourseInfos { get; set; } = new();
    }

    /// <summary>
    /// 课程信息
    /// </summary>
    public class NotAddCourseInfoDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        /// <summary>
        /// 单词数量
        /// </summary>
        public int WordsCount { get; set; }
    }

    /// <summary>
    /// 我的课程分类聚合
    /// </summary>
    public class MyCategoryInfoDto
    {
        public List<CategoryInfoDto> CategoryInfos { get; set; } = new();
        public List<CategoryInfoDto> MyCategoryInfos { get; set; } = new();

        /// <summary>
        /// 生词本
        /// </summary>
        public CourseInfoDto NewWord { get; set; }

        /// <summary>
        /// 强化区
        /// </summary>
        public CourseInfoDto StrengthenWord { get; set; }

        /// <summary>
        /// 最后学习
        /// </summary>
        public CourseInfoDto LastCourse { get; set; }
    }

    /// <summary>
    /// 课程内容上传请求
    /// </summary>
    public class CourseContentUploadDto
    {
        public IFormFile? File { get; set; }
        public int CourseId { get; set; }
    }
}
