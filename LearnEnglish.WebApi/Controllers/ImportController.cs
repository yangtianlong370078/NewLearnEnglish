using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 数据导入 API
    /// </summary>
    [Route("api/[controller]")]
    public class ImportController : ApiControllerBase
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

        /// <summary>从 Excel 导入课程内容</summary>
        [HttpPost("DaoruCoursecontent")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DaoruCoursecontent(IFormFile file, [FromForm] int kecenid)
        {
            if (file == null || file.Length == 0)
            {
                return Ok(new { success = false, msg = "请选择文件上传。" });
            }

            try
            {
                var userId = RequireUserId();
                using var stream = file.OpenReadStream();
                var count = await _importService.ImportFromExcelAsync(userId, kecenid, stream, file.FileName);
                return Ok(new { success = true, msg = string.Format("去重后成功导入：{0}条", count) });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, msg = ex.Message });
            }
        }
    }
}
