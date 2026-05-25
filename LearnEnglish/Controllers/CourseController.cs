using LearnEnglish.Application.Dtos.Course;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 课程管理控制器
    /// </summary>
    public class CourseController : BaseController
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

        /// <summary>
        /// 添加课程到我的学习列表
        /// </summary>
        [Authorize]
        public async Task<JsonResult> InsertMyCourse(int setcourseId)
        {
            var userId = RequireUserId();
            var result = await _courseService.InsertMyCourseAsync(userId, setcourseId);
            return Json(new { success = result });
        }

        /// <summary>
        /// 我的课程列表页面（主入口）
        /// </summary>
        [Authorize]
        public ActionResult MyCategoryList(int type = 1)
        {
            ViewData["type"] = type;
            return View("~/Views/Home/MyCategoryList.cshtml");
        }

        /// <summary>
        /// 分类课程列表页面
        /// </summary>
        [Authorize]
        public async Task<ActionResult> CategoryList(int type = 1)
        {
            var userId = RequireUserId();
            var categoryInfos = await _courseService.GetCategoryListAsync(userId, type);
            return View("~/Views/Home/CategoryList.cshtml", MapToOldCategoryInfoList(categoryInfos));
        }

        /// <summary>
        /// 我的分类内容页面
        /// </summary>
        [Authorize]
        public async Task<ActionResult> MyCategoryContent(int type = 1)
        {
            var userId = RequireUserId();
            var model = await _courseService.GetMyCategoryContentAsync(userId, type);
            var viewModel = new MyCategoryInfo
            {
                CategoryInfos = MapToOldCategoryInfoList(model.CategoryInfos),
                MyCategoryInfos = MapToOldCategoryInfoList(model.MyCategoryInfos)
            };
            return View(type == 1 ? "~/Views/Home/MyCategoryContent.cshtml" : "~/Views/Home/MyCategoryContentTL.cshtml", viewModel);
        }

        /// <summary>
        /// 获取我的课程进度 JSON
        /// </summary>
        [Authorize]
        public async Task<JsonResult> MyCategorys(int id)
        {
            var userId = RequireUserId();
            var (data, collectCount) = await _courseService.GetMyCoursesProgressAsync(userId, id);

            // 与原项目保持一致：将收藏夹作为 courseId=-100 的特殊条目加入 data 数组
            var categoryInfos = data.Select(d => new Models.Dtos.courseInfo
            {
                courseId = d.CourseId,
                courseName = d.CourseName,
                WordsCount = d.WordsCount,
                DoneCount = d.DoneCount,
                Percentage = d.Percentage
            }).ToList();

            categoryInfos.Add(new Models.Dtos.courseInfo
            {
                courseId = -100,
                courseName = "强化学习区",
                WordsCount = collectCount,
                DoneCount = 0,
                Percentage = "0.00"
            });

            return Json(new { data = categoryInfos, success = true });
        }

        /// <summary>
        /// 学习表格页面
        /// </summary>
        [Authorize]
        public async Task<ActionResult> learnEnglishTable(int kc)
        {
            var userId = RequireUserId();
            var (id, courseName, isEditable) = await _courseService.GetCourseInfoAsync(userId, kc);

            ViewData["kc"] = id;
            ViewData["courseName"] = courseName;
            ViewData["isup"] = isEditable;
            return View("~/Views/Home/learnEnglishTable.cshtml");
        }

        /// <summary>
        /// 保存/编辑课程
        /// </summary>
        [Authorize]
        public async Task<JsonResult> SaveCourse(int setcourseId, string insercoursename, int type)
        {
            var userId = RequireUserId();
            var courseId = await _courseService.SaveCourseAsync(userId, setcourseId, insercoursename, type);
            return Json(new { msg = courseId > 0 ? "操作成功" : "操作失败", success = courseId > 0 });
        }

        /// <summary>
        /// 删除课程
        /// </summary>
        [Authorize]
        public async Task<JsonResult> deleteCourse(int setcourseId)
        {
            var userId = RequireUserId();
            await _courseService.DeleteCourseAsync(userId, setcourseId);
            return Json(new { msg = "操作成功", success = true });
        }

        /// <summary>
        /// 添加单词到课程
        /// </summary>
        [Authorize]
        public async Task<JsonResult> SaveCoursecontent(int courseId, string en, string cn)
        {
            var userId = RequireUserId();
            var result = await _courseService.SaveWordToCourseAsync(userId, courseId, en, cn);
            return Json(new { msg = result ? "操作成功" : "操作失败", success = result });
        }

        #region 新 DTO → 旧视图模型映射

        /// <summary>
        /// 将新 CategoryInfoDto 列表映射为旧 CategoryInfo 列表（视图兼容）
        /// </summary>
        private static List<CategoryInfo> MapToOldCategoryInfoList(List<CategoryInfoDto> dtos)
        {
            return dtos.Select(d =>
            {
                var info = new CategoryInfo
                {
                    id = d.Id,
                    name = d.Name,
                    ismy = d.IsMy,
                    islearn = d.IsLearn
                };
                info.courseInfos.AddRange(d.CourseInfos.Select(c => new courseInfo
                {
                    courseId = c.CourseId,
                    courseName = c.CourseName,
                    ismycourse = c.IsMyCourse,
                    WordsCount = c.WordsCount,
                    DoneCount = c.DoneCount,
                    Percentage = c.Percentage
                }));
                return info;
            }).ToList();
        }

        #endregion
    }
}
