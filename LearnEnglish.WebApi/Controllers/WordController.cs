using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 单词学习 API
    /// </summary>
    [Route("api/[controller]")]
    public class WordController : ApiControllerBase
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

        /// <summary>检查单词是否已在课程中</summary>
        [HttpGet("WordExist")]
        [Authorize]
        public async Task<IActionResult> WordExist(int kc, string en)
        {
            var (exists, isEnAudio, isUsAudio) = await _wordService.WordExistAsync(kc, en);
            return Ok(new { data = exists, isenaudio = isEnAudio, isusaudio = isUsAudio, success = true });
        }

        /// <summary>课程单词分页列表</summary>
        [HttpGet("WordList")]
        [Authorize]
        public async Task<IActionResult> WordList(int kc = 1, int zt = 1, int tp = 1, string name = "", int index = 1, int pageSize = 30)
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
            return Ok(new { success = true, data = items, total = result.TotalCount, pageIndex = index, pageSize, brs, wlj, yzw });
        }

        /// <summary>收藏单词分页列表</summary>
        [HttpGet("CollectWordList")]
        [Authorize]
        public async Task<IActionResult> CollectWordList(int tp = 1, string name = "", int index = 1)
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
            return Ok(new { success = true, data = items, total = result.TotalCount, pageIndex = index, pageSize = 30 });
        }

        /// <summary>校准学习状态</summary>
        [HttpPost("Calibration")]
        [Authorize]
        public async Task<IActionResult> Calibration()
        {
            var userId = RequireUserId();
            await _wordService.CalibrateNewAsync(userId);
            return Ok(new { msg = "操作成功", success = true });
        }

        /// <summary>设置单词学习状态</summary>
        [HttpPost("szzt")]
        [Authorize]
        public async Task<IActionResult> szzt(int zt, int dqzt, int lexiconId)
        {
            var userId = RequireUserId();
            await _wordService.SetWordStatusAsync(userId, lexiconId, zt);
            return Ok(new { msg = "操作成功", succss = true });
        }

        /// <summary>批量更新单词练习次数</summary>
        [HttpPost("updcnoV2")]
        [Authorize]
        public async Task<IActionResult> updcnoV2([FromForm] string data)
        {
            var userId = RequireUserId();
            await _wordService.ModifyNumberAsync(userId, data);
            return Ok(new { msg = "操作成功", succss = true });
        }

        /// <summary>修改单词英文/中文释义</summary>
        [HttpPost("updc")]
        [Authorize]
        public async Task<IActionResult> updc(int id, string en, string cn)
        {
            var userId = RequireUserId();
            await _wordService.EditWordAsync(userId, id, en, cn);
            return Ok(new { msg = "操作成功", succss = true });
        }

        /// <summary>删除课程内容中的单词</summary>
        [HttpPost("deletedc")]
        [Authorize]
        public async Task<IActionResult> deletedc(int coursecontentId)
        {
            var userId = RequireUserId();
            await _wordService.DeleteWordAsync(userId, coursecontentId);
            return Ok(new { msg = "操作成功", success = true });
        }

        /// <summary>收藏/取消收藏单词</summary>
        [HttpPost("SetCollect")]
        [Authorize]
        public async Task<IActionResult> SetCollect(int lexiconId, bool isCollect)
        {
            var userId = RequireUserId();
            await _wordService.SetCollectAsync(userId, lexiconId, !isCollect ? 1 : 0);
            return Ok(new { msg = "操作成功", succss = true });
        }

        /// <summary>单词详情</summary>
        [HttpGet("lexiconDeatil")]
        [Authorize]
        public async Task<IActionResult> lexiconDeatil(string word, bool iscx)
        {
            var userId = RequireUserId();
            var detail = await _wordService.GetWordDetailAsync(userId, word, 0);
            return Ok(new { success = true, word, iscx, data = detail });
        }
    }
}
