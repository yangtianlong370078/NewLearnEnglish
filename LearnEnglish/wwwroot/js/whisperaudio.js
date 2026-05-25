var mediaRecorder;
var audioChunks = [];
var autoStopTimer = null; // 用于存储自动停止的定时器


//停止录音
function modelsAudioStop() {
    mediaRecorder.stop();
}

//开始录音
async function startRecording(domtwo,id) {
    //开启录音
    if (!mediaRecorder || mediaRecorder.state === 'inactive') {
        try {
            // 获取麦克风访问权限
            const stream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    sampleRate: 16000, // 16kHz采样率
                    channelCount: 1    // 单声道
                }
            });

            // 设置MediaRecorder
            mediaRecorder = new MediaRecorder(stream);
            audioChunks = [];

            // 处理音频数据
            mediaRecorder.ondataavailable = e => {
                if (e.data.size > 0) {
                    audioChunks.push(e.data);
                }
            };

            // 录音结束后的处理
            mediaRecorder.onstop = async () => {

                // 清除自动停止定时器（如果存在）
                if (autoStopTimer) {
                    clearTimeout(autoStopTimer);
                    autoStopTimer = null;
                }

                // 创建Blob并上传
                const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
                await processRecording(audioBlob, id);

                // 停止所有轨道（释放麦克风）
                stream.getTracks().forEach(track => track.stop());
            };

            // 开始录音
            mediaRecorder.start();

            //开始录音表示
            $(domtwo).addClass("is-recording");

            // **5秒后自动停止录音（如果用户没有手动停止）**
            autoStopTimer = setTimeout(() => {
                if (mediaRecorder && mediaRecorder.state === 'recording') {
                    shibiefy(domtwo, id)
                }
            }, 3500); // 5000ms = 5秒


        } catch (err) {
           
        }
    }
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


