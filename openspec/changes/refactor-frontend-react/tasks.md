# 任务清单：前端 React + Tailwind CSS 重构

> 总计 12 个阶段，约 45+ 个任务。任务按依赖关系排序，标注可并行项。

---

## 阶段 1：项目脚手架与基础设施（前置，无依赖）

- [x] **1.1** 在 `LearnEnglish/ClientApp/` 初始化 Vite + React 18 + TypeScript 项目
  - 验证：`npm run dev` 成功启动，浏览器显示默认 React 页面
- [x] **1.2** 配置 Tailwind CSS v3（含 DESIGN_SYSTEM.md 中的全部自定义配置：颜色、阴影、动画、缓动）
  - 验证：创建测试页面，确认 `bg-surface`、`shadow-card`、`animate-fade-in-up` 等自定义 token 生效
- [x] **1.3** 配置 Vite 代理（所有后端路由：`/api/`、`/course/`、`/word/`、`/exam/`、`/hearing/`、`/whisper/`、`/login/`、`/statistics/`、`/translate/`、`/userinfo/`、`/import/`）
  - 验证：前端 `fetch('/login/loginresult')` 能正确代理到后端
- [x] **1.4** 配置 Vite 构建输出到 `wwwroot/`，修改 `Program.cs` 添加 SPA fallback
  - 验证：`npm run build` 后，直接启动 ASP.NET 后端能访问 React 页面
- [x] **1.5** 安装核心依赖：react-router-dom、axios、@tanstack/react-query、zustand、lucide-react、recharts、framer-motion、react-hook-form、zod、@headlessui/react
  - 验证：`npm ls --depth=0` 确认所有包安装成功

## 阶段 2：全局样式与主题系统（依赖 1.2）

- [x] **2.1** 创建 `globals.css`：定义全部 CSS 变量（日间/夜间两套）、基础重置样式、毛玻璃工具类、字体加载
  - 验证：切换 `data-theme="dark"` 时变量正确切换
- [x] **2.2** 创建 `animations.css`（合并到 globals.css 和 tailwind.config.js）：定义所有关键帧动画（20+ 种）、`prefers-reduced-motion` 降级
  - 验证：`animate-fade-in-up`、`animate-flip`、`animate-confetti` 等动画类名可用
- [x] **2.3** 实现 `useTheme` hook + `themeStore`（Zustand）：三种主题模式切换、localStorage 持久化、系统暗色模式监听
  - 验证：主题切换功能端到端工作

## 阶段 3：数据层与认证（依赖 1.5）

- [x] **3.1** 实现 `lib/api.ts`：Axios 实例 + 请求拦截器（JWT 注入）+ 响应拦截器（401 刷新、错误统一处理、`succss` 兼容）
  - 验证：模拟 401 场景，确认 token 自动刷新并重试
- [x] **3.2** 实现 `authStore`（Zustand）：token/refreshToken/用户信息管理，localStorage 持久化
  - 验证：登录后刷新页面，认证状态保持
- [x] **3.3** 配置 TanStack Query `QueryClient`：默认 stale time、retry 策略
  - 验证：数据请求自带 loading/error/success 状态管理

## 阶段 4：基础 UI 组件库（依赖 2.1, 2.2）⚡ 可与阶段 3 并行

- [x] **4.1** 实现 `Button` 组件（6 类型 × 5 尺寸 + 加载态 + 禁用态 + 交互动画）
  - 验证：创建组件展示页，所有变体视觉正确
- [x] **4.2** 实现 `Card` 组件（基础卡片 + StatCard + WordCard）
  - 验证：hover 效果、渐变装饰条、响应式布局
- [x] **4.3** 实现 `Input` 组件（带图标、focus ring、搜索变体）
  - 验证：聚焦时蓝色 ring、占位文字样式
- [x] **4.4** 实现 `Modal` 组件（遮罩 + scale-in 动画 + 粘性头尾）
  - 验证：打开/关闭动画流畅，ESC/点击遮罩关闭
- [x] **4.5** 实现 `Badge` / `Tag` 组件（6 色状态标签 + 模块标签 + 计数徽标）
  - 验证：所有颜色主题正确
- [x] **4.6** 实现 `Progress` 组件（线性渐变进度条 + SVG 环形进度条）
  - 验证：数值变化有过渡动画
- [x] **4.7** 实现 `Table` 组件（行 hover、分页器、响应式）
  - 验证：hover 高亮、分页切换
- [x] **4.8** 实现 `Toast` 系统（4 种类型、自动消失、右上角固定、slide-in-right 动画）
  - 验证：toast.success() / toast.error() 全局调用
- [x] **4.9** 实现 `Skeleton` 组件（shimmer 动画、WordCard 骨架屏变体）
  - 验证：闪烁动画效果
- [x] **4.10** 实现 `EmptyState` 组件（图标 + 描述 + CTA 按钮）
  - 验证：空列表页面显示

## 阶段 5：布局组件（依赖 4.x）

- [x] **5.1** 实现 `Navbar` 毛玻璃导航栏（Logo、导航链接、主题切换、用户头像、响应式折叠）
  - 验证：毛玻璃效果、滚动时固定、移动端精简
- [x] **5.2** 实现 `Sidebar` 固定侧边栏（菜单分组、活动态高亮、今日目标进度卡）
  - 验证：菜单项点击路由跳转、活动态正确
- [x] **5.3** 实现 `MobileTabBar` 底部导航栏（5 Tab + 中间凸起渐变按钮 + 安全区适配）
  - 验证：仅 < 1024px 显示、安全区 padding
- [x] **5.4** 实现 `AppLayout` 组合布局（Navbar + Sidebar + 主内容区 + MobileTabBar + 认证守卫）
  - 验证：桌面端和移动端布局切换正确

## 阶段 6：路由与认证页面（依赖 3.x, 5.4）

- [x] **6.1** 配置 React Router v6 路由表（所有页面路由 + 认证守卫 + 404 页面）
  - 验证：所有 URL 路径正确映射到对应页面组件
- [x] **6.2** 实现 `LoginPage`（品牌背景 + 表单 + 调用 `/login/loginresult`）
  - 验证：登录成功跳转首页，失败显示 Toast
- [x] **6.3** 实现 `RegisterPage`（注册表单 + 调用 `/login/setregister`）
  - 验证：注册成功跳转登录页

## 阶段 7：仪表板（依赖 6.1）

- [x] **7.1** 实现 `DashboardPage`（指标卡片 + 趋势图表 + 今日任务 + 最近单词）
  - 验证：所有数据接口正确调用，图表渲染正确

## 阶段 8：单词模块（依赖 6.1）⚡ 可与阶段 7 并行

- [x] **8.1** 实现 `WordListPage`（卡片网格 + 筛选 Tab + 搜索 + 分页 + 骨架屏加载态）
  - 验证：分页切换、筛选筛选响应正确
  - **依赖后端**：需要新增返回 JSON 的单词列表 API
- [x] **8.2** 实现 `WordTablePage`（表格视图 + 操作列）
  - 验证：行 hover、操作按钮功能
- [x] **8.3** 实现 `WordDetailPage`（单词详情 + 有道词典数据 + 编辑/删除功能）
  - 验证：翻译数据正确展示、编辑保存、删除确认

## 阶段 9：课程模块（依赖 6.1）⚡ 可与阶段 7、8 并行

- [x] **9.1** 实现 `CourseCatalogPage`（课程分类卡片网格 + 加入学习操作）
  - 验证：加入课程成功后 Toast 提示
- [x] **9.2** 实现 `MyCoursesPage`（我的课程列表 + 进度 + 模式切换 Tab）
  - 验证：学单词/练听力切换
- [x] **9.3** 实现 `CourseDetailPage`（课程内容 + 添加单词 + Excel 导入）
  - 验证：手动添加、Excel 导入功能正常

## 阶段 10：考试模块（依赖 6.1）⚡ 可与阶段 7~9 并行

- [x] **10.1** 实现 `ExamListPage`（考试记录列表 + 创建考试 + 删除）
  - 验证：创建考试跳转答题页
  - **依赖后端**：需要新增返回 JSON 的考试列表 API
- [x] **10.2** 实现 `ExamTakePage`（答题界面 + 选项交互 + 倒计时 + 提交）
  - 验证：答题流程端到端、倒计时正确
- [x] **10.3** 实现 `ExamResultPage`（成绩展示 + 错题回顾 + 庆祝动画 + 重考）
  - 验证：高分触发 confetti 动画

## 阶段 11：听力/口语/统计模块（依赖 6.1）⚡ 可与阶段 7~10 并行

- [x] **11.1** 实现 `HearingPracticePage`（音频播放 + 字幕同步 + 波形动画 + 中英对照模式）
  - 验证：音频播放时字幕自动跟随高亮
- [x] **11.2** 实现 `SpeechRecognitionPage`（录音 + 提交识别 + 环形评分展示）
  - 验证：录音上传到 `/whisper/recognize`，评分正确显示
- [ ] **11.3** 实现 `VoskRecognitionPage`（待实现）（Vosk 离线识别页面）
  - 验证：录音并识别
- [x] **11.4** 实现 `StatisticsPage`（指标卡片 + 趋势图 + 分布饼图 + 正确率折线 + 考试记录表 + 任务管理）
  - 验证：所有图表渲染正确
- [x] **11.5** 实现 `ProfilePage`（用户信息 + 修改密码功能）
  - 验证：修改密码调用 `/login/modifypassword`

## 阶段 12：清理与优化（依赖全部）

- [ ] **12.1** 移除旧前端资源：`wwwroot/assets/vendor_components/`、`wwwroot/lib/`、`wwwroot/Scripts/`、`wwwroot/Content/`、`wwwroot/css/`、`wwwroot/js/`
  - 验证：旧资源不再被引用
- [ ] **12.2** 全页面响应式测试（手机/平板/桌面三个断点）
  - 验证：所有页面在 375px、768px、1440px 宽度下布局正确
- [ ] **12.3** 主题切换全页面测试（日间/夜间模式下所有页面颜色正确）
  - 验证：无色彩异常、对比度达标
- [x] **12.4** 性能优化：路由级代码分割（React.lazy）、图片懒加载、Vite 分包策略
  - 验证：首屏 JS 体积 < 200KB（gzip 后）
- [ ] **12.5** 无障碍审查：键盘导航、焦点指示、ARIA 标签、语义化 HTML
  - 验证：Tab 键可遍历所有交互元素

---

## 依赖关系图

```
阶段1 (脚手架)
  ├─→ 阶段2 (样式/主题) ─→ 阶段4 (UI组件) ─→ 阶段5 (布局) ─┐
  └─→ 阶段3 (数据/认证) ─────────────────────────────────────┤
                                                              ↓
                                                        阶段6 (路由/登录)
                                                              │
                              ┌────────┬────────┬──────────┬──┤
                              ↓        ↓        ↓          ↓  ↓
                          阶段7    阶段8    阶段9    阶段10  阶段11
                          (仪表板)  (单词)   (课程)   (考试)  (听力/口语/统计)
                              └────────┴────────┴──────────┴──┘
                                                              ↓
                                                        阶段12 (清理/优化)
```

## 后端配合项（阻塞依赖）

以下任务需要后端配合新增 JSON API（不修改业务逻辑，仅新增返回格式）：

| 任务 | 需要后端提供 | 阻塞阶段 |
|------|------------|---------|
| 8.1 | `GET /api/word/list`（JSON 分页列表） | 阶段 8 |
| 8.1 | `GET /api/word/collect-list`（JSON 收藏列表） | 阶段 8 |
| 10.1 | `GET /api/exam/list`（JSON 考试列表） | 阶段 10 |
| 10.2 | `GET /api/exam/content`（JSON 题目列表） | 阶段 10 |

> 如后端无法及时提供，前端可先使用 Mock 数据开发，后续对接。
