## 新增需求

### 需求：语音识别服务层
系统必须保持现有 Whisper 语音识别的接口抽象模式（`ITranscriptionService`），并将 WhisperController 中的百度语音 API 调用提取到 `ISpeechRecognitionService` 中。

#### 场景：Whisper 本地语音识别
- **当** 用户上传音频进行本地识别时
- **那么** 通过 `ITranscriptionService.TranscribeAsync(audio)` 处理
- **那么** 使用对象池管理 Whisper 处理器资源（保持现有优秀模式）

#### 场景：百度语音 API 调用
- **当** 用户使用在线语音识别时
- **那么** 通过 `ISpeechRecognitionService.RecognizeAsync(audio)` 处理
- **那么** HTTP 客户端通过 `IHttpClientFactory` 创建
- **那么** API 密钥从配置读取

### 需求：音频处理工具类
系统中 WhisperController 内定义的内联类（`AudioConverter`、`WavHeader` 等）必须提取为独立的工具类，放置在 Infrastructure 层的 `Audio` 命名空间下。

#### 场景：音频格式转换
- **当** 需要转换音频格式时
- **那么** 通过 `IAudioConverter.ConvertToWavAsync(stream)` 处理
- **那么** 音频工具类不依赖 Controller 上下文
