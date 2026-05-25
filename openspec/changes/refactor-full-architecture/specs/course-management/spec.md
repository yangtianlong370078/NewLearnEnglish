## 新增需求

### 需求：课程管理服务
系统必须通过 `ICourseService` 提供课程的 CRUD 操作，包括课程分类列表、课程内容获取、课程创建和更新，从 HomeController 中拆分为独立的 `CourseController`。

#### 场景：获取课程分类列表
- **当** 用户请求课程分类时
- **那么** 通过 `ICourseService.GetCategoriesAsync()` 获取
- **那么** 数据从 MySQL 通过 `ICourseRepository` 查询
- **那么** Controller 不直接操作数据库连接

#### 场景：课程内容上传
- **当** 用户上传课程内容时
- **那么** 通过 `ICourseService.UploadContentAsync(dto)` 处理
- **那么** 输入通过 DTO 验证（DataAnnotations）
- **那么** 事务在 Service 层管理

### 需求：单词学习服务
系统必须通过 `IWordService` 提供单词学习相关功能，包括单词查询、发音校准、学习进度跟踪，从 HomeController 中拆分为独立的 `WordController`。

#### 场景：单词发音校准
- **当** 用户提交发音校准请求时
- **那么** 通过 `IWordService.CalibrateAsync(dto)` 处理
- **那么** 发音比对逻辑封装在 Service 层
- **那么** Controller 仅负责接收请求和返回结果

#### 场景：获取单词详情
- **当** 用户查询单词详情时
- **那么** 优先从 MongoDB 查询富文本详情
- **那么** 如 MongoDB 无数据则从 MySQL 查询基础信息
- **那么** 查询结果可缓存至 Redis

### 需求：学习统计服务
系统必须通过 `IStatisticsService` 提供学习统计功能，包括每日学习量、正确率、学习趋势，从 HomeController 中拆分为独立的 `StatisticsController`。

#### 场景：获取用户学习统计
- **当** 用户请求学习统计数据时
- **那么** 通过 `IStatisticsService.GetStatisticsAsync(userId)` 获取
- **那么** 统计数据通过聚合查询生成
- **那么** 返回 `StatisticsDto` 包含学习量、正确率等指标

### 需求：收藏功能服务
系统必须通过 `IFavoriteService` 提供单词收藏功能，从 HomeController 中拆分为独立的 `FavoriteController`。

#### 场景：收藏和取消收藏单词
- **当** 用户收藏或取消收藏一个单词时
- **那么** 通过 `IFavoriteService.ToggleFavoriteAsync(userId, wordId)` 处理
- **那么** 操作为幂等的

### 需求：翻译服务封装
系统必须将有道翻译 API 调用封装为 `ITranslateService`，使用 `IHttpClientFactory` 管理 HTTP 客户端，从 HomeController 中拆分为独立的 `TranslateController`。

#### 场景：调用翻译 API
- **当** 用户请求翻译时
- **那么** 通过 `ITranslateService.TranslateAsync(word)` 调用
- **那么** HTTP 客户端通过 `IHttpClientFactory` 创建（非手动 `new HttpClient()`）
- **那么** API 密钥从配置读取，不硬编码

### 需求：数据导入服务
系统必须将 Excel 数据导入功能封装为 `IImportService`，支持事务完整性和错误报告，从 HomeController 中拆分为独立的 `ImportController`。

#### 场景：批量导入课程数据
- **当** 用户上传 Excel 文件导入数据时
- **那么** 通过 `IImportService.ImportAsync(file)` 处理
- **那么** 导入在单个事务中完成（全部成功或全部回滚）
- **那么** 返回导入结果（成功数、失败数、错误详情）
