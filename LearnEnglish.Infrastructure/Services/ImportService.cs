using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 数据导入服务实现
    /// </summary>
    public class ImportService : IImportService
    {
        private readonly ILexiconRepository _lexiconRepository;
        private readonly ICourseContentRepository _courseContentRepository;
        private readonly ILogger<ImportService> _logger;

        public ImportService(
            ILexiconRepository lexiconRepository,
            ICourseContentRepository courseContentRepository,
            ILogger<ImportService> logger)
        {
            _lexiconRepository = lexiconRepository;
            _courseContentRepository = courseContentRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> ImportFromExcelAsync(int userId, int courseId, Stream fileStream, string fileName)
        {
            // 读取 Excel 文件获取单词列表
            var words = ReadWordsFromStream(fileStream);
            if (words.Count == 0) return 0;

            var insertedCount = 0;

            foreach (var (en, cn) in words)
            {
                // 查找单词是否存在
                var lexicon = await _lexiconRepository.GetByEnAsync(en);
                int lexiconId;

                if (lexicon == null)
                {
                    // 插入新单词
                    lexiconId = await _lexiconRepository.CreateAsync(new Lexicon
                    {
                        En = en,
                        Cn = cn,
                        UserId = userId
                    });
                }
                else
                {
                    lexiconId = lexicon.Id;
                }

                // 检查课程内容是否已关联
                var existing = await _courseContentRepository.GetByCourseAndLexiconAsync(courseId, lexiconId);
                if (existing == null)
                {
                    await _courseContentRepository.CreateAsync(new CourseContent
                    {
                        CourseId = courseId,
                        LexiconId = lexiconId,
                        CreateDate = DateTime.Now
                    });
                    insertedCount++;
                }
            }

            return insertedCount;
        }

        /// <summary>
        /// 从流中读取单词列表（简单文本格式：每行一个单词，可用 Tab/逗号 分隔英文和中文）
        /// </summary>
        private static List<(string en, string cn)> ReadWordsFromStream(Stream stream)
        {
            var result = new List<(string en, string cn)>();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 支持 Tab 或逗号分隔
                var parts = line.Split(new[] { '\t', ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
                var en = parts[0].Trim();
                var cn = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                if (!string.IsNullOrWhiteSpace(en))
                {
                    result.Add((en, cn));
                }
            }

            return result;
        }
    }
}
