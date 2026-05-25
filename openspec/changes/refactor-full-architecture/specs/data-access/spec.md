## 新增需求

### 需求：数据库连接工厂
系统必须通过 `IDbConnectionFactory` 接口管理数据库连接的创建，禁止在业务代码中直接 `new MySqlConnection()`，连接必须通过 `using` 语句管理生命周期。

#### 场景：获取数据库连接
- **当** Repository 需要执行数据库操作时
- **那么** 通过注入的 `IDbConnectionFactory.CreateConnection()` 获取连接
- **那么** 连接使用 `using` 语句包裹，确保自动释放
- **那么** 连接字符串由配置文件统一管理，支持连接池参数

### 需求：Dapper Repository 基类
系统必须提供泛型 `DapperRepository<T>` 基类，封装 CRUD 通用操作（`GetByIdAsync`、`GetAllAsync`、`InsertAsync`、`UpdateAsync`、`DeleteAsync`），各领域 Repository 继承基类并扩展领域特有查询。

#### 场景：执行参数化查询
- **当** Repository 执行任何 SQL 查询时
- **那么** 必须使用 Dapper 参数化查询（`@parameter` 占位符）
- **那么** 禁止使用字符串插值或拼接构建 SQL
- **那么** 所有查询方法为异步方法（返回 `Task<T>`）

#### 场景：通用 CRUD 操作
- **当** 任何实体需要基础增删改查时
- **那么** 通过基类提供的通用方法完成，无需重复编写 SQL
- **那么** 实体类使用 `[Table]`、`[Key]` 等属性标注映射关系

### 需求：MongoDB 服务统一封装
系统必须提供统一的 `IMongoRepository<T>` 接口和实现，替代当前 Controller 中手动创建 MongoDB 客户端的方式。

#### 场景：MongoDB 文档操作
- **当** 需要查询或写入 MongoDB 文档时
- **那么** 通过注入的 `IMongoRepository<T>` 操作
- **那么** MongoDB 客户端作为单例注册在 DI 容器中
- **那么** 集合名称通过配置或约定自动推导

### 需求：Redis 服务重构
系统必须重构 Redis 封装层，修复现有 Bug，所有 Redis 操作异步化，提供类型安全的泛型方法。

#### 场景：Redis 缓存操作
- **当** 需要读写 Redis 缓存时
- **那么** 通过注入的 `IRedisService` 接口操作
- **那么** 提供 `GetAsync<T>`、`SetAsync<T>`、`HashGetAsync` 等类型安全方法
- **那么** `HashExistsAsync` 必须正确调用底层 `HashExistsAsync`（修复当前调用 `HashGetAsync` 的 Bug）

### 需求：分页查询支持
系统必须提供统一的分页查询能力，`PagedList<T>` 仅包含数据和分页元信息，禁止在数据模型中嵌入 HTML 生成逻辑。

#### 场景：获取分页数据
- **当** 执行分页查询时
- **那么** 返回 `PagedList<T>` 包含 `Items`、`TotalCount`、`PageIndex`、`PageSize`
- **那么** 分页 HTML 渲染由 View 层的 TagHelper 或 Partial View 负责
- **那么** `PagedList<T>` 不包含任何 HTML 或 JavaScript 代码
