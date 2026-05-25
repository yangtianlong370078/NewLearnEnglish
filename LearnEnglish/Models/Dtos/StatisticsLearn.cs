namespace LearnEnglish.Models.Dtos
{

    public class StatisticsLearnGroup
    {
        public DateTime date { get; set; }
        public int year => date.Year;
        public int month => date.Month;
        public List<StatisticsLearn> statisticsLearns { get; set; }=new List<StatisticsLearn>();
        public int totalcount { get; set; }
        public LearnTask task { get; set; } = new LearnTask();
    }

    public record StatisticsLearn
    {
        public DateTime date { get; set; }
        public int count { get; set; }
        public int Year => date.Year;

        public int Month => date.Month;
        public int Day => date.Day;
    }


    public class LearnTask
    {
        public int id { get; set; }
        public int userid { get; set; }
        public DateTime startdate { get; set; }
        public int count { get; set; }

        public int weekend { get; set; }
        public int Year => startdate.Year;
        public int Month => startdate.Month;
    }
}
