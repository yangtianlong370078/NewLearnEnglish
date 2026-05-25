using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models.ErrorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 考试 API（对应原 MVC ExamController）
    /// </summary>
    [Route("api/[controller]")]
    public class ExamController : ApiControllerBase
    {
        private readonly ILogger<ExamController> _logger;
        private readonly IExamService _examService;
        private readonly UserService _userService;

        public ExamController(ILogger<ExamController> logger, IExamService examService, UserService userService)
        {
            _logger = logger;
            _examService = examService;
            _userService = userService;
        }

        /// <summary>创建考试</summary>
        [HttpPost("SaveExam")]
        [Authorize]
        public async Task<IActionResult> SaveExam(int examcount, int limittime)
        {
            var user = _userService.GetValidUser();
            var (success, message, wordCount) = await _examService.CreateExamAsync(user.id, examcount, limittime);
            return Ok(new { msg = message, success, wordCount });
        }

        /// <summary>获取考试详情</summary>
        [HttpGet("GetExam")]
        public async Task<IActionResult> GetExam(int examid)
        {
            var dto = await _examService.GetExamDetailAsync(examid);
            return Ok(new { data = dto });
        }

        /// <summary>考试列表</summary>
        [HttpGet("ExamList")]
        [Authorize]
        public async Task<IActionResult> ExamList(int index = 1, string word = "")
        {
            var user = _userService.GetValidUser();
            var (list, total) = await _examService.GetExamListAsync(user.id, string.IsNullOrWhiteSpace(word) ? null : word, index, 7);
            return Ok(new { success = true, data = list, total, pageIndex = index, pageSize = 7, word });
        }

        /// <summary>考试基础信息</summary>
        [HttpGet("ExamContentTable")]
        [Authorize]
        public async Task<IActionResult> ExamContentTable(int id)
        {
            var user = _userService.GetValidUser();
            var examInfo = await _examService.GetExamInfoAsync(user.id, id);
            return Ok(new
            {
                success = true,
                examId = examInfo?.id ?? id,
                limittime = examInfo?.limitTime ?? 0,
                examName = examInfo?.name ?? ""
            });
        }

        /// <summary>重新考试</summary>
        [HttpPost("RestartExam")]
        [Authorize]
        public async Task<IActionResult> RestartExam(int examid, int type)
        {
            var success = await _examService.ReExamAsync(examid, type);
            return Ok(new { msg = success ? "操作成功" : "操作失败", success });
        }

        /// <summary>删除考试</summary>
        [HttpPost("DeleteExam")]
        [Authorize]
        public async Task<IActionResult> DeleteExam(int examid)
        {
            var success = await _examService.DeleteExamAsync(examid);
            return Ok(new { msg = success ? "操作成功" : "操作失败", success });
        }

        /// <summary>提交考试答案</summary>
        [HttpPost("InsertExamnswer")]
        [Authorize]
        public async Task<IActionResult> InsertExamnswer([FromForm] string data, [FromForm] int examid, [FromForm] int type, [FromForm] int score)
        {
            var user = _userService.GetValidUser();
            var success = await _examService.SubmitExamAnswersAsync(user.id, data, examid, type, score);
            return Ok(new { msg = success ? "操作成功" : "操作失败", succss = success });
        }

        /// <summary>考试内容列表</summary>
        [HttpGet("ExamContentList")]
        [Authorize]
        public async Task<IActionResult> ExamContentList(int id = 1, int type = 1)
        {
            var user = _userService.GetValidUser();
            var (items, limitTime, score) = await _examService.GetExamContentListAsync(user.id, id, type);
            return Ok(new
            {
                success = true,
                data = items,
                limittime = limitTime ?? 0,
                score = score ?? 0,
                isexam = limitTime != null && score != null
            });
        }
    }
}
