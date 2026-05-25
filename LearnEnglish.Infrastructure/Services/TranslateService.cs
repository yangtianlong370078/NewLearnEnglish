using System.Text;
using LearnEnglish.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 翻译服务实现
    /// </summary>
    public class TranslateService : ITranslateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TranslateService> _logger;

        public TranslateService(IHttpClientFactory httpClientFactory, ILogger<TranslateService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<object?> QueryWordAsync(string word)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("keyword", word)
                });

                var response = await client.PostAsync("https://bdc2.youzack.com/Recitation/Home/FindWord", content);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<object>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<string> ProxyRequestAsync(string baseUrl, string path, string? queryString, string? body, string method)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{baseUrl}{path}";

                if (!string.IsNullOrEmpty(queryString))
                {
                    url += $"?{queryString}";
                }

                HttpResponseMessage response;
                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) && body != null)
                {
                    response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                }
                else
                {
                    response = await client.GetAsync(url);
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
