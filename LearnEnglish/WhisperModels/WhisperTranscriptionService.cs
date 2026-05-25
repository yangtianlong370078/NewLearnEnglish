using Microsoft.Extensions.ObjectPool;
using Whisper.net;

namespace LearnEnglish.WhisperModels
{
    public class WhisperTranscriptionService : ITranscriptionService, IDisposable
    {
        private readonly string _modelPath;
        private readonly WhisperFactory _factory;
        private readonly ObjectPool<WhisperProcessor> _processorPool;
        private readonly int _maxPoolSize;
        private readonly PhoneticsHelper _phoneticsHelper;


        public WhisperTranscriptionService(string modelPath, string cmuDictPath)
        {
            _modelPath = modelPath;

            try
            {
                // 单例加载模型工厂
                _factory = WhisperFactory.FromPath(_modelPath);

                // 设置池大小（建议为CPU核心数×2）
                _maxPoolSize = Environment.ProcessorCount * 2;

                // 创建处理器对象池
                _processorPool = new DefaultObjectPool<WhisperProcessor>(
                    new WhisperProcessorPoolPolicy(_factory),
                    _maxPoolSize
                );


                // 加载发音词典
                _phoneticsHelper = new PhoneticsHelper(cmuDictPath);

                // 预热对象池
                PrewarmPool();
            }
            catch (Exception e)
            {

                throw;
            }
           
        }

        // 👉 新增方法：识别并返回发音相近的单词
        public async Task<(List<string> similarWords, string targetWord)> TranscribeAndFindPhoneticallySimilarWordsAsync(string audioFilePath, int top = 0)
        {
            var text = await TranscribeAudioAsync(audioFilePath);
            var words = text.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0) return (new List<string>(), string.Empty);

            // 取第一个单词做发音匹配（适用于单词发音练习）
            var targetWord =string.Join("", words).ToLowerInvariant();

            //本地算法查找相适音单词
            List<string> similarWords = new List<string>();

            if(top>0)
            {
                similarWords = _phoneticsHelper.FindSimilarPhoneticWords(targetWord, top)
               .Select(x => x.word)
               .Where(w => w != targetWord) // 排除自己
               .ToList();
            }
            return (similarWords, targetWord);
        }



        private void PrewarmPool()
        {
            var warmupProcessors = new List<WhisperProcessor>();
            int prewarmCount = Math.Min(4, _maxPoolSize);

            for (int i = 0; i < prewarmCount; i++)
            {
                warmupProcessors.Add(_processorPool.Get());
            }

            foreach (var processor in warmupProcessors)
            {
                _processorPool.Return(processor);
            }
        }

        public async Task<string> TranscribeAudioAsync(string audioFilePath)
        {

            var processor = _processorPool.Get();
            try
            {
                using var audioStream = File.OpenRead(audioFilePath);
                var segments = new List<string>();

                await foreach (var result in processor.ProcessAsync(audioStream))
                {
                    segments.Add(result.Text);
                }

                return string.Join(" ", segments);
            }
            finally
            {
                _processorPool.Return(processor);
            }


        }

        // 自定义对象池策略
        private class WhisperProcessorPoolPolicy : IPooledObjectPolicy<WhisperProcessor>
        {
            private readonly WhisperFactory _factory;

            public WhisperProcessorPoolPolicy(WhisperFactory factory)
            {
                _factory = factory;
            }

            public WhisperProcessor Create()
            {
                //return _factory.CreateBuilder()
                //    .WithLanguage("en")
                //    .WithThreads(Environment.ProcessorCount / 2)

                //      .WithTokenTimestamps() // 关键：启用词元级时间戳
                //        .WithTemperature(0.0F) 

                //    .Build();


                // 精准度优化配置
                var builder = _factory.CreateBuilder()
                    .WithLanguage("en")                  // 强制英语识别
                    .WithThreads(Environment.ProcessorCount / 2) //优化线程使用
                   // .WithTemperature(0.9f)               // 确定性模式（适合清晰音频）

                   // .WithSingleSegment()


                    // .WithTokenTimestamps()            // 启用单词级时间戳和置信度
                    //.WithMaxAlternatives(3)              // 生成3个备选结果
                    // .WithBeamSize(5)                     // 波束搜索宽度（平衡速度和精度）
                    .WithNoContext();  //不使用上一段的上下文（更独立）

             //var beamBuiler =   builder.WithBeamSearchSamplingStrategy() as BeamSearchSamplingStrategyBuilder;

             //   if(beamBuiler!=null)
             //   {
             //       beamBuiler.WithBeamSize(15);
             //         //  .WithPatience(1.5f);

             //   }
                



                    return builder.Build();


            }

            public bool Return(WhisperProcessor processor)
            {
                // 这里可以添加处理器重置逻辑
                return true;
            }
        }

        public void Dispose()
        {
            DisposeProcessors();
            _factory?.Dispose();
        }

        private void DisposeProcessors()
        {
            // 释放池中所有处理器
            var processorsToDispose = new List<WhisperProcessor>();

            // 获取池中所有可用对象
            for (int i = 0; i < _maxPoolSize; i++)
            {
                try
                {
                    var processor = _processorPool.Get();
                    processorsToDispose.Add(processor);
                }
                catch
                {
                    // 池中无更多对象时退出
                    break;
                }
            }

            // 释放所有处理器
            foreach (var processor in processorsToDispose)
            {
                processor.Dispose();
            }

            // 注意：这里不返还到池中，因为正在销毁
        }
    }
}
