namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 翻译服务接口
    /// </summary>
    public interface ITranslateService
    {
        /// <summary>
        /// 从外部 API 查询单词详情（youzack 词典）
        /// </summary>
        Task<object?> QueryWordAsync(string word);

        /// <summary>
        /// 代理转发请求到本地服务
        /// </summary>
        Task<string> ProxyRequestAsync(string baseUrl, string path, string? queryString, string? body, string method);
    }
}
