# 变更：全面重构前端页面——从 Bootstrap/jQuery 迁移到 React + Tailwind CSS

## 为什么

当前前端基于 Bootstrap 4 + jQuery 3.3 + 45+ 个第三方 jQuery 插件构建，技术栈陈旧：
1. **维护性差**：大量 jQuery DOM 操作、内联脚本、全局变量污染、事件管理混乱
2. **体积臃肿**：vendor_components 包含大量未使用的库（天气图标、地图、富文本编辑器等），首屏加载慢
3. **交互体验落后**：全页刷新式 MVC 路由、手动 AJAX 管理、无统一状态管理
4. **设计陈旧**：Bootstrap 默认样式缺乏品牌辨识度、无暗色主题、动画薄弱
5. **已有设计规范**：DESIGN_SYSTEM.md 已详细定义了现代化视觉体系（毛玻璃、渐变、微动画）和 React + Tailwind 技术栈

重构不改变任何后端接口和业务逻辑，仅替换前端 UI 层。

## 变更内容

### 核心迁移
- 摒弃 Bootstrap、jQuery 及所有关联 jQuery 插件
- 引入 **React 18 + TypeScript + Vite** 构建体系
- 引入 **Tailwind CSS v3** 原子化样式框架
- 引入 **React Router v6** 实现 SPA 路由
- 严格实现 DESIGN_SYSTEM.md 中定义的所有视觉规范

### 页面重写清单

| 原始 View | React 页面 | 所属模块 |
|-----------|-----------|---------|
| Login/Login.cshtml | `LoginPage` | 认证 |
| Login/Register.cshtml | `RegisterPage` | 认证 |
| Login/Index.cshtml | `WxLoginPage` | 认证 |
| Home/Index.cshtml | `AppShell`（主路由容器） | 布局 |
| Home/Home.cshtml | `DashboardPage` | 仪表板 |
| Home/My.cshtml | `ProfilePage` | 用户中心 |
| Home/CategoryList.cshtml | `CourseCatalogPage` | 课程 |
| Home/MyCategoryList.cshtml | `MyCoursesPage` | 课程 |
| Home/MyCategoryContent.cshtml | `CourseDetailPage` | 课程 |
| Home/MyCategoryContentTL.cshtml | `HearingCoursePage` | 听力 |
| Home/learnEnglishList.cshtml | `WordListPage` | 单词 |
| Home/learnEnglishTable.cshtml | `WordTablePage` | 单词 |
| Home/lexiconDeatil.cshtml | `WordDetailPage` | 单词 |
| Exam/ExamList.cshtml | `ExamListPage` | 考试 |
| Exam/ExamContentList.cshtml | `ExamTakePage` | 考试 |
| Exam/ExamContentTable.cshtml | `ExamResultPage` | 考试 |
| Hearing/HearingLX.cshtml | `HearingPracticePage` | 听力 |
| Whisper/Index.cshtml | `SpeechRecognitionPage` | 口语 |
| Vosk/Index.cshtml | `VoskRecognitionPage` | 口语 |
| Shared/_Layout.cshtml | `AppLayout` + `Navbar` + `Sidebar` | 布局 |
| Shared/Header.cshtml | `Navbar` 组件 | 布局 |
| Shared/Search.cshtml | `SearchInput` 组件 | 布局 |

### 通用组件库（依据 DESIGN_SYSTEM.md）
- Button（Primary/Secondary/Ghost/Danger/Gradient/Icon）
- Card（基础/指标/单词卡片）
- Input（搜索框/表单输入）
- Modal（模态框）
- Badge/Tag（状态标签）
- Progress（线性/环形进度条）
- Table（数据表格）
- Toast（消息提示）
- Skeleton（骨架屏）
- EmptyState（空状态）

### 布局系统
- 毛玻璃导航栏（固定顶部）
- 桌面端固定侧边栏（260px）+ 移动端抽屉式侧边栏
- 移动端底部 Tab 栏
- 响应式自适应（mobile-first）

### 主题与动画
- 三种主题模式（日间/夜间/跟随系统）
- CSS 变量驱动的主题切换
- 完整的微交互动画体系（按钮、卡片、翻牌、波形等）
- 滚动入场动画
- 骨架屏加载态

### 技术集成
- **Axios + TanStack Query**：API 调用 + 缓存 + 状态管理
- **Zustand**：全局状态管理（认证、主题、学习进度）
- **Recharts**：学习统计图表
- **Lucide React**：统一图标库
- **Framer Motion**：高级动画效果
- **React Hook Form + Zod**：表单管理与校验

### 后端集成策略
- 保持所有现有 API 端点不变
- 前端通过 Axios 直接调用现有 `/api/auth/`、`/course/`、`/word/`、`/exam/`、`/hearing/` 等接口
- JWT token 管理由前端负责（localStorage + 自动刷新）
- 对于返回 PartialView（HTML）的端点，需重构为返回 JSON
  - `/word/learnenglishlist` → 前端直接调用 `GET /word/list` JSON 接口
  - `/exam/examlist` → 前端直接调用 `GET /exam/list` JSON 接口
  - 其他 PartialView 类似处理

### 不变的部分
- 所有后端 Controller 业务逻辑不变
- 数据库结构不变
- API 请求参数格式不变
- API 返回数据结构不变
- 认证流程不变（JWT + Refresh Token）

## 影响

- **受影响规范**：`frontend-ui`、`frontend-routing`、`frontend-theme`、`frontend-components`、`frontend-data-layer`、`frontend-authentication`、`frontend-dashboard`、`frontend-words`、`frontend-exam`、`frontend-hearing`、`frontend-speech`、`frontend-statistics`
- **受影响代码**：
  - 移除：`LearnEnglish/Views/` 下所有 .cshtml 文件（保留为 SPA fallback 入口）
  - 移除：`wwwroot/assets/vendor_components/`、`wwwroot/lib/`、`wwwroot/Scripts/`、`wwwroot/Content/`
  - 新增：`LearnEnglish/ClientApp/` React SPA 项目目录
  - 修改：后端需新增几个 JSON API 端点（替代 PartialView 端点）
  - 修改：`Program.cs` 添加 SPA fallback 中间件配置
- **风险**：
  - PartialView 端点需后端配合提供 JSON 版本
  - 微信登录流程中的重定向需 SPA 适配
