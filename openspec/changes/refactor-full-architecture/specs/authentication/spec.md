## 新增需求

### 需求：安全密码存储
系统必须使用 BCrypt 算法存储用户密码，禁止使用 MD5 或其他不安全的哈希算法，必须支持从旧 MD5 格式渐进迁移。

#### 场景：新用户注册
- **当** 新用户注册时
- **那么** 密码使用 BCrypt 哈希后存储
- **那么** 数据库中 `PasswordVersion` 字段设为 `1`

#### 场景：旧用户登录自动迁移
- **当** `PasswordVersion=0` 的用户使用正确密码登录时
- **那么** 使用 MD5 验证密码
- **那么** 验证通过后自动将密码升级为 BCrypt 格式
- **那么** 将 `PasswordVersion` 更新为 `1`

### 需求：JWT Token 安全配置
系统必须配置合理的 JWT 有效期，Access Token 不超过 2 小时，同时提供 Refresh Token 机制延长会话。

#### 场景：Token 签发
- **当** 用户登录成功时
- **那么** 签发 Access Token（有效期 2 小时）和 Refresh Token（有效期 7 天）
- **那么** JWT Claims 仅包含必要信息（UserId、UserName、Role），禁止包含敏感信息（手机号等）
- **那么** Refresh Token 存储在 Redis 中，支持主动失效

#### 场景：Token 刷新
- **当** Access Token 过期但 Refresh Token 有效时
- **那么** 使用 Refresh Token 换取新的 Access Token
- **那么** 旧 Refresh Token 失效（Token Rotation）

### 需求：仅 POST 方式登录
系统禁止通过 GET 请求进行登录认证，所有认证端点必须使用 POST 方法。

#### 场景：拒绝 GET 登录请求
- **当** 客户端通过 GET 请求访问登录端点时
- **那么** 返回 405 Method Not Allowed

### 需求：密钥安全管理
系统中所有敏感配置（JWT 密钥、数据库密码、微信 AppSecret、百度 API Key 等）禁止硬编码在源代码中，必须通过 User Secrets（开发环境）或环境变量（生产环境）管理。

#### 场景：开发环境密钥管理
- **当** 开发者本地运行项目时
- **那么** 通过 `dotnet user-secrets` 管理敏感配置
- **那么** `appsettings.json` 中不包含真实密钥值

### 需求：全面参数化查询
系统中所有 SQL 查询必须使用参数化方式，禁止任何形式的字符串拼接或插值构建 SQL。

#### 场景：防止 SQL 注入
- **当** 执行任何包含用户输入的 SQL 查询时
- **那么** 使用 `@parameter` 占位符和 Dapper 参数对象传值
- **那么** 代码审查中不存在 `$"...{variable}..."` 形式的 SQL 语句
