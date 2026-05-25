using LearnEnglish.Application.Dtos.Course;
using LearnEnglish.Application.Dtos.Word;
using LearnEnglish.Domain.Common;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 课程管理服务接口
    /// </summary>
    public interface ICourseService
    {
        /// <summary>
        /// 将课程加入用户学习列表
        /// </summary>
        Task<bool> InsertMyCourseAsync(int userId, int courseId);

        /// <summary>
        /// 获取分类课程列表（含完成/未完成统计）
        /// </summary>
        Task<List<CategoryInfoDto>> GetCategoryListAsync(int userId, int type);

        /// <summary>
        /// 获取我的分类内容（含收藏夹统计）
        /// </summary>
        Task<MyCategoryInfoDto> GetMyCategoryContentAsync(int userId, int type);

        /// <summary>
        /// 获取我的课程学习进度 JSON
        /// </summary>
        Task<(List<CourseInfoDto> data, int collectCount)> GetMyCoursesProgressAsync(int userId, int courseId);

        /// <summary>
        /// 获取课程信息（学习表格页面）
        /// </summary>
        Task<(int id, string courseName, bool isEditable)> GetCourseInfoAsync(int userId, int courseId);

        /// <summary>
        /// 保存/编辑课程
        /// </summary>
        Task<int> SaveCourseAsync(int userId, int courseId, string name, int type);

        /// <summary>
        /// 删除课程（含级联删除）
        /// </summary>
        Task DeleteCourseAsync(int userId, int courseId);

        /// <summary>
        /// 添加单词到课程
        /// </summary>
        Task<bool> SaveWordToCourseAsync(int userId, int courseId, string en, string cn);
    }
}
