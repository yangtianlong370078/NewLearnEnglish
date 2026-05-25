# 架构设计文档：LearnEnglish 全面重构

## 上下文

LearnEnglish 是一个英语学习 MVC Web 平台，当前由初级工程师开发，存在严重架构缺陷。项目使用 .NET 10.0、MySQL、MongoDB、Redis，集成了 Whisper 语音识别。需要在保持功能不变的前提下，以超级架构师标准完成全面重构。

**利益相关者**：开发团队、终端用户（英语学习者）
**约束**：保持 MVC 模式、功能不变、ORM 改用 Dapper

---

## 目标 / 非目标

### 目标
- 建立清晰的分层架构（Controller → Service → Repository）
- 消除所有安全漏洞（SQL 注入、弱密码哈希、密钥泄露）
- 通过连接池化和异步管道支持高并发
- 提升代码可维护性和可测试性
- 统一编码规范和命名约定

### 非目标
- 不更改现有业务功能和用户体验
- 不迁移前端框架（保持 Razor Views）
- 不更换数据库引擎（保持 MySQL + MongoDB + Redis）
- 不引入微服务架构（保持单体应用）
- 不引入 CQRS/Event Sourcing 等复杂模式

---

## 决策

### D1：解决方案分层结构

**决策**：采用经典的四层架构，但保持在单个解决方案内。

```
LearnEnglish.sln
├── src/
│   ├── LearnEnglish.Web/              # 表示层 - MVC Controllers, Views, Filters
│   ├── LearnEnglish.Application/      # 应用层 - Service 接口与实现、DTOs
│   ├── LearnEnglish.Domain/           # 领域层 - 实体、枚举、领域异常
│   └── LearnEnglish.Infrastructure/   # 基础设施层 - Dapper Repositories, Redis, MongoDB, 外部 API
└── tests/
    ├── LearnEnglish.UnitTests/
    └── LearnEnglish.IntegrationTests/
```

**考虑的替代方案**：
- ❌ Clean Architecture（Onion）：对于 MVC CRUD 应用过度设计
- ❌ Vertical Slice Architecture：团队不熟悉，学习曲线高
- ✅ 经典 N-Layer：简单直观，团队容易理解，与 MVC 天然契合

**理由**：项目本质是 CRUD+查询的 MVC 应用，经典分层足够清晰，且初级开发者容易上手。

---

### D2：ORM 选型 — Dapper

**决策**：使用 Dapper 作为主要数据访问技术，搭配 Repository 模式封装。

**架构模式**：
```
IRepository<T> (泛型基础)
├── IExamRepository
├── ICourseRepository
├── IWordRepository
├── IUserRepository
└── ...

DapperRepository<T> (泛型基础实现)
├── ExamRepository : DapperRepository<Exam>, IExamRepository
├── CourseRepository : DapperRepository<Course>, ICourseRepository
└── ...
```

**数据库连接管理**：
```csharp
// 连接工厂 — 每次创建新连接，由 Dapper 配合 using 管理生命周期
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public MySqlConnectionFactory(string connectionString) => _connectionString = connectionString;
    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}
```

**考虑的替代方案**：
- ❌ EF Core：用户明确要求使用 Dapper
- ❌ 原生 ADO.NET：当前方式，问题太多
- ❌ SqlSugar/FreeSql：虽流行但非标准，Dapper 社区更大

**理由**：Dapper 轻量高性能、学习曲线低、与原始 SQL 兼容好，适合从原生 ADO.NET 迁移。

---

### D3：认证安全升级

**决策**：
| 项目 | 当前 | 目标 |
|------|------|------|
| 密码哈希 | MD5（无盐） | BCrypt（自动盐） |
| JWT 有效期 | 1 年 | Access Token 2小时 + Refresh Token 7天 |
| 登录端点 | GET + POST | 仅 POST |
| 密钥管理 | 硬编码 | User Secrets / 环境变量 |
| SQL | 字符串拼接 | 参数化查询 |

**密码迁移策略**：
1. 新增 `PasswordVersion` 字段（`0`=MD5, `1`=BCrypt）
2. 用户登录时：如 `PasswordVersion=0`，用 MD5 验证后自动升级为 BCrypt
3. 渐进迁移，无需一次性重置所有密码

**考虑的替代方案**：
- ❌ 强制所有用户重置密码：用户体验差
- ❌ Argon2id：性能好但 .NET 生态支持不如 BCrypt 成熟
- ✅ BCrypt + 渐进迁移：安全且对用户透明

---

### D4：HomeController 拆分策略

**决策**：按领域边界拆分 3854 行的 HomeController 为 6 个独立 Controller + 对应的 Service。

```
HomeController (3854行)
  ├── CourseController       → ICourseService       — 课程 CRUD、分类管理
  ├── WordController         → IWordService         — 单词学习、校准、发音
  ├── StatisticsController   → IStatisticsService   — 学习统计
  ├── FavoriteController     → IFavoriteService      — 收藏功能
  ├── TranslateController    → ITranslateService     — 有道翻译 API
  ├── ImportController       → IImportService        — Excel 数据导入
  └── HomeController (new)   → 仅保留首页 Index
```

**路由兼容策略**：
- 使用 `[Route]` 属性路由保持原有 URL 不变
- 或在拆分初期使用 `RedirectToAction` 进行过渡

---

### D5：异步管道与高并发支持

**决策**：
- 所有数据库操作改为 `async/await`（Dapper 天然支持）
- 使用 `IDbConnection` + `using` 确保连接及时归还连接池
- MySQL 连接字符串配置连接池参数：`Max Pool Size=100;Min Pool Size=5`
- Redis 操作全部异步化
- HTTP 客户端使用 `IHttpClientFactory` 管理生命周期

---

### D6：错误处理策略

**决策**：采用全局异常处理中间件 + `Result<T>` 模式。

```
中间件层: ExceptionHandlingMiddleware (捕获未处理异常，返回统一格式)
Service层: 返回 Result<T> 或抛出领域异常
Controller层: 映射 Result<T> 到 HTTP 响应
```

**统一响应格式**：
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}
```

---

### D7：日志策略

**决策**：引入 Serilog，输出到控制台 + 文件（按日滚动），支持结构化日志。

**理由**：Serilog 是 .NET 生态最流行的日志库，配置灵活、性能优秀、支持结构化日志便于后期接入 ELK 等。

---

### D8：命名规范统一

**决策**：
| 类别 | 规范 | 示例 |
|------|------|------|
| 类名 | PascalCase | `ExamDetail`（非 `examdetail`） |
| 属性 | PascalCase | `UserName`（非 `name`） |
| 私有字段 | _camelCase | `_connectionString` |
| 方法 | PascalCase + Async 后缀 | `GetByIdAsync` |
| 文件夹 | PascalCase | `Entities`（非 `Entitys`） |
| 接口 | I + PascalCase | `IExamRepository` |

---

## 风险 / 权衡

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 密码迁移可能丢失用户 | 中 | 渐进迁移策略，保留 MD5 兼容层 |
| 路由变更导致前端 404 | 高 | 使用属性路由保持 URL 兼容，或统一更新 View |
| Dapper SQL 手写易出错 | 中 | 建立 Repository 基类封装通用 CRUD，编写单元测试 |
| 重构周期长影响交付 | 高 | 分 5 个阶段渐进实施，每阶段独立可部署 |
| Redis 缓存一致性 | 低 | 保持现有 Redis 使用模式，仅优化封装 |

---

## 迁移计划

### 渐进式迁移（非大爆炸）

1. **阶段 1**（项目结构）：创建新项目结构 → 迁移实体和接口 → 确保编译通过
2. **阶段 2**（基础设施）：引入 Dapper + ConnectionFactory → 迁移 Repository → 确保数据访问正常
3. **阶段 3**（安全）：升级认证 → 密码迁移 → 消除 SQL 注入
4. **阶段 4**（业务层）：拆分 Controller → 引入 Service → 逐个迁移 Action
5. **阶段 5**（横切关注点）：统一错误处理 → 日志 → 健康检查

**回滚策略**：每个阶段独立 Git 分支，合并前确保所有功能测试通过。

---

## 已确认决策

1. ✅ 数据库允许新增 `PasswordVersion` 字段，支持 BCrypt 渐进迁移方案
2. 前端 View 中的 AJAX URL 在 Controller 拆分后同步更新
3. ✅ 微信 API（`WXApi`）相关部分保持现状，仅做代码位置迁移（移入 Infrastructure 层），不改变功能实现
4. ✅ 听力模块的 base64 硬编码数据保持现有获取方式，仅做代码层面的组织优化（提取到 Service 层），不迁移数据存储方式
5. ✅ Whisper 模型不需要容器化部署，保持本地文件路径配置方式，通过 `IOptions<WhisperOptions>` 管理路径配置即可
