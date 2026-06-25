var autoStopTimer = null; // 用于存储自动停止的定时器

// 录音相关变量（与讯飞录音保持一致的原始 PCM 采集方式）
var whisperAudioContext;
var whisperScriptProcessor;
var whisperMediaStream;
var whisperPcmChunks = [];   // 存储原始 PCM（L16）数据
var whisperIsRecording = false;
var whisperWord = "";        // 当前录音对应的单词

// 录音参数（与 xunfeiaudio.js 中 CONFIG 完全一致）
const WHISPER_CONFIG = {
    sampleRate: 16000, // 必须16k
    channelCount: 1,   // 单声道
    bitDepth: 16       // 16位
};


//停止录音
function modelsAudioStop() {
    stopWhisperRecording();
}

//开始录音
async function startRecording(domtwo, id) {
    // 若已在录音中则忽略
    if (whisperIsRecording) {
        return;
    }

    try {
        whisperWord = id;
        whisperPcmChunks = [];

        // 1. 初始化音频上下文（强制16k采样率）
        whisperAudioContext = new AudioContext({ sampleRate: WHISPER_CONFIG.sampleRate });

        // 2. 获取麦克风流（单声道）
        whisperMediaStream = await navigator.mediaDevices.getUserMedia({
            audio: {
                sampleRate: WHISPER_CONFIG.sampleRate,
                channelCount: WHISPER_CONFIG.channelCount,
                echoCancellation: true // 降噪
            }
        });

        // 3. 创建音频处理器（直接捕获原始数据）
        const source = whisperAudioContext.createMediaStreamSource(whisperMediaStream);
        whisperScriptProcessor = whisperAudioContext.createScriptProcessor(4096, 1, 1); // 输入1声道，输出1声道

        // 4. 实时捕获原始音频（Float32Array）并转换为L16格式
        whisperScriptProcessor.onaudioprocess = (event) => {
            const inputData = event.inputBuffer.getChannelData(0); // 单声道数据
            whisperPcmChunks.push(whisperFloat32ToL16(inputData)); // 转为16位PCM
        };

        // 5. 连接音频节点
        source.connect(whisperScriptProcessor);
        whisperScriptProcessor.connect(whisperAudioContext.destination);

        whisperIsRecording = true;

        //开始录音表示
        $(domtwo).addClass("is-recording");

        // **3.5秒后自动停止录音（如果用户没有手动停止）**
        autoStopTimer = setTimeout(() => {
            if (whisperIsRecording) {
                shibiefy(domtwo, id)
            }
        }, 3500);

    } catch (err) {

    }
}

// 停止录音并将采集到的原始 PCM 封装为 WAV 后发送到后端
function stopWhisperRecording() {
    if (!whisperIsRecording) {
        return;
    }
    whisperIsRecording = false;

    // 1. 清除自动停止定时器（如果存在）
    if (autoStopTimer) {
        clearTimeout(autoStopTimer);
        autoStopTimer = null;
    }

    // 2. 清理音频流和处理器
    if (whisperScriptProcessor) {
        whisperScriptProcessor.disconnect();
        whisperScriptProcessor = null;
    }
    if (whisperMediaStream) {
        whisperMediaStream.getTracks().forEach(track => track.stop());
        whisperMediaStream = null;
    }
    if (whisperAudioContext) {
        whisperAudioContext.close();
        whisperAudioContext = null;
    }

    // 3. 将原始 PCM 数据封装为 WAV 文件并发送到后端
    const audioBlob = whisperEncodeWAV(whisperPcmChunks, WHISPER_CONFIG.sampleRate, WHISPER_CONFIG.channelCount, WHISPER_CONFIG.bitDepth);
    whisperPcmChunks = [];
    processRecording(audioBlob, whisperWord);
}

// 辅助函数：Float32Array（-1~1）转16位L16格式（小端序，与讯飞一致）
function whisperFloat32ToL16(float32Data) {
    const buffer = new ArrayBuffer(float32Data.length * 2); // 16位=2字节/样本
    const view = new DataView(buffer);
    for (let i = 0; i < float32Data.length; i++) {
        // 限制范围到[-1, 1]，转换为16位整数（范围-32768~32767）
        const value = Math.max(-1, Math.min(1, float32Data[i]));
        view.setInt16(i * 2, value < 0 ? value * 32768 : value * 32767, true); // 小端序
    }
    return buffer;
}

// 辅助函数：将多段 L16 PCM 数据封装为标准 WAV 文件 Blob
function whisperEncodeWAV(pcmChunks, sampleRate, channelCount, bitDepth) {
    let dataLength = 0;
    for (let i = 0; i < pcmChunks.length; i++) {
        dataLength += pcmChunks[i].byteLength;
    }

    const blockAlign = channelCount * bitDepth / 8;
    const byteRate = sampleRate * blockAlign;
    const buffer = new ArrayBuffer(44 + dataLength);
    const view = new DataView(buffer);

    const writeString = (offset, str) => {
        for (let i = 0; i < str.length; i++) {
            view.setUint8(offset + i, str.charCodeAt(i));
        }
    };

    // RIFF chunk descriptor
    writeString(0, 'RIFF');
    view.setUint32(4, 36 + dataLength, true);
    writeString(8, 'WAVE');
    // fmt sub-chunk
    writeString(12, 'fmt ');
    view.setUint32(16, 16, true);          // Subchunk1Size (PCM)
    view.setUint16(20, 1, true);           // AudioFormat = 1 (PCM)
    view.setUint16(22, channelCount, true);
    view.setUint32(24, sampleRate, true);
    view.setUint32(28, byteRate, true);
    view.setUint16(32, blockAlign, true);
    view.setUint16(34, bitDepth, true);
    // data sub-chunk
    writeString(36, 'data');
    view.setUint32(40, dataLength, true);

    // 写入 PCM 数据
    let offset = 44;
    for (let i = 0; i < pcmChunks.length; i++) {
        const src = new Uint8Array(pcmChunks[i]);
        new Uint8Array(buffer, offset, src.length).set(src);
        offset += src.length;
    }

    return new Blob([buffer], { type: 'audio/wav' });
}

//结果
async function processRecording(audioBlob, word) {
    debugger
    try {
        let type = $("#sbmodeltype").val();
        // 创建FormData
        const formData = new FormData();
        formData.append('audioFile', audioBlob, 'recording.wav');
        formData.append('word', word);
        formData.append('type', type);
        // 上传到服务器
        const response = await fetch('/Whisper/Recognize', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Server responded with ${response.status}`);
        }
        //当前操作的单词 
        currentWord = "";
        // 解析结果
        const result = await response.text();

        if (result != "") {
            let jsonData = JSON.parse(result);
            if (jsonData.success) {
                audioResultSet(word,jsonData.result);
            }
        }

    } catch (err) {
        
    }
}


