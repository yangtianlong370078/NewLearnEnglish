# 变更：全面重构 LearnEnglish 项目架构

## 为什么

当前项目由初级工程师开发，存在严重的架构缺陷：HomeController 长达 3854 行（God Class）、无数据访问层（原始 ADO.NET SQL 散落在 Controller 中）、多处 SQL 注入漏洞、MD5 无盐密码存储、密钥硬编码、命名不规范等问题。项目无法支撑高并发场景，也不具备可维护性和可测试性。需要以超级架构师标准进行全面重构，在保持功能不变的前提下，采用业界最佳实践重塑项目。

## 变更内容

### 阶段 1：项目结构重塑
- **重大变更**：重新组织解决方案结构，拆分为多层架构（Web / Application / Domain / Infrastructure）
- 引入 Dapper 替代原生 ADO.NET，建立 Repository 模式
- 建立统一的依赖注入容器配置
- 统一代码命名规范（PascalCase）

### 阶段 2：基础设施层重构
- 建立数据库连接工厂（`IDbConnectionFactory`），替代手动 `MySqlConnection` 管理
- 重构 Redis 封装层，修复已知 Bug（`HashExists` 调用错误等）
- 重构 MongoDB 服务层，消除重复模型定义
- 统一配置管理，使用 Options 模式，密钥移入安全存储

### 阶段 3：认证与安全重构
- **重大变更**：密码存储从 MD5 升级为 BCrypt
- **重大变更**：移除 AuthController 的 GET 登录端点（安全漏洞）
- JWT 有效期从 1 年缩短为合理时长，引入 Refresh Token 机制
- 消除所有 SQL 注入漏洞（全部改为参数化查询）
- 微信 AppSecret 等密钥从代码移入配置

### 阶段 4：Controller 拆分与 Service 层引入
- 拆分 HomeController（3854 行）为多个领域 Controller：
  - `CourseController` — 课程管理
  - `WordController` — 单词学习
  - `StatisticsController` — 学习统计
  - `FavoriteController` — 收藏功能
  - `TranslateController` — 翻译 API
  - `ImportController` — 数据导入
- 引入 Service 层，Controller 仅负责 HTTP 关注点
- 重构 ExamController、LoginController、HearingController、WhisperController

### 阶段 5：横切关注点
- 统一异常处理中间件（替代当前不完整的 Filter 方案）
- 引入结构化日志（Serilog）
- 引入健康检查端点
- API 响应标准化（统一 `ApiResponse<T>` 封装）
- 清理死代码（空的 `UserData` 类、注释代码、未使用的引用）

## 影响

- 受影响规范：`project-structure`、`data-access`、`authentication`、`course-management`、`exam-management`、`speech-recognition`、`error-handling`、`configuration`
- 受影响代码：**整个项目所有文件**
- 数据库：密码字段需要迁移（MD5 → BCrypt），需提供迁移脚本
- 前端视图：Controller/Action 路由可能变更，需同步更新 View 中的 URL 引用
- 部署：需要新增 User Secrets 或环境变量配置

## 约束
- 功能保持不变，仅重构实现方式
- 保持 MVC 模式
- ORM 从原生 ADO.NET 改为 Dapper
- 保持对 MySQL、MongoDB、Redis 三种数据存储的支持
- 微信 API 相关部分保持现有功能实现，仅迁移代码位置
- 听力模块 base64 数据保持现有获取方式，不迁移存储
- Whisper 模型保持本地文件路径部署方式
- 数据库允许新增字段（如 `PasswordVersion`）
