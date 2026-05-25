# 设计文档：前端 React + Tailwind CSS 重构

## 1. 架构决策

### 1.1 SPA 集成方案

**决策**：在 ASP.NET Core 项目中内嵌 React SPA（ClientApp 模式）

**理由**：
- 无需独立部署前端服务器，复用现有 ASP.NET 部署流水线
- ASP.NET Core 原生支持 `UseSpa()` 中间件
- 开发时可利用 Vite dev server 热更新，生产时编译为静态文件输出到 `wwwroot`

**替代方案（已排除）**：
- 独立前端项目 + CORS：增加运维复杂度，存在跨域问题
- Next.js SSR：过度工程化，本项目不需要 SEO

**集成方式**：
```
LearnEnglish/
├── ClientApp/                   ← React SPA 源码
│   ├── src/
│   ├── public/
│   ├── package.json
│   ├── vite.config.ts
│   ├── tailwind.config.ts
│   └── tsconfig.json
├── wwwroot/                     ← Vite 构建输出目录
│   ├── assets/                  ← 打包后的 JS/CSS/图片
│   └── index.html               ← SPA 入口
├── Program.cs                   ← 配置 SPA fallback
└── ...
```

### 1.2 路由策略

**决策**：React Router v6 客户端路由 + ASP.NET SPA Fallback

**路由映射**：

| URL 路径 | React 页面 | 原始 MVC 路由 |
|----------|-----------|-------------|
| `/login` | `LoginPage` | `/login/login` |
| `/register` | `RegisterPage` | `/login/register` |
| `/wx-login` | `WxLoginPage` | `/login/index` |
| `/` | `DashboardPage` | `/home/home` |
| `/profile` | `ProfilePage` | `/home/my` |
| `/courses` | `CourseCatalogPage` | `/course/categorylist` |
| `/courses/mine` | `MyCoursesPage` | `/course/mycategorylist` |
| `/courses/:id` | `CourseDetailPage` | `/course/mycategorycontent` |
| `/courses/:id/hearing` | `HearingCoursePage` | `/course/mycategorycontenttl` |
| `/words` | `WordListPage` | `/word/learnenglishlist` |
| `/words/table/:courseId` | `WordTablePage` | `/course/learnenglishTable` |
| `/words/:word` | `WordDetailPage` | `/word/lexicondeatil` |
| `/exams` | `ExamListPage` | `/exam/examlist` |
| `/exams/:id` | `ExamTakePage` | `/exam/examcontentlist` |
| `/exams/:id/result` | `ExamResultPage` | `/exam/examcontenttable` |
| `/hearing/:courseId` | `HearingPracticePage` | `/hearing/hearinglx` |
| `/speech` | `SpeechRecognitionPage` | `/whisper/index` |
| `/speech/vosk` | `VoskRecognitionPage` | `/vosk/index` |
| `/statistics` | `StatisticsPage` | (新页面，整合统计功能) |

**后端配置**：
```csharp
// Program.cs 新增
app.MapFallbackToFile("index.html"); // SPA fallback
```

### 1.3 状态管理方案

**决策**：Zustand 轻量级状态管理 + TanStack Query 服务端状态

**职责划分**：

| 状态类型 | 管理方案 | 说明 |
|---------|---------|------|
| 服务端数据（单词、课程、考试...） | TanStack Query | 自动缓存、重试、过期刷新 |
| 认证状态（token、用户信息） | Zustand `authStore` | 持久化到 localStorage |
| 主题状态 | Zustand `themeStore` | 持久化到 localStorage |
| UI 状态（sidebar 开关、modal...） | Zustand `uiStore` | 仅内存 |

### 1.4 API 层设计

**决策**：统一 Axios 实例 + 请求/响应拦截器

```typescript
// lib/api.ts
const api = axios.create({
  baseURL: '/',
  timeout: 15000,
});

// 请求拦截器：自动注入 JWT token
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 响应拦截器：自动刷新 token、统一错误处理
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // 尝试刷新 token
      await refreshToken();
      return api(error.config);
    }
    return Promise.reject(error);
  }
);
```

### 1.5 接口调用注意事项

**关键发现**：当前后端有部分端点使用 Query String 传参（而非 Request Body），前端必须保持一致。

| 接口 | 参数方式 | 注意 |
|------|---------|------|
| `/course/insertmycourse` | `?setcourseId=xxx` | query string |
| `/word/szzt` | `?zt=&dqzt=&lexiconId=` | query string |
| `/exam/insertexamnswer` | `?data=&examid=&type=&score=` | query string |
| `/login/loginresult` | `?loginID=&password=&sb=` | query string |
| `/login/setregister` | `?loginID=&password=&phone=&name=` | query string |

**后端返回不一致**：部分端点的 success 字段拼写为 `succss`（少了 e），前端需兼容处理：
```typescript
// 兼容后端拼写错误
const isSuccess = (res: any) => res.success === true || res.succss === true;
```

### 1.6 PartialView 端点迁移策略

现有 PartialView 端点（返回 HTML 片段而非 JSON）：
- `GET /word/learnenglishlist` → 返回 HTML
- `GET /word/learnenglishcollectlist` → 返回 HTML
- `GET /exam/examlist` → 返回 HTML
- `GET /exam/examcontentlist` → 返回 HTML

**策略**：需要在后端新增对应的 JSON API 端点，或修改现有端点支持 `Accept: application/json` 内容协商。

建议新增 API：
| 新端点 | HTTP | 返回 | 对应原端点 |
|--------|------|------|---------|
| `GET /api/word/list` | GET | JSON（分页列表） | `/word/learnenglishlist` |
| `GET /api/word/collect-list` | GET | JSON（分页列表） | `/word/learnenglishcollectlist` |
| `GET /api/exam/list` | GET | JSON（分页列表） | `/exam/examlist` |
| `GET /api/exam/content` | GET | JSON（题目列表） | `/exam/examcontentlist` |

## 2. 目录结构

```
LearnEnglish/ClientApp/
├── public/
│   └── favicon.svg
├── src/
│   ├── assets/                  # 静态资源
│   │   ├── images/
│   │   └── fonts/
│   ├── components/              # 通用组件
│   │   ├── ui/                  # 基础 UI 组件
│   │   │   ├── Button.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── Badge.tsx
│   │   │   ├── Progress.tsx
│   │   │   ├── Skeleton.tsx
│   │   │   ├── Table.tsx
│   │   │   ├── Toast.tsx
│   │   │   ├── EmptyState.tsx
│   │   │   └── index.ts
│   │   ├── layout/              # 布局组件
│   │   │   ├── Navbar.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   ├── MobileTabBar.tsx
│   │   │   ├── PageContainer.tsx
│   │   │   └── AppLayout.tsx
│   │   └── shared/              # 业务通用组件
│   │       ├── WordCard.tsx
│   │       ├── AudioPlayer.tsx
│   │       ├── StatCard.tsx
│   │       ├── ChartContainer.tsx
│   │       └── EmptyState.tsx
│   ├── features/                # 功能模块（按业务划分）
│   │   ├── auth/
│   │   │   ├── pages/
│   │   │   │   ├── LoginPage.tsx
│   │   │   │   ├── RegisterPage.tsx
│   │   │   │   └── WxLoginPage.tsx
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   ├── dashboard/
│   │   │   ├── pages/
│   │   │   │   └── DashboardPage.tsx
│   │   │   └── components/
│   │   ├── course/
│   │   │   ├── pages/
│   │   │   │   ├── CourseCatalogPage.tsx
│   │   │   │   ├── MyCoursesPage.tsx
│   │   │   │   └── CourseDetailPage.tsx
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   ├── words/
│   │   │   ├── pages/
│   │   │   │   ├── WordListPage.tsx
│   │   │   │   ├── WordTablePage.tsx
│   │   │   │   └── WordDetailPage.tsx
│   │   │   ├── components/
│   │   │   │   └── WordCard.tsx
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   ├── exam/
│   │   │   ├── pages/
│   │   │   │   ├── ExamListPage.tsx
│   │   │   │   ├── ExamTakePage.tsx
│   │   │   │   └── ExamResultPage.tsx
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   ├── hearing/
│   │   │   ├── pages/
│   │   │   │   └── HearingPracticePage.tsx
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   ├── speech/
│   │   │   ├── pages/
│   │   │   │   ├── SpeechRecognitionPage.tsx
│   │   │   │   └── VoskRecognitionPage.tsx
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   ├── statistics/
│   │   │   ├── pages/
│   │   │   │   └── StatisticsPage.tsx
│   │   │   ├── components/
│   │   │   ├── api.ts
│   │   │   └── types.ts
│   │   └── profile/
│   │       ├── pages/
│   │       │   └── ProfilePage.tsx
│   │       └── api.ts
│   ├── hooks/                   # 自定义 Hooks
│   │   ├── useTheme.ts
│   │   ├── useScrollAnimation.ts
│   │   ├── useAudio.ts
│   │   └── useMediaQuery.ts
│   ├── lib/                     # 工具函数
│   │   ├── api.ts               # Axios 实例
│   │   ├── utils.ts
│   │   └── constants.ts
│   ├── stores/                  # Zustand 状态管理
│   │   ├── authStore.ts
│   │   ├── themeStore.ts
│   │   └── uiStore.ts
│   ├── styles/                  # 全局样式
│   │   ├── globals.css          # CSS 变量、基础重置
│   │   └── animations.css       # 关键帧动画
│   ├── types/                   # 全局 TypeScript 类型
│   │   └── index.ts
│   ├── App.tsx                  # 根组件（路由配置）
│   ├── main.tsx                 # 入口文件
│   └── vite-env.d.ts
├── index.html
├── package.json
├── tsconfig.json
├── tsconfig.node.json
├── tailwind.config.ts
├── postcss.config.js
└── vite.config.ts
```

## 3. 构建与开发配置

### 3.1 Vite 配置

```typescript
// vite.config.ts
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // 将 API 请求代理到 ASP.NET 后端
      '/api': 'https://localhost:7xxx',
      '/course': 'https://localhost:7xxx',
      '/word': 'https://localhost:7xxx',
      '/exam': 'https://localhost:7xxx',
      '/examv2': 'https://localhost:7xxx',
      '/hearing': 'https://localhost:7xxx',
      '/whisper': 'https://localhost:7xxx',
      '/login': 'https://localhost:7xxx',
      '/home': 'https://localhost:7xxx',
      '/statistics': 'https://localhost:7xxx',
      '/translate': 'https://localhost:7xxx',
      '/userinfo': 'https://localhost:7xxx',
      '/import': 'https://localhost:7xxx',
    },
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
  },
});
```

### 3.2 Tailwind 配置

严格遵循 DESIGN_SYSTEM.md 中定义的配置：
- CSS 变量驱动的语义化颜色
- 自定义阴影层次（card、card-hover、float、modal、btn 等）
- 自定义缓动函数（spring、smooth、decel、accel）
- 丰富关键帧动画（20+ 种动画）

### 3.3 开发流程

1. `cd ClientApp && npm run dev` — 启动 Vite 开发服务器（5173 端口）
2. `dotnet run` — 启动 ASP.NET 后端（代理模式）
3. Vite 将 API 请求代理到后端
4. 生产构建时 `npm run build` 输出到 `wwwroot/`

## 4. 权衡记录

| 决策项 | 选择 | 理由 |
|--------|------|------|
| SPA vs MPA | SPA | 更流畅的用户体验，适合学习类应用 |
| Vite vs Webpack | Vite | 开发体验更优，构建更快 |
| Zustand vs Redux | Zustand | 更轻量，够用，学习成本低 |
| Tailwind vs CSS Modules | Tailwind | 设计规范已基于 Tailwind 定义 |
| Recharts vs ECharts | Recharts | React 原生声明式 API，体积更小 |
| Headless UI vs Radix | Headless UI | Tailwind 官方推荐，无样式冲突 |
| Axios vs fetch | Axios | 拦截器、token 刷新更方便 |
| Framer Motion vs CSS | Framer Motion | 复杂动画（翻牌、庆祝）需要 JS 动画库 |
