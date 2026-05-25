## 新增需求

### 需求：仪表板页面
系统**必须**提供仪表板首页（`/`），展示用户学习概况，包含：顶部欢迎语、指标卡片行（已学词数/正确率/连续天数/总课程）、学习趋势图表、今日任务列表、最近学习单词网格。

#### 场景：仪表板数据加载
- **当** 用户进入仪表板
- **那么** 并行调用以下接口：
  - `GET /home/getuiserinfo` → 获取用户信息和连续学习天数
  - `GET /statistics/querylearncount` → 获取本周/总学习单词数量
  - `GET /statistics/statisticslearncounttwo` → 获取月度统计数据

#### 场景：指标卡片展示
- **当** 数据加载完成
- **那么** 显示 4 个指标卡片（每个包含图标、数值、标签、变化趋势百分比）
- **并且** 卡片 hover 时图标放大 110%

#### 场景：学习趋势图表
- **当** 统计数据加载完成
- **那么** 使用 Recharts 渲染面积图（Area Chart），展示近期学习趋势
- **并且** 图表区域使用蓝色渐变填充

**交叉引用**：依赖 `frontend-data-layer`、`frontend-ui-components`（StatCard/ChartContainer）、`frontend-layout`
