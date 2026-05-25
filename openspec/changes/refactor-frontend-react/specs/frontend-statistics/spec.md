## 新增需求

### 需求：学习统计页面
系统**必须**提供学习统计页面（`/statistics`），按照 DESIGN_SYSTEM.md 定义的统计页面布局，展示多维度学习数据。

#### 场景：统计数据加载
- **当** 用户进入统计页面
- **那么** 并行调用：
  - `GET /statistics/querylearncount` → 本周/总学习数量和用户名
  - `GET /statistics/statisticslearncounttwo` → 月度统计和任务数据

#### 场景：顶部指标卡片
- **当** 数据加载完成
- **那么** 显示 4 个指标卡片：本周学词数、本月学词数、总学习时长、完成率
- **并且** 数字使用 count-up 滚动动画（1200ms）

#### 场景：学习趋势图表
- **当** 统计数据就绪
- **那么** 使用 Recharts 渲染面积图（周/月切换）
- **并且** 图表区域使用蓝色渐变填充

#### 场景：模块学习分布
- **当** 有各模块学习数据
- **那么** 使用 Recharts 渲染环形图（PieChart）显示各模块占比

#### 场景：正确率趋势
- **当** 有历史正确率数据
- **那么** 使用 Recharts 渲染折线图展示正确率变化

#### 场景：学习任务管理
- **当** 用户设置月度学习任务
- **那么** POST 请求 `/statistics/savelearntask?id=xxx&count=xxx&date=xxx&type=xxx&weekend=xxx`

#### 场景：最近考试记录
- **当** 有考试历史
- **那么** 底部表格显示最近考试记录（考试名称、日期、分数、状态标签）

**交叉引用**：依赖 `frontend-data-layer`、`frontend-ui-components`（StatCard/ChartContainer/Table/Badge）、`frontend-layout`
