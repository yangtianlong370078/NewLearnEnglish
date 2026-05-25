namespace LearnEnglish.Application.Dtos.Exam
{
    /// <summary>
    /// 考卷列表输出DTO
    /// </summary>
    public class ExamOutDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public double Count { get; set; }
        public DateTime CreateDate { get; set; }
        public ExamAnswerCountDto TxCount { get; set; } = new();
        public ExamAnswerCountDto YzCount { get; set; } = new();
        public ExamAnswerCountDto ZyCount { get; set; } = new();

        public double TxScore => Count > 0 ? (TxCount.Count / Count) * 100 : 0;
        public double YzScore => Count > 0 ? (YzCount.Count / Count) * 100 : 0;
        public double ZyScore => Count > 0 ? (ZyCount.Count / Count) * 100 : 0;
    }

    /// <summary>
    /// 考试答题统计
    /// </summary>
    public class ExamAnswerCountDto
    {
        public int ExamId { get; set; }
        public bool IsCompleted { get; set; }
        public int Type { get; set; }
        public double Count { get; set; }
    }

    /// <summary>
    /// 考试内容输出DTO
    /// </summary>
    public class ExamContentOutDto
    {
        public int Id { get; set; }
        public int LearnId { get; set; }
        public int IsEnAudio { get; set; }
        public int IsUsAudio { get; set; }
        public int LexiconId { get; set; }
        public string En { get; set; } = string.Empty;
        public string Cn { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public int Type { get; set; }
        public bool? IsOk { get; set; }
        public string Answer { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// 考试内容查询结果（Repository 层返回的原始数据）
    /// </summary>
    public class ExamContentQueryDto
    {
        public int Id { get; set; }
        public int LearnId { get; set; }
        public int IsEnAudio { get; set; }
        public int IsUsAudio { get; set; }
        public int LexiconId { get; set; }
        public string En { get; set; } = string.Empty;
        public string Cn { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public int? Type { get; set; }
        public bool? IsOk { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}
