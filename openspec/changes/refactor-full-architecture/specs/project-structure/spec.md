## 新增需求

### 需求：分层解决方案结构
系统必须采用四层架构组织代码，包含 Web（表示层）、Application（应用层）、Domain（领域层）、Infrastructure（基础设施层）四个独立项目，各层通过接口解耦，依赖方向从上到下单向引用。

#### 场景：项目引用关系正确
- **当** 查看解决方案项目引用时
- **那么** Web 引用 Application 和 Infrastructure
- **那么** Application 引用 Domain
- **那么** Infrastructure 引用 Domain 和 Application（实现接口）
- **那么** Domain 不引用任何其他项目

#### 场景：各层职责边界清晰
- **当** 开发者需要添加新功能时
- **那么** Controller 仅处理 HTTP 请求/响应映射，不包含业务逻辑
- **那么** Service（Application 层）包含业务逻辑编排
- **那么** Repository（Infrastructure 层）包含数据访问实现
- **那么** Entity（Domain 层）包含领域模型定义

### 需求：统一命名规范
系统中所有代码必须遵循 C# 标准命名约定：类名 PascalCase、私有字段 _camelCase、公有属性 PascalCase、异步方法以 Async 后缀结尾、文件夹名 PascalCase。

#### 场景：实体类命名规范化
- **当** 查看领域实体类时
- **那么** 类名使用 PascalCase（如 `Exam` 而非 `exam`，`ExamDetail` 而非 `examdetail`）
- **那么** 属性名使用 PascalCase（如 `UserName` 而非 `name`）
- **那么** 文件夹名使用正确拼写（如 `Entities` 而非 `Entitys`）

### 需求：依赖注入容器统一配置
系统必须在 `Program.cs` 中通过扩展方法分类注册所有服务，禁止在 Controller 构造函数中手动创建依赖实例。

#### 场景：服务注册模块化
- **当** 查看 `Program.cs` 启动配置时
- **那么** 数据库服务通过 `AddInfrastructure()` 扩展方法注册
- **那么** 应用服务通过 `AddApplication()` 扩展方法注册
- **那么** 认证服务通过 `AddAuthentication()` 扩展方法注册
- **那么** 所有服务均通过构造函数注入获取，不存在 `new` 直接实例化的情况

### 需求：无死代码
系统中禁止存在空类（如空的 `UserData`）、大段注释代码、未使用的 `using` 引用和未使用的 NuGet 包。

#### 场景：清理无用代码
- **当** 代码审查时
- **那么** 不存在空的占位类
- **那么** 不存在被注释掉的大段代码块
- **那么** 不存在未使用的命名空间引用
- **那么** 不存在未引用的 NuGet 包（如 `Nancy`）
