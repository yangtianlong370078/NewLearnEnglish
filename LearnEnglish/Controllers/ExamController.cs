using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models;
using LearnEnglish.Models.Dtos;
using LearnEnglish.Models.ErrorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{

    public class ExamController : BaseController
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

        [Authorize]
        public async Task<JsonResult> SaveExam(int examcount, int limittime)
        {
            var user = _userService.GetValidUser();
            var (success, message, wordCount) = await _examService.CreateExamAsync(user.id, examcount, limittime);
            return Json(new { msg = message, success });
        }

        public async Task<JsonResult> GetExam(int examid)
        {
            var dto = await _examService.GetExamDetailAsync(examid);
            // 映射为旧 DTO（视图兼容）
            var data = dto != null ? MapToOldExamOutDto(dto) : null;
            return Json(new { data });
        }

        [Authorize]
        public async Task<ActionResult> ExamList(int index = 1, string word = "")
        {
            var user = _userService.GetValidUser();
            var (list, total) = await _examService.GetExamListAsync(user.id, string.IsNullOrWhiteSpace(word) ? null : word, index, 7);

            // 映射为旧 DTO（视图兼容）
            var oldDtos = list.Select(MapToOldExamOutDto).ToList();

            ViewData["word"] = word;
            PagedList<ExamOutDto> datas = new PagedList<ExamOutDto>(oldDtos, index, 7, total, "ExamFlushTable", true);
            return PartialView(datas);
        }

        [Authorize]
        public async Task<ActionResult> ExamContentTable(int id)
        {
            var user = _userService.GetValidUser();
            var examInfo = await _examService.GetExamInfoAsync(user.id, id);

            ViewData["ExamId"] = examInfo?.id ?? id;
            ViewData["Limittime"] = examInfo?.limitTime ?? 0;
            ViewData["ExamName"] = examInfo?.name ?? "";
            return View();
        }

        [Authorize]
        public async Task<JsonResult> RestartExam(int examid, int type)
        {
            var success = await _examService.ReExamAsync(examid, type);
            return Json(new { msg = success ? "操作成功" : "操作失败", success });
        }

        [Authorize]
        public async Task<JsonResult> DeleteExam(int examid)
        {
            var success = await _examService.DeleteExamAsync(examid);
            return Json(new { msg = success ? "操作成功" : "操作失败", success });
        }

        [Authorize]
        public async Task<JsonResult> InsertExamnswer(string data, int examid, int type, int score)
        {
            var user = _userService.GetValidUser();
            var success = await _examService.SubmitExamAnswersAsync(user.id, data, examid, type, score);
            return Json(new { msg = success ? "操作成功" : "操作失败", succss = success });
        }

        [Authorize]
        public async Task<ActionResult> ExamContentList(int id = 1, int type = 1)
        {
            var user = _userService.GetValidUser();
            var (items, limitTime, score) = await _examService.GetExamContentListAsync(user.id, id, type);

            // 映射为旧 DTO（视图兼容）
            var oldDtos = items.Select(q => new ExamContentOutDto
            {
                id = q.Id,
                learnid = q.LearnId,
                isenaudio = q.IsEnAudio,
                isusaudio = q.IsUsAudio,
                lexiconid = q.LexiconId,
                en = q.En,
                cn = q.Cn,
                examid = q.ExamId,
                type = q.Type,
                isok = q.IsOk,
                answer = q.Answer,
                name = q.Name,
                value = q.Value
            }).ToList();

            ViewData["limittime"] = limitTime ?? 0;
            ViewData["score"] = score ?? 0;
            ViewData["isexam"] = limitTime != null && score != null;

            return PartialView(oldDtos);
        }

        /// <summary>
        /// 将新架构 DTO 映射为旧视图 DTO
        /// </summary>
        private static ExamOutDto MapToOldExamOutDto(Application.Dtos.Exam.ExamOutDto dto)
        {
            return new ExamOutDto
            {
                id = dto.Id,
                name = dto.Name,
                userid = dto.UserId,
                count = dto.Count,
                createdate = dto.CreateDate,
                ZyCount = new examnswerCount
                {
                    examid = dto.ZyCount.ExamId,
                    isyk = dto.ZyCount.IsCompleted,
                    type = dto.ZyCount.Type,
                    count = dto.ZyCount.Count
                },
                YzCount = new examnswerCount
                {
                    examid = dto.YzCount.ExamId,
                    isyk = dto.YzCount.IsCompleted,
                    type = dto.YzCount.Type,
                    count = dto.YzCount.Count
                },
                TxCount = new examnswerCount
                {
                    examid = dto.TxCount.ExamId,
                    isyk = dto.TxCount.IsCompleted,
                    type = dto.TxCount.Type,
                    count = dto.TxCount.Count
                }
            };
        }
    }
}
