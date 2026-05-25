## 新增需求

### 需求：考试服务层
系统必须将 ExamController 中的业务逻辑提取到 `IExamService`，Controller 仅负责 HTTP 关注点，数据访问通过 `IExamRepository` 完成。

#### 场景：创建考试
- **当** 用户创建新考试时
- **那么** 通过 `IExamService.CreateExamAsync(dto)` 处理
- **那么** 输入通过 DTO 验证
- **那么** 数据通过 `IExamRepository` 持久化
- **那么** 所有 SQL 使用参数化查询

#### 场景：获取考试列表
- **当** 用户请求考试列表时
- **那么** 通过 `IExamService.GetExamListAsync(userId, page)` 获取
- **那么** 支持分页查询，返回 `PagedList<ExamDto>`

#### 场景：提交考试答案
- **当** 用户提交考试答案时
- **那么** 通过 `IExamService.SubmitAnswerAsync(dto)` 处理
- **那么** 在事务中保存答案和更新分数
- **那么** 返回答题结果和正确答案

### 需求：考试实体命名规范化
系统中考试相关实体类必须遵循 PascalCase 命名：`Exam`、`ExamDetail`、`ExamAnswer`（替代当前的 `exam`、`examdetail`、`examnswer`）。

#### 场景：实体类命名
- **当** 查看考试领域实体定义时
- **那么** 类名为 `Exam`、`ExamDetail`、`ExamAnswer`
- **那么** 属性名使用 PascalCase
- **那么** 通过 Dapper 的 `[Table]` 属性映射到数据库表名
