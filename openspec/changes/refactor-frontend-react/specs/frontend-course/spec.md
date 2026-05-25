## 新增需求

### 需求：课程目录页
系统**必须**提供课程分类浏览页面（`/courses`），展示所有可用课程分类列表，支持按类型筛选（学单词/练听力）。

#### 场景：课程列表加载
- **当** 用户进入课程目录页
- **那么** 调用 `GET /course/categorylist?type=1` 获取课程分类列表
- **并且** 以卡片网格形式展示（md 2列，lg 3列）

#### 场景：添加课程到我的学习
- **当** 用户点击课程卡片上"加入学习"按钮
- **那么** POST 请求 `/course/insertmycourse?setcourseId=xxx`
- **并且** 成功后显示 Toast 提示

### 需求：我的课程页
系统**必须**提供用户已加入的课程列表页面（`/courses/mine`），展示每个课程的学习进度。

#### 场景：我的课程列表
- **当** 用户进入我的课程页
- **那么** 调用 `GET /course/mycategorys?id=xxx` 获取课程进度数据
- **并且** 每个课程卡片显示：课程名、总词数、已完成数、进度百分比条

#### 场景：课程模式切换
- **当** 用户切换"学单词"/"练听力"Tab
- **那么** 对应调用 `type=1`（单词）或 `type=2`（听力）请求数据

### 需求：课程详情页
系统**必须**提供单个课程的学习内容页面（`/courses/:id`），展示该课程的单词列表和操作。

#### 场景：课程内容加载
- **当** 用户进入课程详情
- **那么** 显示课程名、进度、课程内单词网格/表格列表

### 需求：课程管理
系统**必须**支持课程的创建、编辑、删除操作。

#### 场景：创建课程
- **当** 用户点击"创建课程"并填写名称
- **那么** POST 请求 `/course/savecourse?setcourseId=0&insercoursename=xxx&type=1`

#### 场景：删除课程
- **当** 用户确认删除课程
- **那么** POST 请求 `/course/deletecourse?setcourseId=xxx`
- **并且** 刷新课程列表

### 需求：向课程添加单词
系统**必须**支持在课程详情页内添加单词。

#### 场景：手动添加单词
- **当** 用户输入英文和中文释义
- **那么** POST 请求 `/course/savecoursecontent?courseId=xxx&en=xxx&cn=xxx`

#### 场景：Excel 导入单词
- **当** 用户上传 Excel 文件
- **那么** POST 请求 `/import/daoroucoursecontent`（multipart/form-data）
- **并且** 显示导入结果提示

**交叉引用**：依赖 `frontend-data-layer`、`frontend-ui-components`、`frontend-layout`
