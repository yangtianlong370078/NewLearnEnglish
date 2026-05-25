namespace LearnEnglish.Application.Dtos.Statistics
{
    /// <summary>
    /// 学习统计分组（按月）
    /// </summary>
    public class StatisticsLearnGroupDto
    {
        public DateTime Date { get; set; }
        public int Year => Date.Year;
        public int Month => Date.Month;
        public List<StatisticsLearnDto> StatisticsLearns { get; set; } = new();
        public int TotalCount { get; set; }
        public LearnTaskDto Task { get; set; } = new();
    }

    /// <summary>
    /// 每日学习统计
    /// </summary>
    public record StatisticsLearnDto
    {
        public DateTime Date { get; init; }
        public int Count { get; init; }
        public int Year => Date.Year;
        public int Month => Date.Month;
        public int Day => Date.Day;
    }

    /// <summary>
    /// 学习任务DTO
    /// </summary>
    public class LearnTaskDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public int Count { get; set; }
        public int Weekend { get; set; }
        public int Year => StartDate.Year;
        public int Month => StartDate.Month;
    }
}
