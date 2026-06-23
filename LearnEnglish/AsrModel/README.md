---

license: Apache License 2.0
language:
  - zh
frameworks: PyTorch
tasks:
  - auto-speech-recognition
base_model_relation: quantized
base_model:
  - FunAudioLLM/Fun-ASR-Nano-2512
---
# FunASR-CTC-Nano-INT8-ONNX

基于 [Fun-ASR-Nano](https://www.modelscope.cn/models/FunAudioLLM/Fun-ASR-Nano-2512) 的 ONNX INT8 量化语音识别模型。


## 模型文件

| 文件 | 大小 | 说明 |
|-----|------|------|
| `encoder.int8.onnx` | ~50MB | 音频编码器 |
| `ctc.int8.onnx` | ~10MB | CTC 解码器 |
| `tokens.txt` | ~100KB | Token 词汇表 |

## 使用方法

### Android 平台

```kotlin
val engine = FunASRCTCEngine(context)
engine.initialize()

// 识别音频
val result = engine.transcribe(audioFilePath)
println(result.text)

## 原始模型

* **来源**: [FunAudioLLM/Fun-ASR-Nano-2512](https://www.modelscope.cn/models/FunAudioLLM/Fun-ASR-Nano-2512)

* **许可证**: Apache 2.0

* **参数量**: 800M

## 引用

```bibtex
@article{an2025fun,
  title={Fun-ASR Technical Report},
  author={An, Keyu and Chen, Yanni and Deng, Chong and others},
  journal={arXiv preprint arXiv:2509.12508},
  year={2025}
}
```

## 许可证

Apache License 2.0
