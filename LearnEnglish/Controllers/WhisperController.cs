using LearnEnglish.Models;
using LearnEnglish.WhisperModels;
using LearnEnglish.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 语音识别控制器（重构版，使用 IHttpClientFactory + Options 模式）
    /// </summary>
    public class WhisperController : BaseController
    {
        private record DatamuseResponse(string Word, int Score);

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BaiduOptions _baiduOptions;

        private const string BaiduTokenUrl = "https://aip.baidubce.com/oauth/2.0/token";
        private const string BaiduRecognizeUrl = "https://vop.baidu.com/server_api";

        public WhisperController(
            IWebHostEnvironment webHostEnvironment,
            ITranscriptionService transcriptionService,
            IHttpClientFactory httpClientFactory,
            IOptions<BaiduOptions> baiduOptions)
        {
            _webHostEnvironment = webHostEnvironment;
            _transcriptionService = transcriptionService;
            _httpClientFactory = httpClientFactory;
            _baiduOptions = baiduOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        private string ProcessAudioV2(IFormFile audioFile)
        {
            // 创建临时文件路径
            var tempInputPath = Path.GetTempFileName();
            var tempOutputPath = Path.GetTempFileName() + ".wav";
            try
            {
                // 步骤1: 保存上传的音频文件到临时位置
                using (var stream = new FileStream(tempInputPath, FileMode.Create))
                {
                    audioFile.CopyTo(stream);
                }

                // 步骤2: 转换音频格式为16kHz单声道WAV
                ConvertToWavFormat(tempInputPath, tempOutputPath);

                return tempOutputPath;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                // 步骤4: 清理临时文件
                // CleanupTempFiles(tempInputPath, tempOutputPath);
                if (System.IO.File.Exists(tempInputPath))
                {
                    try { System.IO.File.Delete(tempInputPath); } catch { }
                }
            }
        }

        private void ConvertToWavFormat(string inputPath, string outputPath)
        {
            try
            {
                using (var reader = new AudioFileReader(inputPath))
                {

                    // 目标格式：16kHz, 16bit, 单声道
                    var targetFormat = new WaveFormat(16000, 16, 1);

                    if (reader.WaveFormat.Equals(targetFormat))
                    {
                        WaveFileWriter.CreateWaveFile(outputPath, reader);
                        return;
                    }

                    // 创建重采样器
                    var resampler = new MediaFoundationResampler(reader, targetFormat);
                    resampler.ResamplerQuality = 60;

                    // 转换为ISampleProvider
                    var resampledProvider = resampler.ToSampleProvider();

                    // 创建音量控制
                    var volumeProvider = new VolumeSampleProvider(resampledProvider);
                    //volumeProvider.Volume = 1.2f;

                    // 应用噪声抑制
                    //  var noiseSuppression = new NoiseSuppressionSampleProvider(volumeProvider);

                    // 转换为16位PCM
                    var finalProvider = volumeProvider.ToWaveProvider16();

                    // 创建WAV文件
                    WaveFileWriter.CreateWaveFile(outputPath, finalProvider);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to convert audio format", ex);
            }
        }
        [HttpPost]
        //[Authorize]
        public async Task<JsonResult> Recognize(IFormFile audioFile, string word = "", int type = 1)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return Json(new { result = false, scoring = 0, success = true });
            }


            var filePath = ProcessAudioV2(audioFile);

            try
            {
                // var text = await _transcriptionService.TranscribeAudioAsync(filePath);
                bool result = false; int scoring = 0; bool success = false;
                if (type == 1)
                {
                    (result, scoring, success) = await benIdModel(filePath, word);
                }
                else if (type == 3)
                {
                    var token = await GetToken();
                    (result, scoring, success) = await BaiDuModel(new RecognitionRequest
                    {
                        Token = token,
                        Cuid = "baidu_speech_demo",
                        Lan = "zh"
                    }, filePath, word);
                }


                return Json(new { result, scoring, success });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Transcription failed: {ex.Message}");
            }
            finally
            {
                //  清理临时文件
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); } catch { }
                }
            }
            return Json(new { result = false, scoring = 0, success = true });
        }

        /// <summary>
        /// 获取百度API的访问令牌
        /// </summary>
        public async Task<string> GetToken()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{BaiduTokenUrl}?grant_type=client_credentials&client_id={_baiduOptions.ApiKey}&client_secret={_baiduOptions.ApiSecret}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    var token = JsonConvert.DeserializeObject<BaiduTokenModel>(result);
                    return token?.access_token ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        // 替换ReadPcmFromWavStream方法，确保输出纯净PCM
        public byte[] ReadPcmFromWavStream(FileStream wavStream)
        {
            using (var reader = new WaveFileReader(wavStream))
            {
                // 强制转换为目标格式（16kHz/16bit/单声道）
                var targetFormat = new WaveFormat(16000, 16, 1);
                using (var converter = new WaveFormatConversionStream(targetFormat, reader))
                {
                    var buffer = new byte[converter.Length];
                    converter.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
        }
        public byte[] ReadPcmFromWavStreamV2(FileStream wavStream)
        {
            using var reader = new WaveFileReader(wavStream);
            if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            {
                // 如果输入不是PCM，尝试转换（需安装NAudio.Lame等库）
                throw new InvalidDataException("Input WAV must be PCM format.");
            }

            // 直接读取原始PCM数据（WaveFileReader已跳过WAV头）
            var buffer = new byte[reader.Length];
            reader.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        // 辅助方法：读取 PCM 头部信息

        public async Task<(bool result, int scoring, bool success)> BaiDuModel(RecognitionRequest request, string filePath, string word)
        {
            // 构建请求URL
            //var url = $"{_recognizeUrl}?cuid={WebUtility.UrlEncode(request.Cuid)}&dev_pid=1737&rate=16000&access_token={Uri.EscapeDataString(request.Token)}";
            //var url = $"{_recognizeUrl}?cuid={request.Cuid}&dev_pid=1737";

            using var audioStream = System.IO.File.OpenRead(filePath ?? "");
            var audioBytes = ReadPcmFromWavStreamV2(audioStream);
            // Console.WriteLine($"PCM 实际采样率: {GetSampleRate(audioBytes)}"); // 调试输出
            Console.WriteLine($"Audio Length: {audioBytes.Length} bytes");

            // 将音频转换为Base64编码
            string base64Audio = Convert.ToBase64String(audioBytes);

            // 构建百度API请求参数
            var requestData = new
            {
                format = "pcm",       // 音频格式
                rate = 16000,          // 采样率
                channel = 1,    // 声道数
                cuid = Guid.NewGuid().ToString(), // 建议提供
                dev_pid = 1737,
                token = request.Token,
                speech = base64Audio, // 音频Base64
                len = audioBytes.Length, // 音频字节长度
                
            };
            var jsonContentstr = JsonConvert.SerializeObject(requestData);

            var client = _httpClientFactory.CreateClient();
            using var content = new StringContent(jsonContentstr, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(BaiduRecognizeUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"语音识别请求失败，状态码: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(result))
            {
                BaiduResultModel? model = JsonConvert.DeserializeObject<BaiduResultModel>(result);

                if (model != null)
                {
                    List<string> words = model.result;
                    words = words.Select(s => s.Replace(" ", "")).ToList();
                    var isok = words.Any(w => w.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (isok)
                    {
                        return (true, 1, true);
                    }

                    // 2. 并行启动两个异步任务（不使用 await，先获取 Task）
                    var homophonesTask = GetHomophonesAsync(words[0]);
                    var similarSoundingsTask = GetSimilarSoundingsAsync(words[0]);

                    // 3. 等待两个任务全部完成
                    await Task.WhenAll(homophonesTask, similarSoundingsTask);

                    // 4. 安全获取结果（处理 null 情况）
                    var xiantonyin = homophonesTask.Result ?? new List<string>();
                    var xianshiyin = similarSoundingsTask.Result ?? new List<string>();

                    // 5. 合并所有单词（去重可选）
                    var words2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // 添加同音词和近音词
                    words2.UnionWith(xiantonyin);
                    words2.UnionWith(xianshiyin);

                    isok = words2.Select(s => s.Replace(" ", "")).Any(w => w.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
                    return (isok, 2, true);

                }
            }
            return (false, 2, false);
        }



        public async Task<(bool result, int scoring, bool success)> benIdModel(string filePath, string word)
        {
            var similarWords = await ((WhisperTranscriptionService)_transcriptionService).TranscribeAndFindPhoneticallySimilarWordsAsync(filePath, 0);

            var recognizedWord = similarWords.targetWord;
            var baseSimilarWords = similarWords.similarWords ?? new List<string>();

            if (recognizedWord.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return (true, 1, true);
            }

            // 2. 并行启动两个异步任务（不使用 await，先获取 Task）
            var homophonesTask = GetHomophonesAsync(recognizedWord);
            var similarSoundingsTask = GetSimilarSoundingsAsync(recognizedWord);

            // 3. 等待两个任务全部完成
            await Task.WhenAll(homophonesTask, similarSoundingsTask);

            // 4. 安全获取结果（处理 null 情况）
            var xiantonyin = homophonesTask.Result ?? new List<string>();
            var xianshiyin = similarSoundingsTask.Result ?? new List<string>();

            // 5. 合并所有单词（去重可选）
            var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 添加原始相似词
            words.UnionWith(baseSimilarWords);
            words.Add(recognizedWord); // 添加识别出的词本身

            // 添加同音词和近音词
            words.UnionWith(xiantonyin);
            words.UnionWith(xianshiyin);

            var isok = words.Select(s => s.Replace(" ", "")).Any(w => w.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

            return (isok, 2, true);
        }


        // 获取同音词
        public async Task<List<string>?> GetHomophonesAsync(string word)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<DatamuseResponse[]>(
                $"https://api.datamuse.com/words?sl={word}&max=10"
            );
            return response?.Where(r => r.Score > 80).Select(r => r.Word).ToList();
        }

        // 获取相似音词
        public async Task<List<string>?> GetSimilarSoundingsAsync(string word)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<DatamuseResponse[]>(
                $"https://api.datamuse.com/words?sl={word}&max=10"
            );
            return response?.Select(r => r.Word).ToList();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
