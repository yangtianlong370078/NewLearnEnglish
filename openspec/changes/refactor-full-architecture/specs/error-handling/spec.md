## 新增需求

### 需求：全局异常处理中间件
系统必须使用全局异常处理中间件（`ExceptionHandlingMiddleware`）替代当前不完整的 Filter 方案，统一捕获未处理异常并返回标准化错误响应。

#### 场景：未处理异常返回统一格式
- **当** 系统发生未捕获异常时
- **那么** 中间件捕获异常并记录结构化日志
- **那么** 对 API 请求返回 JSON 格式的 `ApiResponse<T>`（包含 Success=false、ErrorCode、Message）
- **那么** 对页面请求重定向到错误页面
- **那么** 生产环境不暴露异常堆栈信息

#### 场景：领域异常映射
- **当** Service 层抛出自定义领域异常时
- **那么** `NotFoundException` 映射为 HTTP 404
- **那么** `ValidationException` 映射为 HTTP 400
- **那么** `UnauthorizedException` 映射为 HTTP 401
- **那么** 其他未知异常映射为 HTTP 500

### 需求：统一 API 响应格式
系统中所有 API 端点（返回 JSON 的 Action）必须使用统一的 `ApiResponse<T>` 包装响应数据。

#### 场景：成功响应
- **当** API 请求处理成功时
- **那么** 返回 `{ "success": true, "data": {...}, "message": null }`

#### 场景：失败响应
- **当** API 请求处理失败时
- **那么** 返回 `{ "success": false, "data": null, "message": "错误描述", "errorCode": "ERROR_CODE" }`

### 需求：结构化日志
系统必须使用 Serilog 进行结构化日志记录，输出到控制台和文件（按日滚动），在关键操作点记录上下文信息。

#### 场景：日志记录要求
- **当** 系统运行时
- **那么** 所有未处理异常记录为 Error 级别
- **那么** 所有 HTTP 请求记录基本信息（路径、方法、耗时、状态码）
- **那么** 认证相关操作（登录、Token 刷新）记录为 Information 级别
- **那么** 日志包含结构化属性（UserId、TraceId 等）

### 需求：健康检查端点
系统必须提供 `/health` 端点，检查 MySQL、MongoDB、Redis 三个数据存储的连通性。

#### 场景：所有服务正常
- **当** 所有依赖服务可用时
- **那么** `/health` 返回 HTTP 200 和各服务状态 `Healthy`

#### 场景：部分服务异常
- **当** 任一依赖服务不可用时
- **那么** `/health` 返回 HTTP 503 和具体不可用服务的 `Unhealthy` 状态
