using LearnEnglish.Application.Dtos.Course;
using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 课程管理 API
    /// </summary>
    [Route("api/[controller]")]
    public class CourseController : ApiControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ICurrentUserService _currentUserService;

        public CourseController(ICourseService courseService, ICurrentUserService currentUserService)
        {
            _courseService = courseService;
            _currentUserService = currentUserService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>添加课程到我的学习列表</summary>
        [HttpPost("InsertMyCourse")]
        [Authorize]
        public async Task<IActionResult> InsertMyCourse(int setcourseId)
        {
            var userId = RequireUserId();
            var result = await _courseService.InsertMyCourseAsync(userId, setcourseId);
            return Ok(new { success = result });
        }

        /// <summary>分类课程列表（JSON）</summary>
        [HttpGet("CategoryList")]
        [Authorize]
        public async Task<IActionResult> CategoryList(int type = 1)
        {
            var userId = RequireUserId();
            var categoryInfos = await _courseService.GetCategoryListAsync(userId, type);
            return Ok(new { success = true, type, data = categoryInfos });
        }

        /// <summary>我的分类内容（JSON）</summary>
        [HttpGet("MyCategoryContent")]
        [Authorize]
        public async Task<IActionResult> MyCategoryContent(int type = 1)
        {
            var userId = RequireUserId();
            var model = await _courseService.GetMyCategoryContentAsync(userId, type);
            return Ok(new { success = true, type, data = model });
        }

        /// <summary>获取我的课程进度</summary>
        [HttpGet("MyCategorys")]
        [Authorize]
        public async Task<IActionResult> MyCategorys(int id)
        {
            var userId = RequireUserId();
            var (data, collectCount) = await _courseService.GetMyCoursesProgressAsync(userId, id);

            var list = data.Select(d => new
            {
                courseId = d.CourseId,
                courseName = d.CourseName,
                WordsCount = d.WordsCount,
                DoneCount = d.DoneCount,
                Percentage = d.Percentage
            }).ToList<object>();

            list.Add(new
            {
                courseId = -100,
                courseName = "强化学习区",
                WordsCount = collectCount,
                DoneCount = 0,
                Percentage = "0.00"
            });

            return Ok(new { data = list, success = true });
        }

        /// <summary>获取学习表格基础信息</summary>
        [HttpGet("learnEnglishTable")]
        [Authorize]
        public async Task<IActionResult> learnEnglishTable(int kc)
        {
            var userId = RequireUserId();
            var (id, courseName, isEditable) = await _courseService.GetCourseInfoAsync(userId, kc);
            return Ok(new { success = true, kc = id, courseName, isup = isEditable });
        }

        /// <summary>保存/编辑课程</summary>
        [HttpPost("SaveCourse")]
        [Authorize]
        public async Task<IActionResult> SaveCourse(int setcourseId, string insercoursename, int type)
        {
            var userId = RequireUserId();
            var courseId = await _courseService.SaveCourseAsync(userId, setcourseId, insercoursename, type);
            return Ok(new { msg = courseId > 0 ? "操作成功" : "操作失败", success = courseId > 0 });
        }

        /// <summary>删除课程</summary>
        [HttpPost("deleteCourse")]
        [Authorize]
        public async Task<IActionResult> deleteCourse(int setcourseId)
        {
            var userId = RequireUserId();
            await _courseService.DeleteCourseAsync(userId, setcourseId);
            return Ok(new { msg = "操作成功", success = true });
        }

        /// <summary>添加单词到课程</summary>
        [HttpPost("SaveCoursecontent")]
        [Authorize]
        public async Task<IActionResult> SaveCoursecontent(int courseId, string en, string cn)
        {
            var userId = RequireUserId();
            var result = await _courseService.SaveWordToCourseAsync(userId, courseId, en, cn);
            return Ok(new { msg = result ? "操作成功" : "操作失败", success = result });
        }
    }
}
