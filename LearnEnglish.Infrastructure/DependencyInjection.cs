using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.BackgroundServices;
using LearnEnglish.Infrastructure.Configuration;
using LearnEnglish.Infrastructure.Data;
using LearnEnglish.Infrastructure.MongoDB;
using LearnEnglish.Infrastructure.Redis;
using LearnEnglish.Infrastructure.Repositories;
using LearnEnglish.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LearnEnglish.Infrastructure
{
    /// <summary>
    /// Infrastructure 层依赖注入扩展方法
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 注册 Infrastructure 层所有服务
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ========== 数据库配置 ==========
            services.Configure<DatabaseOptions>(options =>
            {
                options.MySqlConnectionString = configuration.GetConnectionString("lexiconMysqlDb") ?? string.Empty;
            });
            services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

            // ========== MongoDB 配置 ==========
            services.Configure<MongoDbOptions>(configuration.GetSection("lexiconMongoDBOptions"));
            services.AddSingleton<ILexiconDetailRepository, LexiconDetailRepository>();

            // ========== Redis 配置 ==========
            var redisSection = configuration.GetSection("Redis:Default");
            services.AddSingleton(new LearnEnglish.Redis.RedisConfig(redisSection.Get<LearnEnglish.Redis.RedisOption>()!));
            services.AddSingleton<IRedisService, RedisService>();

            // ========== Options 模式配置 ==========
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.Configure<BaiduOptions>(configuration.GetSection("BaiDuConfig"));
            services.Configure<WeChatOptions>(configuration.GetSection("WeChatConfig"));
            services.Configure<WhisperOptions>(configuration.GetSection("Whisper"));
            services.Configure<XfyunOptions>(configuration.GetSection("XfyunConfig"));

            // ========== Repository 注册 ==========
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IMyCourseRepository, MyCourseRepository>();
            services.AddScoped<ICourseContentRepository, CourseContentRepository>();
            services.AddScoped<ILexiconRepository, LexiconRepository>();
            services.AddScoped<ILearnRepository, LearnRepository>();
            services.AddScoped<IMyLexiconRepository, MyLexiconRepository>();
            services.AddScoped<IExamRepository, ExamRepository>();
            services.AddScoped<IExamDetailRepository, ExamDetailRepository>();
            services.AddScoped<IExamAnswerRepository, ExamAnswerRepository>();
            services.AddScoped<IExamRecordRepository, ExamRecordRepository>();
            services.AddScoped<IMyLearnRepository, MyLearnRepository>();
            services.AddScoped<ILearnTaskRepository, LearnTaskRepository>();
            services.AddScoped<IWordQueryRepository, WordQueryRepository>();

            // ========== 安全 & 认证服务注册 ==========
            services.AddHttpContextAccessor();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // ========== 业务服务注册 ==========
            services.AddScoped<ICourseService, Services.CourseService>();
            services.AddScoped<IWordService, Services.WordService>();
            services.AddScoped<IStatisticsService, Services.StatisticsService>();
            services.AddSingleton<IStatisticsVersionService, Services.StatisticsVersionService>();
            services.AddScoped<IFavoriteService, Services.FavoriteService>();
            services.AddScoped<IExamService, Services.ExamService>();
            services.AddScoped<ITranslateService, Services.TranslateService>();
            services.AddScoped<IImportService, Services.ImportService>();

            // ========== 后台任务队列服务 ==========
            services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(capacity: 100));
            services.AddHostedService<QueuedHostedService>();

            return services;
        }
    }
}
