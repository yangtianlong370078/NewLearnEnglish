using LearnEnglish.Application.Dtos.Exam;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Common;
using LearnEnglish.Models;
using LearnEnglish.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 考试控制器（重构版，使用 IExamService）
    /// </summary>
    public class ExamV2Controller : BaseController
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

        /// <summary>
        /// 创建考试
        /// </summary>
        [Authorize]
        public async Task<JsonResult> SaveExam(int examcount, int limittime)
        {
            var userId = RequireUserId();
            var (success, message, wordCount) = await _examService.CreateExamAsync(userId, examcount, limittime);
            return Json(new { msg = message, success });
        }

        /// <summary>
        /// 获取考试详情
        /// </summary>
        public async Task<JsonResult> GetExam(int examid)
        {
            var result = await _examService.GetExamDetailAsync(examid);
            return Json(new { data = result });
        }

        /// <summary>
        /// 考试列表页面（PartialView）
        /// </summary>
        [Authorize]
        public async Task<ActionResult> ExamList(int index = 1, string word = "")
        {
            var userId = RequireUserId();
            var (list, total) = await _examService.GetExamListAsync(userId, word, index, 7);

            // 映射为旧 DTO（视图兼容）
            var oldDtos = list.Select(MapToOldExamOutDto).ToList();
            ViewData["word"] = word;
            var pagedList = new Models.PagedList<Models.Dtos.ExamOutDto>(oldDtos, index, 7, total, "ExamFlushTable", true);
            return PartialView("~/Views/Exam/ExamList.cshtml", pagedList);
        }

        /// <summary>
        /// 考试内容表格页面
        /// </summary>
        [Authorize]
        public async Task<ActionResult> ExamContentTable(int id)
        {
            var userId = RequireUserId();
            var examInfo = await _examService.GetExamInfoAsync(userId, id);

            ViewData["ExamId"] = examInfo?.id ?? id;
            ViewData["Limittime"] = examInfo?.limitTime ?? 0;
            ViewData["ExamName"] = examInfo?.name ?? "";
            return View("~/Views/Exam/ExamContentTable.cshtml");
        }

        /// <summary>
        /// 重新考试
        /// </summary>
        [Authorize]
        public async Task<JsonResult> RestartExam(int examid, int type)
        {
            await _examService.ReExamAsync(examid, type);
            return Json(new { msg = "操作成功", success = true });
        }

        /// <summary>
        /// 删除考试
        /// </summary>
        [Authorize]
        public async Task<JsonResult> DeleteExam(int examid)
        {
            await _examService.DeleteExamAsync(examid);
            return Json(new { msg = "操作成功", success = true });
        }

        /// <summary>
        /// 提交考试答案（批量）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> InsertExamnswer(string data, int examid, int type, int score)
        {
            var userId = RequireUserId();
            await _examService.SubmitExamAnswersAsync(userId, data, examid, type, score);
            return Json(new { msg = "操作成功", succss = true });
        }

        /// <summary>
        /// 考试内容列表（PartialView）
        /// </summary>
        [Authorize]
        public async Task<ActionResult> ExamContentList(int id = 1, int type = 1)
        {
            var userId = RequireUserId();
            var (items, limitTime, score) = await _examService.GetExamContentListAsync(userId, id, type);

            // 映射为旧 DTO（视图兼容）
            var oldDtos = items.Select(q => new Models.Dtos.ExamContentOutDto
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

            return PartialView("~/Views/Exam/ExamContentList.cshtml", oldDtos);
        }
        /// <summary>
        /// 考试列表 JSON API（供 SPA 前端使用）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> ExamListJson(int index = 1, string word = "")
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
            return Json(new { success = true, data = items, total, pageIndex = index, pageSize = 7 });
        }

        /// <summary>
        /// 考试内容 JSON API（供 SPA 前端使用）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> ExamContentListJson(int id = 1, int type = 1)
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
            return Json(new { success = true, data, limittime = limitTime ?? 0, score = score ?? 0 });
        }

        /// <summary>
        /// 将新架构 DTO 映射为旧视图 DTO
        /// </summary>
        private static Models.Dtos.ExamOutDto MapToOldExamOutDto(Application.Dtos.Exam.ExamOutDto dto)
        {
            return new Models.Dtos.ExamOutDto
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
        }    }
}
