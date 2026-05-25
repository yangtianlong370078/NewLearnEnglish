using LearnEnglish.Models;
using LearnEnglish.Models.ErrorHelper;
using LearnEnglish.Models.MongoDB;
using LearnEnglish.WhisperModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text;

namespace LearnEnglish.Extensions
{
    /// <summary>
    /// Web 层服务注册扩展方法
    /// 将 Program.cs 中的服务注册逻辑模块化
    /// </summary>
    public static class WebServiceExtensions
    {
        /// <summary>
        /// 注册 Web 层所有服务（CORS、JWT、Whisper、旧服务兼容等）
        /// </summary>
        public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ========== MVC ==========
            services.AddControllersWithViews();

            // ========== CORS 策略 ==========
            services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", builder =>
                {
                    builder.WithOrigins("https://marketimages.oss-cn-shanghai.aliyuncs.com")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            // ========== HttpClient 工厂 ==========
            services.AddHttpClient();

            // ========== Whisper 语音识别 ==========
            services.AddWhisperTranscription(configuration);

            // ========== JWT 认证 ==========
            var jwtKey = configuration.GetSection("Jwt:Key").Value
                ?? throw new InvalidOperationException("JWT Key 未在配置中设置");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = configuration.GetSection("Jwt:Issuer").Value,
                    ValidAudience = configuration.GetSection("Jwt:Audience").Value,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ========== 旧服务兼容（待后续阶段逐步移除） ==========
            services.AddScoped<JwtService>();
            services.Configure<lexiconMongoDBOptions>(configuration.GetSection(nameof(lexiconMongoDBOptions)));
            services.AddScoped<UserService>();
            services.AddSingleton<ErrorHandlerService>();
            services.AddScoped<InvalidUserExceptionFilter>(provider =>
                new InvalidUserExceptionFilter(provider.GetService<ErrorHandlerService>()!));

            return services;
        }

        /// <summary>
        /// 注册健康检查服务（MySQL、MongoDB、Redis）
        /// </summary>
        public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var mysqlConnStr = configuration.GetConnectionString("lexiconMysqlDb") ?? string.Empty;
            var mongoConnStr = configuration.GetSection("lexiconMongoDBOptions:ConnectionString").Value ?? "mongodb://localhost:27017";
            var redisConnStr = configuration.GetSection("Redis:Default:Connection").Value ?? "localhost:6379";

            services.AddHealthChecks()
                .AddMySql(mysqlConnStr, name: "mysql", tags: new[] { "db", "mysql" })
                .AddMongoDb(sp => new MongoClient(mongoConnStr), name: "mongodb", tags: new[] { "db", "mongodb" })
                .AddRedis(sp => ConnectionMultiplexer.Connect(redisConnStr), name: "redis", tags: new[] { "cache", "redis" });

            return services;
        }
    }
}
