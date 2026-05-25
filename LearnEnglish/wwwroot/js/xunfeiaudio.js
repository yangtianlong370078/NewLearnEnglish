//const startBtn = document.getElementById('startBtn');
//const stopBtn = document.getElementById('stopBtn');
//const resultEl = document.getElementById('result');
let ws;
let audioContext;
let scriptProcessor;
let mediaStream;
let recordingInterval;
let xfAudioChunks = []; // 存储原始PCM数据
const CONFIG = {
    appId: 'b61a9681',
    apiKey: '6bfb85fba69d0c53ec94a2fcc327503b',
    apiSecret: 'YmM1ZmVmYjQ4YmExZjEwNTI5YzQ5ZGNl',
    host: 'iat-api.xfyun.cn',
    path: '/v2/iat',
    sampleRate: "16000", // 必须16k
    channelCount: 1,   // 单声道
    bitDepth: 16       // 16位
};

// 生成签名（不变）
function generateSignature(date) {
    const signStr = `host: ${CONFIG.host}\ndate: ${date}\nGET ${CONFIG.path} HTTP/1.1`;
    const hmac = CryptoJS.HmacSHA256(signStr, CONFIG.apiSecret);
    const base64Sig = CryptoJS.enc.Base64.stringify(hmac);
    const authorizationOrigin = `api_key="${CONFIG.apiKey}", algorithm="hmac-sha256", headers="host date request-line", signature="${base64Sig}"`;
    return CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(authorizationOrigin));
}

// 连接WebSocket（不变）
async function connectWebSocket(domtwo, word) {
    const date = new Date().toUTCString();
    const authorization = generateSignature(date);
    const url = `wss://${CONFIG.host}${CONFIG.path}?host=${CONFIG.host}&date=${encodeURIComponent(date)}&authorization=${encodeURIComponent(authorization)}`;

    ws = new WebSocket(url);
    ws.binaryType = 'arraybuffer';

    ws.onopen = () => {
        console.log('[WebSocket] 连接成功');

        //开始录音标识
        $(domtwo).addClass("is-recording");
        sendStartFrame();
    };

    ws.onmessage = (event) => {
        try {
            const data = JSON.parse(event.data);
            handleResponse(data, word);
        } catch (error) {
            console.error('解析响应失败:', error);
        }
    };

    ws.onerror = (error) => console.error('[WebSocket] 错误:', error);
    ws.onclose = (event) => {
        console.log('[WebSocket] 关闭:', event.code, event.reason);

    };
}

// 发送开始帧（不变）
function sendStartFrame() {
    const frame = {
        common: { app_id: CONFIG.appId },
        business: {
            language: 'en_us',
            accent: 'mandarin',
            domain: 'iat',
            sample_rate: CONFIG.sampleRate, // 数字类型
            vad_eos: 1000,
            nbest: 5
        },
        data: { status: 0 }
    };
    ws.send(JSON.stringify(frame));
}

// 发送音频块（修正：直接使用原始PCM）
function sendAudioChunk(chunk) {
    const base64 = arrayBufferToBase64(chunk);
    ws.send(JSON.stringify({
        data: { status: 1, audio: base64 }
    }));
}

// 处理响应（不变）
function handleResponse(jsonData, word) {
    try {
        if (jsonData.code !== 0) {
            //  resultEl.textContent += `错误: ${jsonData.message}\n`;
            return;
        }
        if (jsonData.data?.result?.ws) {
            const text = jsonData.data.result.ws
                .map(item => item.cw.map(w => w.w.replace(/\s+/g, '').toLowerCase()).join(',')) // 去除单个词内部空格
                .filter(word => word) // 过滤空字符串
                .join('');

            console.log(text);
            // resultEl.textContent = text;
            //当前操作的单词
            currentWord = "";
            var isok = includesIgnoreCase(text, word);

            shibiefy($(".lyzbtn_" + word), word);

            audioResultSet(word, isok);

            if (jsonData.data.status === 2) stopRecording();
        }
    } catch (error) {
        console.error('处理响应失败:', error);
    }
}

function includesIgnoreCase(a, b) {
    // 处理空值情况（可选，根据需求决定是否保留）
    if (!a || !b) return false;
    // 统一转为小写（或大写）后判断包含关系
    return a.toLowerCase().includes(b.toLowerCase());
}

// 开始录音（核心修正：直接捕获原始PCM）
async function xunFeiStartRecording(domtwo, word) {
    try {
        // 1. 初始化音频上下文（强制16k采样率）
        audioContext = new AudioContext({ sampleRate: CONFIG.sampleRate });

        // 2. 获取麦克风流（单声道）
        mediaStream = await navigator.mediaDevices.getUserMedia({
            audio: {
                sampleRate: CONFIG.sampleRate,
                channelCount: CONFIG.channelCount,
                echoCancellation: true // 降噪
            }
        });

        // 3. 创建音频处理器（直接捕获原始数据）
        const source = audioContext.createMediaStreamSource(mediaStream);
        scriptProcessor = audioContext.createScriptProcessor(4096, 1, 1); // 输入1声道，输出1声道

        // 4. 实时捕获原始音频（Float32Array）并转换为L16格式
        scriptProcessor.onaudioprocess = (event) => {
            const inputData = event.inputBuffer.getChannelData(0); // 单声道数据
            const l16Data = float32ToL16(inputData); // 转为16位PCM
            xfAudioChunks.push(l16Data);
        };

        // 5. 连接音频节点
        source.connect(scriptProcessor);
        scriptProcessor.connect(audioContext.destination);

        // 6. 建立WebSocket连接
        await connectWebSocket(domtwo, word);

        // 7. 定时发送音频块（每40ms，与讯飞要求一致）
        recordingInterval = setInterval(() => {
            if (xfAudioChunks.length > 0 && ws?.readyState === WebSocket.OPEN) {
                sendAudioChunk(xfAudioChunks.shift());
            }
        }, 40);

        //  startBtn.disabled = true;
        //  stopBtn.disabled = false;
    } catch (error) {
        console.error('录音启动失败:', error);
    }
}

// 停止录音
function stopRecording() {

    //当前操作的单词
    currentWord = "";

    // 1. 清理音频流和处理器
    if (scriptProcessor) {
        scriptProcessor.disconnect();
        scriptProcessor = null;
    }
    if (mediaStream) {
        mediaStream.getTracks().forEach(track => track.stop());
        mediaStream = null;
    }
    if (audioContext) {
        audioContext.close();
        audioContext = null;
    }

    // 2. 清理定时器
    clearInterval(recordingInterval);

    // 3. 发送结束帧
    if (ws?.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({ data: { status: 2 } }));
    }

    // startBtn.disabled = false;
    // stopBtn.disabled = true;

    // 6. 清空音频数据
    xfAudioChunks = [];
}

// 辅助函数：Float32Array（-1~1）转16位L16格式（Int16Array）
function float32ToL16(float32Data) {
    const buffer = new ArrayBuffer(float32Data.length * 2); // 16位=2字节/样本
    const view = new DataView(buffer);
    for (let i = 0; i < float32Data.length; i++) {
        // 限制范围到[-1, 1]，转换为16位整数（范围-32768~32767）
        const value = Math.max(-1, Math.min(1, float32Data[i]));
        view.setInt16(i * 2, value < 0 ? value * 32768 : value * 32767, true); // 小端序
    }
    return buffer;
}

// 辅助函数：ArrayBuffer转Base64
function arrayBufferToBase64(buffer) {
    return btoa(String.fromCharCode(...new Uint8Array(buffer)));
}

// 事件绑定
//startBtn.addEventListener('click', startRecording);
//stopBtn.addEventListener('click', stopRecording);