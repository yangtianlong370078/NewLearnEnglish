using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 翻译/代理 API
    /// </summary>
    [Route("api/[controller]")]
    public class TranslateController : ApiControllerBase
    {
        private readonly ITranslateService _translateService;

        public TranslateController(ITranslateService translateService)
        {
            _translateService = translateService;
        }

        /// <summary>查询单词详情（youzack 词典）</summary>
        [HttpGet("xzdc")]
        public async Task<IActionResult> xzdc(string content)
        {
            var result = await _translateService.QueryWordAsync(content);
            return Content(result?.ToString() ?? string.Empty, "application/json");
        }

        /// <summary>代理转发请求（GET/POST 均支持）</summary>
        [HttpGet("Do")]
        [HttpPost("Do")]
        public async Task<IActionResult> Do([FromQuery] string parameters)
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parameters));
            var parts = decoded.Split('|');
            if (parts.Length < 2)
            {
                return Content("参数错误");
            }

            var baseUrl = parts[0];
            var path = parts.Length > 1 ? parts[1] : "";
            var queryString = HttpContext.Request.QueryString.Value?.TrimStart('?');

            string? body = null;
            if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(HttpContext.Request.Body);
                body = await reader.ReadToEndAsync();
            }

            var text = await _translateService.ProxyRequestAsync(baseUrl, path, queryString, body, HttpContext.Request.Method);
            return Content(text ?? string.Empty);
        }
    }
}
