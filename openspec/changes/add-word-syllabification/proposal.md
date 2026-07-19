# 变更：为单词详情提供音节拆分

## 为什么
前端在展示单词详情时需要获得稳定的英文音节拆分结果。当前 `lexiconDeatil` 接口未返回该数据，并且不能在每个请求中重复读取 CMU 字典文件。

## 变更内容
- 新增单词音节拆分服务：应用启动时将 CMU 词典加载到内存。
- 对 CMU 中的词条，按 CMU ARPABET 音素及重音元音边界生成音节；无词条时使用内置英文规则拆分兜底。Natural.NET 在 NuGet.org 不存在可安装版本，因此无法作为运行时依赖。
- 将 CMU 字典作为运行时内容复制到输出与发布目录，并通过配置确定词典路径。
- 在 `GET /api/Word/lexiconDeatil` 响应的 `data` 对象中新增 `syllables` 字段，返回按顺序排列的音节字符串数组。
- 添加单元测试，覆盖 CMU 命中、CMU 未命中时规则兜底，以及空白输入。

## 影响
- 受影响规范：新增 `word-syllabification` 能力。
- 受影响代码：`LearnEnglish.Infrastructure` 服务与依赖注册、`LearnEnglish.WebApi` 的配置与控制器、项目文件、单元测试。
- 新增依赖：无；音节兜底规则在基础设施层内实现。
