using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models.ErrorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace LearnEnglish.Controllers
{
    public class HomeController : BaseController
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

        /// <summary>
        /// 获取用户信息
        /// </summary>
        [Authorize]
        public JsonResult GetUiserInfo()
        {
            var user = _userService.GetValidUser();

            DateTime LastMenses = user.enddate;
            DateTime dtLast = new DateTime(Convert.ToInt32(LastMenses.Year), Convert.ToInt32(LastMenses.Month), Convert.ToInt32(LastMenses.Day));
            DateTime dtThis = new DateTime(Convert.ToInt32(DateTime.Now.Year), Convert.ToInt32(DateTime.Now.Month), Convert.ToInt32(DateTime.Now.Day));
            int Day = new TimeSpan(dtLast.Ticks - dtThis.Ticks).Days;
            TimeSpan ts = LastMenses.Subtract(DateTime.Now).Duration();
            int Hour = ts.Hours;
            int Minute = ts.Minutes;

            ViewData["enddate"] = user.enddate;
            return Json(new { Day = Day, Hour = Hour, Minute = Minute, success = true });
        }

        public IActionResult Index(int id = 0, string pagename = "")
        {
            ViewData["wxid"] = id;
            ViewData["wxpagename"] = pagename;
            return View();
        }

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult My()
        {
            return View();
        }

        /// <summary>
        /// 修改学习记录数量
        /// </summary>
        [Authorize]
        public async Task<JsonResult> updcno(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return Json(new { msg = "操作失败", succss = false });
            }
            var user = _userService.GetValidUser();
            var success = await _wordService.BatchUpdateNumberAsync(user.id, data);
            return Json(new { msg = success ? "操作成功" : "操作失败", succss = success });
        }
    }
}

/// <summary>
/// MongoDB ObjectId JSON转换器
/// </summary>
public class ObjectIdConverter : Newtonsoft.Json.JsonConverter<ObjectId>
{
    public override ObjectId ReadJson(JsonReader reader, Type objectType, ObjectId existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        var value = reader.Value?.ToString();
        return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
    }
    public override void WriteJson(JsonWriter writer, ObjectId value, Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}

/// <summary>
/// 学习记录修改模型
/// </summary>
public class upnoModil
{
    public int id { get; set; }
    public int no { get; set; }
    public string type { get; set; }
}
