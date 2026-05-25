using LearnEnglish.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 翻译/代理请求控制器
    /// </summary>
    public class TranslateController : BaseController
    {
        private readonly ITranslateService _translateService;

        public TranslateController(ITranslateService translateService)
        {
            _translateService = translateService;
        }

        /// <summary>
        /// 查询单词详情（youzack 词典）
        /// </summary>
        public async Task<string> xzdc(string content)
        {
            var result = await _translateService.QueryWordAsync(content);
            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 代理转发请求
        /// </summary>
        public async Task<string> Do([FromQuery] string parameters)
        {
            // 解码 Base64 参数
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parameters));
            var parts = decoded.Split('|');
            if (parts.Length < 2)
                return "参数错误";

            var baseUrl = parts[0];
            var path = parts.Length > 1 ? parts[1] : "";
            var queryString = HttpContext.Request.QueryString.Value?.TrimStart('?');

            string? body = null;
            if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(HttpContext.Request.Body);
                body = await reader.ReadToEndAsync();
            }

            return await _translateService.ProxyRequestAsync(baseUrl, path, queryString, body, HttpContext.Request.Method);
        }
    }
}
