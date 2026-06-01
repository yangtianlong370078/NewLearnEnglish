using System.IO.Compression;
using LearnEnglish.Application;
using LearnEnglish.Extensions;
using LearnEnglish.Infrastructure;
using LearnEnglish.Infrastructure.HealthChecks;
using LearnEnglish.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

// ========== 1. 配置 Serilog 日志 ==========
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LearnEnglish.WebApi")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/learnenglish-webapi-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}",
        encoding: System.Text.Encoding.UTF8)
    .CreateLogger();

try
{
    Log.Information("========== LearnEnglish.WebApi 应用正在启动 ==========");

    var builder = WebApplication.CreateBuilder(args);


// 1. 添加 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        // 允许所有来源、请求头、请求方法，开发环境使用
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // 【生产环境推荐】指定域名，不要用 AllowAnyOrigin
    // options.AddPolicy("AllowSpecificOrigins", policy =>
    // {
    //     policy.WithOrigins(
    //         "http://localhost:3001",
    //         "http://172.16.31.11:3001" // 你的 Next 地址
    //     )
    //     .AllowAnyHeader()
    //     .AllowAnyMethod()
    //     .AllowCredentials(); // 如果使用 Cookie/JWT 凭证必须开启
    // });
});


    // ========== 2. 使用 Serilog ==========
    builder.Host.UseSerilog();

    // ========== 3. 注册服务 ==========
    // Web API 相关（Controllers + JSON 选项）
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    // Swagger (Swashbuckle)
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "LearnEnglish Web API",
            Version = "v1",
            Description = "LearnEnglish 项目 Web API 接口文档"
        });

        // 仅包含本 WebApi 程序集中的控制器，避免与主项目同名 MVC 控制器冲突
        var webApiAssembly = typeof(LearnEnglish.WebApi.Controllers.ApiControllerBase).Assembly;
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            var actionDescriptor = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            return actionDescriptor?.ControllerTypeInfo.Assembly == webApiAssembly;
        });

        // JWT Bearer 支持
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "请输入 JWT Token（不需要 Bearer 前缀）"
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();

    // ========== 响应压缩（Brotli / Gzip） ==========
    // JSON 文本压缩率通常 80–90%，对 /api/Statistics/StatisticsLearnCountTwo
    // 这类返回较大的接口收益尤其明显。EnableForHttps 在内网/受信任前端场景安全。
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes
            .Concat(new[] { "application/json" });
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

    // CORS：API 一般放开限制，按需调整
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ApiCorsPolicy", policyBuilder =>
        {
            policyBuilder.SetIsOriginAllowed(_ => true)
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .AllowCredentials();
        });
    });

    // JWT 认证由主项目的 AddWebServices 统一注册，这里只补充 Authorization
    builder.Services.AddAuthorization();

    // 主项目中包含一些被控制器需要的类型（UserService/JwtService/WhisperModels 等）
    // 复用主项目的 Web 层服务注册（MVC + Whisper + UserService 等）
    builder.Services.AddWebServices(builder.Configuration);

    // Application / Infrastructure
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // 健康检查
    builder.Services.AddHealthChecksConfiguration(builder.Configuration);

    var app = builder.Build();

    // ========== 中间件管道 ==========
    app.UseGlobalExceptionHandling();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Swagger UI：开发环境启用（可按需放到生产环境）
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LearnEnglish Web API v1");
        options.RoutePrefix = "swagger"; // 访问地址: /swagger
        options.DocumentTitle = "LearnEnglish Web API";
    });

    app.UseHttpsRedirection();

    // 必须在 UseRouting / UseCors 之前启用压缩
    app.UseResponseCompression();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} 响应 {StatusCode} 耗时 {Elapsed:0.000}ms";
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null) return LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
            if (elapsed > 5000) return LogEventLevel.Warning;
            return LogEventLevel.Information;
        };
    });

    // 2. 启用 CORS 中间件（⚠️ 顺序很重要：放在路由之前）
app.UseCors("AllowAllOrigins"); 

    app.UseRouting();
    app.UseCors("ApiCorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapDetailedHealthChecks();

    Log.Information("========== LearnEnglish.WebApi 已启动，环境: {Environment} ==========", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WebApi 启动失败");
    throw;
}
finally
{
    Log.Information("========== LearnEnglish.WebApi 已关闭 ==========");
    Log.CloseAndFlush();
}
