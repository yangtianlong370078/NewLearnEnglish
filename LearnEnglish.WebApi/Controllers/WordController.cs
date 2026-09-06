using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        private readonly IEdgeTtsService _ttsService;
        private readonly ISyllableService _syllableService;
        private readonly IPhonicsService _phonicsService;
        public WordController(IWordService wordService, ICurrentUserService currentUserService, IEdgeTtsService ttsService, ISyllableService syllableService, IPhonicsService phonicsService)
        {
            _wordService = wordService;
            _currentUserService = currentUserService;
            _ttsService = ttsService;
            _syllableService = syllableService;
            _phonicsService = phonicsService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>检查单词是否已在课程中</summary>
        [HttpGet("WordExist")]
        [Authorize]
        public async Task<IActionResult> WordExist( string en)
        {
            var kc = _currentUserService.GetValidUser().CourseId;

            var (exists, isEnAudio, isUsAudio) = await _wordService.WordExistAsync(kc, en);

            return Ok(new { data = exists, success = true });
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

        /// <summary>课程单词分页列表</summary>
        [HttpGet("Words")]
        [Authorize]
        public async Task<IActionResult> Words(int kc = 1, int zt = 1,  string name = "", int index = 1, int pageSize = 30)
        {
            var userId = RequireUserId();
            var (result, brs, wlj, yzw) = await _wordService.GetWordListAsync(userId, kc, zt, 1, name, index, pageSize);
            var items = result.Items.Select(d => new
            {
                d.Id,
                d.LexiconId,
                d.En,
                d.Cn,
                d.IsCollect,
                d.NumberSum,
                d.ZyNumber,
                d.YzNumber,
                d.TxNumber,
                d.FyNumber,
                MyWord = d.IsUpdate
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
        public async Task<IActionResult> lexiconDeatil(string word)
        {
            var userId = RequireUserId();
            var detail = await _wordService.GetWordDetailAsync(userId, word, 0);

            lexicondetail? model = null;
            if (detail != null)
            {
                var json = JsonConvert.SerializeObject(detail);
                model = JsonConvert.DeserializeObject<lexicondetail>(json);

                if (model != null)
                {
                    model.Syllables = _syllableService.GetSyllables(word).ToList();
                    model.PhonicsSplits = _phonicsService.Split(word, model.Syllables)
                        .Select(split => new PhonicsSplit
                        {
                            LetterCombine = split.LetterCombine,
                            PhoneticSymbol = split.PhoneticSymbol
                        })
                        .ToList();
                }
            }

            if (model == null)
            {
                return Ok(new { success = true, word, data = model });
            }

            return Ok(new { success = true, word, data = model });
        }


        /// <summary>
        /// 获取英文发音MP3
        /// </summary>
        /// <param name="text">单词/短语</param>
        /// <param name="voice">音色默认 en-US-JennyNeural</param>
        /// <returns>音频流</returns>
        [HttpGet("speak")]
        public async Task<IActionResult> Speak(
            [FromQuery] string text,
            [FromQuery] string voice = "en-US-JennyNeural")
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("文本不能为空");

            var mp3Bytes = await _ttsService.GetAudioBytesAsync(text, voice);
            return File(mp3Bytes, "audio/mpeg");
        }
    }
}
