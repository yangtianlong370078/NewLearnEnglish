using LearnEnglish.Extensions;
using LearnEnglish.Infrastructure;
using LearnEnglish.Infrastructure.HealthChecks;
using LearnEnglish.Application;
using LearnEnglish.Middleware;
using Serilog;
using Serilog.Events;

// ========== 1. 配置 Serilog 日志（在所有其他代码之前） ==========
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LearnEnglish")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/learnenglish-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}",
        encoding: System.Text.Encoding.UTF8)
    .CreateLogger();

try
{
    Log.Information("========== LearnEnglish 应用正在启动 ==========");

    var builder = WebApplication.CreateBuilder(args);

    // ========== 2. 使用 Serilog 替换默认日志 ==========
    builder.Host.UseSerilog();

    // ========== 3. 注册服务（模块化） ==========
    builder.Services.AddWebServices(builder.Configuration);          // Web 层：MVC、CORS、JWT、Whisper
    builder.Services.AddApplication();                                // Application 层：业务编排（预留）
    builder.Services.AddInfrastructure(builder.Configuration);       // Infrastructure 层：DB、Redis、Repository、Service
    builder.Services.AddHealthChecksConfiguration(builder.Configuration); // 健康检查：MySQL、MongoDB、Redis

    // ========== 4. 构建应用 ==========
    var app = builder.Build();

    // ========== 5. 中间件管道（顺序至关重要！） ==========

    // 全局异常处理中间件（最先注册，捕获所有下游异常）
    app.UseGlobalExceptionHandling();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // Serilog 请求日志（尽早注册以记录所有请求，包括静态文件）
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} 响应 {StatusCode} 耗时 {Elapsed:0.000}ms";
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null) return LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
            if (elapsed > 5000) return LogEventLevel.Warning;  // 超过5秒的慢请求
            return LogEventLevel.Information;
        };
    });

    // 静态文件（在 Routing 之前，避免静态文件走完整管道）
    app.UseStaticFiles();

    // 路由 → CORS → 认证 → 授权（ASP.NET Core 标准顺序）
    app.UseRouting();
    app.UseCors("MyCorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();

    // ========== 6. 端点映射 ==========
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // 健康检查端点
    app.MapDetailedHealthChecks();

    // SPA 前端回退：未匹配的路由返回 React 应用入口
    app.MapFallbackToFile("spa/index.html");

    Log.Information("========== LearnEnglish 应用已启动，环境: {Environment} ==========", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用启动失败！");
    throw;
}
finally
{
    Log.Information("========== LearnEnglish 应用已关闭 ==========");
    Log.CloseAndFlush();
}