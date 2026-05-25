## 新增需求

### 需求：React SPA 项目初始化
系统**必须**在 `LearnEnglish/ClientApp/` 下建立 Vite + React 18 + TypeScript 项目，包含 Tailwind CSS v3 配置，构建产物输出到 `wwwroot/`。

#### 场景：开发环境启动
- **当** 开发者执行 `cd ClientApp && npm run dev`
- **那么** Vite 开发服务器在 5173 端口启动，API 请求代理到 ASP.NET 后端

#### 场景：生产构建
- **当** 执行 `npm run build`
- **那么** 构建产物输出到 `wwwroot/` 目录，包含 `index.html` 和 `assets/` 打包文件

### 需求：SPA Fallback 路由
ASP.NET 后端**必须**配置 SPA fallback，将所有未匹配的路由指向 `wwwroot/index.html`，使 React Router 接管客户端路由。

#### 场景：直接访问 SPA 路由
- **当** 用户在浏览器中直接输入 `/words` 或 `/exams`
- **那么** 后端返回 `index.html`，React Router 渲染对应页面

#### 场景：API 路由不受影响
- **当** 前端请求 `/api/auth/login` 或 `/course/mycategorys`
- **那么** 后端正常处理 API 请求，不触发 SPA fallback
