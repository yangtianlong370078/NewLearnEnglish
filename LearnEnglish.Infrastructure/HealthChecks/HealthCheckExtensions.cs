using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace LearnEnglish.Infrastructure.HealthChecks
{
    /// <summary>
    /// 健康检查端点配置扩展方法
    /// </summary>
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// 映射健康检查端点，返回详细的 JSON 格式健康报告
        /// </summary>
        public static IEndpointRouteBuilder MapDetailedHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = WriteDetailedResponse
            });

            // 按标签分组的端点
            endpoints.MapHealthChecks("/health/db", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("db"),
                ResponseWriter = WriteDetailedResponse
            });

            endpoints.MapHealthChecks("/health/cache", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("cache"),
                ResponseWriter = WriteDetailedResponse
            });

            return endpoints;
        }

        /// <summary>
        /// 将健康检查结果以结构化 JSON 格式写入响应
        /// </summary>
        private static async Task WriteDetailedResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds.ToString("0.00") + "ms",
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds.ToString("0.00") + "ms",
                    description = entry.Value.Description,
                    error = entry.Value.Exception?.Message,
                    tags = entry.Value.Tags
                })
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsJsonAsync(response, options);
        }
    }
}
