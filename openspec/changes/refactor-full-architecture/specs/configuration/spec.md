## 新增需求

### 需求：Options 模式配置管理
系统中所有配置项必须使用强类型 Options 模式（`IOptions<T>` / `IOptionsSnapshot<T>`）绑定，禁止在代码中直接通过 `IConfiguration.GetSection().Value` 读取字符串。

#### 场景：JWT 配置
- **当** 需要读取 JWT 配置时
- **那么** 通过注入 `IOptions<JwtOptions>` 获取强类型配置对象
- **那么** `JwtOptions` 包含 `Key`、`Issuer`、`Audience`、`AccessTokenExpireMinutes`、`RefreshTokenExpireDays`

#### 场景：数据库连接配置
- **当** 需要读取数据库连接字符串时
- **那么** 通过 `IOptions<DatabaseOptions>` 获取
- **那么** `DatabaseOptions` 包含 `MySqlConnectionString`、连接池参数

#### 场景：外部 API 配置
- **当** 需要读取外部 API 密钥时
- **那么** 通过 `IOptions<WeChatOptions>`、`IOptions<BaiduOptions>`、`IOptions<WhisperOptions>` 等获取
- **那么** 开发环境密钥通过 User Secrets 覆盖

### 需求：CORS 配置规范化
系统的 CORS 策略必须从配置文件读取允许的源列表，禁止在代码中硬编码 URL，禁止在生产环境使用 `AllowAnyOrigin`。

#### 场景：CORS 策略配置
- **当** 配置 CORS 策略时
- **那么** 允许的源从 `appsettings.json` 的 `Cors:AllowedOrigins` 数组读取
- **那么** 不同环境（Development / Production）可配置不同的允许源

### 需求：连接池优化配置
系统的 MySQL 连接字符串必须包含连接池优化参数，支持高并发场景。

#### 场景：MySQL 连接池
- **当** 系统在高并发场景运行时
- **那么** 连接字符串包含 `Max Pool Size`、`Min Pool Size`、`Connection Lifetime` 参数
- **那么** 连接池大小可通过配置调整
