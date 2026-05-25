## 新增需求

### 需求：Axios 统一 API 层
系统**必须**提供全局 Axios 实例，配置请求拦截器（自动注入 JWT token）和响应拦截器（token 刷新、统一错误处理）。

#### 场景：自动注入认证头
- **当** 前端发起 API 请求
- **并且** authStore 中存在有效 token
- **那么** 请求头自动附带 `Authorization: Bearer <token>`

#### 场景：token 过期自动刷新
- **当** API 返回 401 状态码
- **那么** 自动调用 `/api/auth/refresh` 获取新 token
- **并且** 使用新 token 重试原请求
- **当** 刷新也失败
- **那么** 清除认证状态，跳转到登录页

#### 场景：兼容后端拼写不一致
- **当** 解析后端返回数据
- **那么** 同时检查 `success` 和 `succss` 字段（兼容后端拼写错误）

### 需求：TanStack Query 数据缓存
系统**必须**使用 TanStack Query 管理所有服务端状态（列表数据、详情数据），配置合理的缓存策略。

#### 场景：单词列表请求
- **当** 进入单词列表页
- **那么** TanStack Query 自动管理请求状态（loading/error/success）
- **并且** 页面切换回该列表时，先显示缓存数据再背景刷新（stale-while-revalidate）

### 需求：Zustand 状态管理
系统**必须**使用 Zustand 管理客户端全局状态：authStore（认证）、themeStore（主题）、uiStore（UI 状态）。

#### 场景：认证状态持久化
- **当** 用户登录成功
- **那么** token、refreshToken、用户信息存入 authStore 并持久化到 localStorage
- **当** 页面刷新
- **那么** 从 localStorage 恢复认证状态

**交叉引用**：依赖 `frontend-authentication`（认证流程）、`frontend-theme`（主题状态）
