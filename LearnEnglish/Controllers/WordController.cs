using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models;
using LearnEnglish.Models.Dtos;
using LearnEnglish.Models.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 单词学习控制器
    /// </summary>
    public class WordController : BaseController
    {
        private readonly IWordService _wordService;
        private readonly ICurrentUserService _currentUserService;

        public WordController(IWordService wordService, ICurrentUserService currentUserService)
        {
            _wordService = wordService;
            _currentUserService = currentUserService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>
        /// 检查单词是否存在于课程中
        /// </summary>
        [Authorize]
        public async Task<JsonResult> WordExist(int kc, string en)
        {
            var (exists, isEnAudio, isUsAudio) = await _wordService.WordExistAsync(kc, en);
            return Json(new { data = exists, isenaudio = isEnAudio, isusaudio = isUsAudio, success = true });
        }

        /// <summary>
        /// 课程单词分页列表（PartialView）
        /// </summary>
        [Authorize]
        public async Task<ActionResult> learnEnglishList(int kc = 1, int zt = 1, int tp = 1, string name = "", int index = 1, int pageSize = 30)
        {
            var userId = RequireUserId();
            var (result, brs, wlj, yzw) = await _wordService.GetWordListAsync(userId, kc, zt, tp, name, index, pageSize);

            var oldItems = result.Items.Select(MapToOldShowTranslateDto).ToList();
            var pagedList = new PagedList<ShowTranslateDto>(oldItems, index, pageSize, result.TotalCount, "learnEnglishLFlushTable", true);

            ViewData["tp"] = tp;
            ViewData["brs"] = brs;
            ViewData["wlj"] = wlj;
            ViewData["yzw"] = yzw;
            return PartialView("~/Views/Home/learnEnglishList.cshtml", pagedList);
        }

        /// <summary>
        /// 收藏单词分页列表（PartialView）
        /// </summary>
        [Authorize]
        public async Task<ActionResult> learnEnglishCollectList(int kc = 1, int zt = 1, int tp = 1, string name = "", int index = 1)
        {
            var userId = RequireUserId();
            var result = await _wordService.GetFavoriteListAsync(userId, tp, name, index, 30);

            var oldItems = result.Items.Select(MapToOldShowTranslateDto).ToList();
            var pagedList = new PagedList<ShowTranslateDto>(oldItems, index, 30, result.TotalCount, "learnEnglishLFlushTable", true);

            ViewData["tp"] = tp;
            ViewData["brs"] = 0;
            ViewData["wlj"] = 0;
            ViewData["yzw"] = 0;
            return PartialView("~/Views/Home/learnEnglishList.cshtml", pagedList);
        }

        /// <summary>
        /// 课程单词分页列表 JSON API（供 SPA 前端使用）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> WordListJson(int kc = 1, int zt = 1, int tp = 1, string name = "", int index = 1, int pageSize = 30)
        {
            var userId = RequireUserId();
            var (result, brs, wlj, yzw) = await _wordService.GetWordListAsync(userId, kc, zt, tp, name, index, pageSize);
            var items = result.Items.Select(d => new
            {
                d.Id,
                d.LexiconId,
                d.CourseContentId,
                en = d.En,
                cn = d.Cn,
                d.Zt,
                isCollect = d.IsCollect == 1,
                d.IsEnAudio,
                d.IsUsAudio,
                d.NumberSum,
                d.ZyNumber,
                d.YzNumber,
                d.TxNumber,
                d.FyNumber,
                d.Name,
                d.Value,
            }).ToList();
            return Json(new { success = true, data = items, total = result.TotalCount, pageIndex = index, pageSize, brs, wlj, yzw });
        }

        /// <summary>
        /// 收藏单词分页列表 JSON API（供 SPA 前端使用）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> CollectWordListJson(int kc = 1, int zt = 1, int tp = 1, string name = "", int index = 1)
        {
            var userId = RequireUserId();
            var result = await _wordService.GetFavoriteListAsync(userId, tp, name, index, 30);
            var items = result.Items.Select(d => new
            {
                d.Id,
                d.LexiconId,
                d.CourseContentId,
                en = d.En,
                cn = d.Cn,
                d.Zt,
                isCollect = d.IsCollect == 1,
                d.IsEnAudio,
                d.IsUsAudio,
                d.NumberSum,
                d.ZyNumber,
                d.YzNumber,
                d.TxNumber,
                d.FyNumber,
                d.Name,
                d.Value,
            }).ToList();
            return Json(new { success = true, data = items, total = result.TotalCount, pageIndex = index, pageSize = 30 });
        }

        /// <summary>
        /// 校准学习状态（新版）
        /// </summary>
        [Authorize]
        public async Task<JsonResult> Calibration()
        {
            var userId = RequireUserId();
            var (changed, added, removed) = await _wordService.CalibrateNewAsync(userId);
            return Json(new { msg = "操作成功", success = true });
        }

        /// <summary>
        /// 设置单词学习状态
        /// </summary>
        [Authorize]
        public async Task<JsonResult> szzt(int zt, int dqzt, int lexiconId)
        {
            var userId = RequireUserId();
            await _wordService.SetWordStatusAsync(userId, lexiconId, zt);
            return Json(new { msg = "操作成功", succss = true });
        }

        /// <summary>
        /// 批量更新单词练习次数
        /// </summary>
        [Authorize]
        public async Task<JsonResult> updcnoV2(string data)
        {
            var userId = RequireUserId();
            await _wordService.ModifyNumberAsync(userId, data);
            return Json(new { msg = "操作成功", succss = true });
        }

        /// <summary>
        /// 修改单词英文/中文释义
        /// </summary>
        [Authorize]
        public async Task<JsonResult> updc(int id, string en, string cn)
        {
            var userId = RequireUserId();
            await _wordService.EditWordAsync(userId, id, en, cn);
            return Json(new { msg = "操作成功", succss = true });
        }

        /// <summary>
        /// 删除课程内容中的单词
        /// </summary>
        [Authorize]
        public async Task<JsonResult> deletedc(int coursecontentId)
        {
            var userId = RequireUserId();
            await _wordService.DeleteWordAsync(userId, coursecontentId);
            return Json(new { msg = "操作成功", success = true });
        }

        /// <summary>
        /// 收藏/取消收藏单词
        /// </summary>
        [Authorize]
        public async Task<JsonResult> SetCollect(int lexiconId, bool isCollect)
        {
            var userId = RequireUserId();
            await _wordService.SetCollectAsync(userId, lexiconId, !isCollect ? 1 : 0);
            return Json(new { msg = "操作成功", succss = true });
        }

        /// <summary>
        /// 单词详情页面
        /// </summary>
        [Authorize]
        public async Task<ActionResult> lexiconDeatil(string word, bool iscx)
        {
            var userId = RequireUserId();
            var detail = await _wordService.GetWordDetailAsync(userId, word, 0);

            // 将 Service 返回的 object 映射为旧视图模型（View 使用 @model lexicondetail）
            lexicondetail? model = null;
            if (detail != null)
            {
                var json = JsonConvert.SerializeObject(detail);
                model = JsonConvert.DeserializeObject<lexicondetail>(json);
            }

            ViewData["word"] = word;
            ViewData["iscx"] = iscx;
            return View("~/Views/Home/lexiconDeatil.cshtml", model);
        }

        #region 新 DTO → 旧视图模型映射

        /// <summary>
        /// 将新 ShowTranslateDto 映射为旧视图 ShowTranslateDto
        /// </summary>
        private static ShowTranslateDto MapToOldShowTranslateDto(Application.Dtos.Word.ShowTranslateDto dto)
        {
            return new ShowTranslateDto
            {
                id = dto.Id,
                userId = dto.UserId,
                coursecontentId = dto.CourseContentId,
                lexiconId = dto.LexiconId,
                iscollect = dto.IsCollect,
                numbersum = dto.NumberSum,
                zynumber = dto.ZyNumber,
                yznumber = dto.YzNumber,
                txnumber = dto.TxNumber,
                fynumber = dto.FyNumber,
                en = dto.En,
                cn = dto.Cn,
                name = dto.Name,
                value = dto.Value,
                zt = dto.Zt,
                isenaudio = dto.IsEnAudio,
                isusaudio = dto.IsUsAudio,
                isUpdate = dto.IsUpdate
            };
        }

        #endregion
    }
}
