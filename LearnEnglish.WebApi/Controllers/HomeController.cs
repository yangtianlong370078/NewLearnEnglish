using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models.ErrorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 首页相关 API
    /// </summary>
    [Route("api/[controller]")]
    public class HomeController : ApiControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserService _userService;
        private readonly IWordService _wordService;

        public HomeController(ILogger<HomeController> logger, UserService userService, IWordService wordService)
        {
            _logger = logger;
            _userService = userService;
            _wordService = wordService;
        }

        /// <summary>获取用户信息及 VIP 剩余时长</summary>
        [HttpGet("GetUiserInfo")]
        [Authorize]
        public IActionResult GetUiserInfo()
        {
            var user = _userService.GetValidUser();

            DateTime LastMenses = user.enddate;
            DateTime dtLast = new DateTime(LastMenses.Year, LastMenses.Month, LastMenses.Day);
            DateTime dtThis = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            int Day = new TimeSpan(dtLast.Ticks - dtThis.Ticks).Days;
            TimeSpan ts = LastMenses.Subtract(DateTime.Now).Duration();
            int Hour = ts.Hours;
            int Minute = ts.Minutes;

            return Ok(new { Day, Hour, Minute, success = true });
        }

        /// <summary>批量修改学习记录数量</summary>
        [HttpPost("updcno")]
        [Authorize]
        public async Task<IActionResult> updcno([FromForm] string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return Ok(new { msg = "操作失败", succss = false });
            }
            var user = _userService.GetValidUser();
            var success = await _wordService.BatchUpdateNumberAsync(user.id, data);
            return Ok(new { msg = success ? "操作成功" : "操作失败", succss = success });
        }
    }
}
