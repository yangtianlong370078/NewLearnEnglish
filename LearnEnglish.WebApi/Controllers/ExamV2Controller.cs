using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 考试 V2 API（对应原 MVC ExamV2Controller）
    /// </summary>
    [Route("api/[controller]")]
    public class ExamV2Controller : ApiControllerBase
    {
        private readonly IExamService _examService;
        private readonly ICurrentUserService _currentUserService;

        public ExamV2Controller(IExamService examService, ICurrentUserService currentUserService)
        {
            _examService = examService;
            _currentUserService = currentUserService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>创建考试</summary>
        [HttpPost("SaveExam")]
        [Authorize]
        public async Task<IActionResult> SaveExam(int examcount, int limittime)
        {
            var userId = RequireUserId();
            var (success, message, wordCount) = await _examService.CreateExamAsync(userId, examcount, limittime);
            return Ok(new { msg = message, success, wordCount });
        }

        /// <summary>获取考试详情</summary>
        [HttpGet("GetExam")]
        public async Task<IActionResult> GetExam(int examid)
        {
            var result = await _examService.GetExamDetailAsync(examid);
            return Ok(new { data = result });
        }

        /// <summary>考试列表</summary>
        [HttpGet("ExamList")]
        [Authorize]
        public async Task<IActionResult> ExamList(int index = 1, string word = "")
        {
            var userId = RequireUserId();
            var (list, total) = await _examService.GetExamListAsync(userId, word, index, 7);
            var items = list.Select(dto => new
            {
                dto.Id,
                dto.Name,
                examcount = (int)dto.Count,
                createtime = dto.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                txScore = Math.Round(dto.TxScore, 1),
                yzScore = Math.Round(dto.YzScore, 1),
                zyScore = Math.Round(dto.ZyScore, 1),
                txCompleted = dto.TxCount.IsCompleted,
                yzCompleted = dto.YzCount.IsCompleted,
                zyCompleted = dto.ZyCount.IsCompleted,
            }).ToList();
            return Ok(new { success = true, data = items, total, pageIndex = index, pageSize = 7 });
        }

        /// <summary>考试基础信息</summary>
        [HttpGet("ExamContentTable")]
        [Authorize]
        public async Task<IActionResult> ExamContentTable(int id)
        {
            var userId = RequireUserId();
            var examInfo = await _examService.GetExamInfoAsync(userId, id);
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
            await _examService.ReExamAsync(examid, type);
            return Ok(new { msg = "操作成功", success = true });
        }

        /// <summary>删除考试</summary>
        [HttpPost("DeleteExam")]
        [Authorize]
        public async Task<IActionResult> DeleteExam(int examid)
        {
            await _examService.DeleteExamAsync(examid);
            return Ok(new { msg = "操作成功", success = true });
        }

        /// <summary>提交考试答案</summary>
        [HttpPost("InsertExamnswer")]
        [Authorize]
        public async Task<IActionResult> InsertExamnswer([FromForm] string data, [FromForm] int examid, [FromForm] int type, [FromForm] int score)
        {
            var userId = RequireUserId();
            await _examService.SubmitExamAnswersAsync(userId, data, examid, type, score);
            return Ok(new { msg = "操作成功", succss = true });
        }

        /// <summary>考试内容列表</summary>
        [HttpGet("ExamContentList")]
        [Authorize]
        public async Task<IActionResult> ExamContentList(int id = 1, int type = 1)
        {
            var userId = RequireUserId();
            var (items, limitTime, score) = await _examService.GetExamContentListAsync(userId, id, type);
            var data = items.Select(q => new
            {
                q.Id,
                q.LexiconId,
                q.En,
                q.Cn,
                q.ExamId,
                q.Type,
                q.IsOk,
                q.Answer,
                q.Name,
                q.Value,
                q.IsEnAudio,
                q.IsUsAudio,
            }).ToList();
            return Ok(new { success = true, data, limittime = limitTime ?? 0, score = score ?? 0 });
        }
    }
}
