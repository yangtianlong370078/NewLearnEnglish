using LearnEnglish.Application.Dtos.Course;
using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 数据导入控制器
    /// </summary>
    public class ImportController : BaseController
    {
        private readonly IImportService _importService;
        private readonly ICurrentUserService _currentUserService;

        public ImportController(IImportService importService, ICurrentUserService currentUserService)
        {
            _importService = importService;
            _currentUserService = currentUserService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>
        /// 从 Excel 导入课程内容
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DaoruCoursecontent(IFormFile file, int kecenid)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, msg = "请选择文件上传。" });
            }

            try
            {
                var userId = RequireUserId();
                using var stream = file.OpenReadStream();
                var count = await _importService.ImportFromExcelAsync(userId, kecenid, stream, file.FileName);
                return Json(new { success = true, msg = string.Format("去重后成功导入：{0}条", count) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }
    }
}
