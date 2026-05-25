using LearnEnglish.Infrastructure.Configuration;
using LearnEnglish.Models;
using LearnEnglish.WhisperModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System.Text;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 语音识别 API
    /// </summary>
    [Route("api/[controller]")]
    public class WhisperController : ApiControllerBase
    {
        private record DatamuseResponse(string Word, int Score);

        private readonly ITranscriptionService _transcriptionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BaiduOptions _baiduOptions;

        private const string BaiduTokenUrl = "https://aip.baidubce.com/oauth/2.0/token";
        private const string BaiduRecognizeUrl = "https://vop.baidu.com/server_api";

        public WhisperController(
            ITranscriptionService transcriptionService,
            IHttpClientFactory httpClientFactory,
            IOptions<BaiduOptions> baiduOptions)
        {
            _transcriptionService = transcriptionService;
            _httpClientFactory = httpClientFactory;
            _baiduOptions = baiduOptions.Value;
        }

        /// <summary>语音识别主入口</summary>
        [HttpPost("Recognize")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Recognize(IFormFile audioFile, [FromForm] string word = "", [FromForm] int type = 1)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return Ok(new { result = false, scoring = 0, success = true });
            }

            var filePath = ProcessAudioV2(audioFile);
            try
            {
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

                return Ok(new { result, scoring, success });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Transcription failed: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); } catch { }
                }
            }

            return Ok(new { result = false, scoring = 0, success = true });
        }

        /// <summary>获取百度访问令牌</summary>
        [HttpGet("GetToken")]
        public async Task<IActionResult> GetTokenEndpoint()
        {
            var token = await GetToken();
            return Ok(new { token, success = !string.IsNullOrEmpty(token) });
        }

        #region 内部实现

        private string ProcessAudioV2(IFormFile audioFile)
        {
            var tempInputPath = Path.GetTempFileName();
            var tempOutputPath = Path.GetTempFileName() + ".wav";
            try
            {
                using (var stream = new FileStream(tempInputPath, FileMode.Create))
                {
                    audioFile.CopyTo(stream);
                }
                ConvertToWavFormat(tempInputPath, tempOutputPath);
                return tempOutputPath;
            }
            finally
            {
                if (System.IO.File.Exists(tempInputPath))
                {
                    try { System.IO.File.Delete(tempInputPath); } catch { }
                }
            }
        }

        private void ConvertToWavFormat(string inputPath, string outputPath)
        {
            using var reader = new AudioFileReader(inputPath);
            var targetFormat = new WaveFormat(16000, 16, 1);

            if (reader.WaveFormat.Equals(targetFormat))
            {
                WaveFileWriter.CreateWaveFile(outputPath, reader);
                return;
            }

            var resampler = new MediaFoundationResampler(reader, targetFormat);
            resampler.ResamplerQuality = 60;
            var resampledProvider = resampler.ToSampleProvider();
            var volumeProvider = new VolumeSampleProvider(resampledProvider);
            var finalProvider = volumeProvider.ToWaveProvider16();
            WaveFileWriter.CreateWaveFile(outputPath, finalProvider);
        }

        private async Task<string> GetToken()
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
            catch
            {
                return string.Empty;
            }
        }

        private static byte[] ReadPcmFromWavStreamV2(FileStream wavStream)
        {
            using var reader = new WaveFileReader(wavStream);
            if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            {
                throw new InvalidDataException("Input WAV must be PCM format.");
            }
            var buffer = new byte[reader.Length];
            reader.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private async Task<(bool result, int scoring, bool success)> BaiDuModel(RecognitionRequest request, string filePath, string word)
        {
            using var audioStream = System.IO.File.OpenRead(filePath ?? "");
            var audioBytes = ReadPcmFromWavStreamV2(audioStream);
            string base64Audio = Convert.ToBase64String(audioBytes);

            var requestData = new
            {
                format = "pcm",
                rate = 16000,
                channel = 1,
                cuid = Guid.NewGuid().ToString(),
                dev_pid = 1737,
                token = request.Token,
                speech = base64Audio,
                len = audioBytes.Length,
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
                var model = JsonConvert.DeserializeObject<BaiduResultModel>(result);
                if (model != null)
                {
                    List<string> words = model.result;
                    words = words.Select(s => s.Replace(" ", "")).ToList();
                    var isok = words.Any(w => w.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (isok) return (true, 1, true);

                    var homophonesTask = GetHomophonesAsync(words[0]);
                    var similarSoundingsTask = GetSimilarSoundingsAsync(words[0]);
                    await Task.WhenAll(homophonesTask, similarSoundingsTask);

                    var xiantonyin = homophonesTask.Result ?? new List<string>();
                    var xianshiyin = similarSoundingsTask.Result ?? new List<string>();

                    var words2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    words2.UnionWith(xiantonyin);
                    words2.UnionWith(xianshiyin);

                    isok = words2.Select(s => s.Replace(" ", "")).Any(w => w.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
                    return (isok, 2, true);
                }
            }
            return (false, 2, false);
        }

        private async Task<(bool result, int scoring, bool success)> benIdModel(string filePath, string word)
        {
            var similarWords = await ((WhisperTranscriptionService)_transcriptionService).TranscribeAndFindPhoneticallySimilarWordsAsync(filePath, 0);

            var recognizedWord = similarWords.targetWord;
            var baseSimilarWords = similarWords.similarWords ?? new List<string>();

            if (recognizedWord.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return (true, 1, true);
            }

            var homophonesTask = GetHomophonesAsync(recognizedWord);
            var similarSoundingsTask = GetSimilarSoundingsAsync(recognizedWord);
            await Task.WhenAll(homophonesTask, similarSoundingsTask);

            var xiantonyin = homophonesTask.Result ?? new List<string>();
            var xianshiyin = similarSoundingsTask.Result ?? new List<string>();

            var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            words.UnionWith(baseSimilarWords);
            words.Add(recognizedWord);
            words.UnionWith(xiantonyin);
            words.UnionWith(xianshiyin);

            var isok = words.Select(s => s.Replace(" ", "")).Any(w => w.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

            return (isok, 2, true);
        }

        private async Task<List<string>?> GetHomophonesAsync(string word)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<DatamuseResponse[]>(
                $"https://api.datamuse.com/words?sl={word}&max=10"
            );
            return response?.Where(r => r.Score > 80).Select(r => r.Word).ToList();
        }

        private async Task<List<string>?> GetSimilarSoundingsAsync(string word)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<DatamuseResponse[]>(
                $"https://api.datamuse.com/words?sl={word}&max=10"
            );
            return response?.Select(r => r.Word).ToList();
        }

        #endregion
    }
}
