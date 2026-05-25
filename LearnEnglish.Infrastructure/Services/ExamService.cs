using LearnEnglish.Application.Dtos.Exam;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 考试服务实现
    /// </summary>
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IExamDetailRepository _examDetailRepository;
        private readonly IExamAnswerRepository _examAnswerRepository;
        private readonly IExamRecordRepository _examRecordRepository;
        private readonly IMyLexiconRepository _myLexiconRepository;
        private readonly ILogger<ExamService> _logger;

        public ExamService(
            IExamRepository examRepository,
            IExamDetailRepository examDetailRepository,
            IExamAnswerRepository examAnswerRepository,
            IExamRecordRepository examRecordRepository,
            IMyLexiconRepository myLexiconRepository,
            ILogger<ExamService> logger)
        {
            _examRepository = examRepository;
            _examDetailRepository = examDetailRepository;
            _examAnswerRepository = examAnswerRepository;
            _examRecordRepository = examRecordRepository;
            _myLexiconRepository = myLexiconRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<(bool success, string message, int wordCount)> CreateExamAsync(int userId, int examCount, int limitTime)
        {
            // 从已掌握的单词中选取今日未考过的单词
            var details = (await _examDetailRepository.SelectWordsForExamAsync(userId, examCount)).ToList();

            if (details.Count < 5)
            {
                return (false, "熟练单词小于5个，不能生成试题", 0);
            }

            // 创建考卷
            var exam = new Exam
            {
                Name = $"{DateTime.Now.Date:yyyy-MM-dd}试题",
                UserId = userId,
                Count = examCount,
                LimitTime = limitTime,
                CreateDate = DateTime.Now
            };
            var examId = await _examRepository.CreateAsync(exam);

            // 批量插入考试详情
            foreach (var d in details)
            {
                d.ExamId = examId;
            }
            await _examDetailRepository.BulkInsertAsync(details);

            // 实际单词数可能少于请求数，更新考卷记录
            if (details.Count != examCount)
            {
                await _examRepository.UpdateCountAsync(examId, details.Count);
            }

            return (true, $"创建试题成功，试题单词{details.Count}个", details.Count);
        }

        /// <inheritdoc/>
        public async Task<ExamOutDto?> GetExamDetailAsync(int examId)
        {
            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null) return null;

            var records = (await _examRecordRepository.GetByExamIdAsync(examId)).ToList();
            var answerCounts = await _examAnswerRepository.CountByExamIdGroupByTypeAsync(examId);
            return BuildExamOutDtoFromRecords(exam, records, answerCounts);
        }

        /// <inheritdoc/>
        public async Task<(List<ExamOutDto> list, int total)> GetExamListAsync(int userId, string? name, int pageIndex, int pageSize)
        {
            var total = await _examRepository.SearchCountByUserIdAsync(userId, name);
            var exams = (await _examRepository.SearchPagedByUserIdAsync(userId, name, pageIndex, pageSize)).ToList();

            if (!exams.Any())
            {
                return (new List<ExamOutDto>(), total);
            }

            var result = new List<ExamOutDto>();
            foreach (var exam in exams)
            {
                var records = (await _examRecordRepository.GetByExamIdAsync(exam.Id)).ToList();
                var answerCounts = await _examAnswerRepository.CountByExamIdGroupByTypeAsync(exam.Id);
                result.Add(BuildExamOutDtoFromRecords(exam, records, answerCounts));
            }

            return (result, total);
        }

        /// <inheritdoc/>
        public async Task<(int id, string name, int limitTime)?> GetExamInfoAsync(int userId, int examId)
        {
            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null || exam.UserId != userId) return null;
            return (exam.Id, exam.Name, exam.LimitTime);
        }

        /// <inheritdoc/>
        public async Task<(List<ExamContentOutDto> items, int? limitTime, int? score)> GetExamContentListAsync(int userId, int examId, int type)
        {
            // 查询考试内容（联表查询）
            var queryItems = (await _examDetailRepository.GetExamContentWithDetailsAsync(userId, examId, type)).ToList();

            // 查询该类型的考试记录（判断是否已提交答案）
            var record = await _examRecordRepository.GetByExamIdAndTypeAsync(examId, type);
            int? limitTime = record?.LimitTime;
            int? score = record?.Score;

            // 转换为输出 DTO
            var items = queryItems.Select(q => new ExamContentOutDto
            {
                Id = q.Id,
                LearnId = q.LearnId,
                IsEnAudio = q.IsEnAudio,
                IsUsAudio = q.IsUsAudio,
                LexiconId = q.LexiconId,
                En = q.En,
                Cn = q.Cn,
                ExamId = q.ExamId,
                Type = type,
                IsOk = q.IsOk,
                Answer = limitTime != null && score != null ? (q.Answer ?? "") : "",
                Name = type == 1 || type == 3 ? q.Cn : q.En,
                Value = type == 1 || type == 3 ? q.En : q.Cn
            }).ToList();

            return (items, limitTime, score);
        }

        /// <inheritdoc/>
        public async Task<bool> SubmitExamAnswersAsync(int userId, string data, int examId, int type, int score)
        {
            var items = JsonConvert.DeserializeObject<List<ExamAnswerSubmitDto>>(data);
            if (items == null || !items.Any()) return false;

            // 构建答案实体
            var answers = items.Select(item => new ExamAnswer
            {
                ExamId = examId,
                ExamDetailId = item.ExamDetailId,
                Type = type,
                IsOk = false,
                Answer = item.Answer ?? ""
            }).ToList();

            // 清除旧答案和记录
            await _examAnswerRepository.DeleteByExamIdAndTypeAsync(examId, type);
            await _examRecordRepository.DeleteByExamIdAndTypeAsync(examId, type);

            // 批量插入新答案
            await _examAnswerRepository.BulkInsertAsync(answers);

            // 插入考试记录
            await _examRecordRepository.CreateAsync(new ExamRecord
            {
                ExamId = examId,
                Type = type,
                LimitTime = 5,
                Score = score,
                CreateTime = DateTime.Now
            });

            // 更新错题的熟练度（重置对应维度次数）
            var numberField = type switch
            {
                1 => "zynumber",
                2 => "yznumber",
                3 => "txnumber",
                4 => "fynumber",
                _ => "zynumber"
            };
            await _myLexiconRepository.ResetExamProficiencyAsync(examId, type, numberField);

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteExamAsync(int examId)
        {
            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null) return false;

            await _examAnswerRepository.DeleteByExamIdAsync(examId);
            await _examRecordRepository.DeleteByExamIdAsync(examId);
            await _examDetailRepository.DeleteByExamIdAsync(examId);
            await _examRepository.DeleteAsync(examId);

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ReExamAsync(int examId, int type)
        {
            // 清除指定类型的答案和记录
            await _examAnswerRepository.DeleteByExamIdAndTypeAsync(examId, type);
            await _examRecordRepository.DeleteByExamIdAndTypeAsync(examId, type);
            return true;
        }

        /// <summary>
        /// 从 examrecord 记录构建 ExamOutDto（含各维度答题统计）
        /// </summary>
        private static ExamOutDto BuildExamOutDtoFromRecords(Exam exam, List<ExamRecord> records, Dictionary<int, int> answerCounts)
        {
            var dto = new ExamOutDto
            {
                Id = exam.Id,
                Name = exam.Name,
                UserId = exam.UserId,
                Count = exam.Count,
                CreateDate = exam.CreateDate
            };

            foreach (var record in records)
            {
                var answerCount = answerCounts.GetValueOrDefault(record.Type, 0);
                var countDto = new ExamAnswerCountDto
                {
                    ExamId = exam.Id,
                    IsCompleted = true,
                    Type = record.Type,
                    Count = exam.Count - answerCount
                };

                switch (record.Type)
                {
                    case 1: dto.ZyCount = countDto; break;
                    case 2: dto.YzCount = countDto; break;
                    case 3: dto.TxCount = countDto; break;
                }
            }

            return dto;
        }
    }

    /// <summary>
    /// 考试答案提交 DTO（前端提交的 JSON 结构）
    /// </summary>
    public class ExamAnswerSubmitDto
    {
        [JsonProperty("examdetailid")]
        public int ExamDetailId { get; set; }

        [JsonProperty("isok")]
        public bool IsOk { get; set; }

        [JsonProperty("answer")]
        public string? Answer { get; set; }
    }
}
