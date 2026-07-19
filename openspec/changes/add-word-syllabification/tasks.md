## 1. 实施
- [x] 1.1 在 Application 层定义音节拆分服务接口和配置模型。
- [x] 1.2 在 Infrastructure 实现 CMU 内存加载、CMU 优先拆分与内置英文规则兜底（Natural.NET 在 NuGet.org 无可用版本）。
- [x] 1.3 注册单例服务，并配置 CMU 词典随 WebApi 构建和发布输出。
- [x] 1.4 在 `lexiconDeatil` 响应的 `data` 对象中增加 `syllables` 字段。
- [x] 1.5 添加 CMU 命中、规则兜底与空输入的单元测试。
- [x] 1.6 执行还原、构建和测试，并修复与本变更相关的问题。
