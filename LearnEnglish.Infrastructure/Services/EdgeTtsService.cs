using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Exceptions;
using LearnEnglish.Infrastructure.Redis;

using EdgeTTS.DotNet;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LearnEnglish.Infrastructure.Services
{
    public class EdgeTtsService : IEdgeTtsService
    {
        private const string AudioCacheKeyPrefix = "edge-tts:audio:";
        private const int CacheDays = 7;
        private const int MaxRetry = 2;
        private const int MaxConcurrentSynthesis = 3;
        private const int UserWindowLimit = 6;
        private const int UserWindowSeconds = 20;
        private const int MaxTextLength = 220;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);
        private static readonly SemaphoreSlim GlobalSynthesisSemaphore = new(MaxConcurrentSynthesis, MaxConcurrentSynthesis);
        private static readonly ConcurrentDictionary<string, Lazy<Task<byte[]>>> InflightRequests = new();
        private static readonly string[] AmericanFallbackVoices = [VoiceEnUsJenny, VoiceEnUsGuy, VoiceEnGbSonia, VoiceEnGbRyan];
        private static readonly string[] BritishFallbackVoices = [VoiceEnGbSonia, VoiceEnGbRyan, VoiceEnUsJenny, VoiceEnUsGuy];

        private readonly IMemoryCache _memoryCache;
        private readonly IRedisService _redisService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EdgeTtsService> _logger;

        #region 常用英文音色常量
        public const string VoiceEnUsJenny = "en-US-JennyNeural";
        public const string VoiceEnUsGuy = "en-US-GuyNeural";
        public const string VoiceEnGbSonia = "en-GB-SoniaNeural";
        public const string VoiceEnGbRyan = "en-GB-RyanNeural";
        #endregion

        public EdgeTtsService(
            IMemoryCache memoryCache,
            IRedisService redisService,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EdgeTtsService> logger)
        {
            _memoryCache = memoryCache;
            _redisService = redisService;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<byte[]> GetAudioBytesAsync(string text, string voice)
        {
            var normalizedText = NormalizeText(text);
            var requestedVoice = string.IsNullOrWhiteSpace(voice) ? VoiceEnUsJenny : voice.Trim();
            var cacheKey = BuildAudioCacheKey(normalizedText, requestedVoice);

            if (_memoryCache.TryGetValue<byte[]>(cacheKey, out var cachedBytes) && cachedBytes is { Length: > 0 })
            {
                return cachedBytes;
            }

            var redisBase64 = await _redisService.GetAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(redisBase64))
            {
                var redisBytes = Convert.FromBase64String(redisBase64);
                CacheInMemory(cacheKey, redisBytes);
                return redisBytes;
            }

            EnforceUserSlidingWindow();

            var lazyTask = InflightRequests.GetOrAdd(
                cacheKey,
                _ => new Lazy<Task<byte[]>>(() => GenerateAndCacheAsync(normalizedText, requestedVoice, cacheKey)));

            try
            {
                return await lazyTask.Value;
            }
            finally
            {
                InflightRequests.TryRemove(cacheKey, out _);
            }
        }

        private async Task<byte[]> GenerateAndCacheAsync(string text, string requestedVoice, string cacheKey)
        {
            await GlobalSynthesisSemaphore.WaitAsync();
            try
            {
                Exception? lastException = null;

                foreach (var fallbackVoice in BuildVoiceFallbackChain(requestedVoice))
                {
                    try
                    {
                        var audioBytes = await SynthesizeWithRetryAsync(text, fallbackVoice);
                        CacheInMemory(cacheKey, audioBytes);
                        await _redisService.SetAsync(cacheKey, Convert.ToBase64String(audioBytes), TimeSpan.FromDays(CacheDays));

                        if (!string.Equals(fallbackVoice, requestedVoice, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Edge TTS 音色降级: {RequestedVoice} -> {FallbackVoice}", requestedVoice, fallbackVoice);
                        }

                        return audioBytes;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        _logger.LogWarning(ex, "Edge TTS 音色 {Voice} 合成失败，尝试下一候选音色", fallbackVoice);
                    }
                }

                throw lastException ?? new InvalidOperationException("语音合成失败");
            }
            finally
            {
                GlobalSynthesisSemaphore.Release();
            }
        }

        private async Task<byte[]> SynthesizeWithRetryAsync(string text, string voice)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    return await SynthesizeToBytesAsync(text, voice);
                }
                catch (Exception ex) when (attempt < MaxRetry && IsRetriable(ex))
                {
                    _logger.LogWarning(ex, "Edge TTS 合成重试，第 {Attempt} 次: Voice={Voice}", attempt + 1, voice);
                    await Task.Delay(RetryDelay);
                }
            }
        }

        private static async Task<byte[]> SynthesizeToBytesAsync(string text, string voice)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"edge-tts-{Guid.NewGuid():N}.mp3");
            try
            {
                var request = new Communicate(text, voice: voice, rate: "-20%");
                await request.SaveAsync(tempPath);
                return await File.ReadAllBytesAsync(tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        private void EnforceUserSlidingWindow()
        {
            var cacheKey = $"edge-tts:rate:{ResolveUserKey()}";
            var state = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                return new SlidingWindowState();
            })!;

            var now = DateTimeOffset.UtcNow;
            var windowStart = now.AddSeconds(-UserWindowSeconds);

            lock (state.SyncRoot)
            {
                while (state.Timestamps.Count > 0 && state.Timestamps.Peek() < windowStart)
                {
                    state.Timestamps.Dequeue();
                }

                if (state.Timestamps.Count >= UserWindowLimit)
                {
                    throw new TooManyRequestsException("语音请求过于频繁，请稍后再试");
                }

                state.Timestamps.Enqueue(now);
            }
        }

        private string ResolveUserKey()
        {
            if (_currentUserService.UserId is int userId)
            {
                return $"user:{userId}";
            }

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                return $"ip:{ipAddress}";
            }

            return "anonymous";
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ValidationException("文本不能为空");
            }

            var normalized = Regex.Replace(text.Trim(), "\\s+", " ");
            if (normalized.Length > MaxTextLength)
            {
                throw new ValidationException($"仅支持 {MaxTextLength} 个字符以内的英文短句朗读");
            }

            return normalized;
        }

        private static string BuildAudioCacheKey(string text, string voice)
        {
            var payload = $"{voice}|{text}";
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            return $"{AudioCacheKeyPrefix}{hash}";
        }

        private static IEnumerable<string> BuildVoiceFallbackChain(string requestedVoice)
        {
            var fallbackPool = requestedVoice.StartsWith("en-GB", StringComparison.OrdinalIgnoreCase)
                ? BritishFallbackVoices
                : AmericanFallbackVoices;

            return new[] { requestedVoice }
                .Concat(fallbackPool)
                .Where(voice => !string.IsNullOrWhiteSpace(voice))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsRetriable(Exception exception)
        {
            if (exception is TimeoutException or TaskCanceledException or IOException or System.Net.WebSockets.WebSocketException or EdgeTTS.DotNet.WebSocketException)
            {
                return true;
            }

            if (exception is HttpRequestException httpRequestException && httpRequestException.StatusCode is null or System.Net.HttpStatusCode.Forbidden)
            {
                return true;
            }

            var message = exception.ToString();
            return message.Contains("403", StringComparison.OrdinalIgnoreCase)
                || message.Contains("forbidden", StringComparison.OrdinalIgnoreCase)
                || message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                || message.Contains("timed out", StringComparison.OrdinalIgnoreCase);
        }

        private void CacheInMemory(string cacheKey, byte[] bytes)
        {
            _memoryCache.Set(cacheKey, bytes, TimeSpan.FromHours(1));
        }

        private sealed class SlidingWindowState
        {
            public object SyncRoot { get; } = new();

            public Queue<DateTimeOffset> Timestamps { get; } = new();
        }
    }
}
