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
        /// 已完成数量
        /// </summary>
        public int DoneCount { get; set; }
        /// <summary>
        /// 完成百分比
        /// </summary>
        public string Percentage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 我的课程分类聚合
    /// </summary>
    public class MyCategoryInfoDto
    {
        public List<CategoryInfoDto> CategoryInfos { get; set; } = new();
        public List<CategoryInfoDto> MyCategoryInfos { get; set; } = new();
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
