namespace LearnEnglish.Models.Dtos
{
    public class ExamOutDto 
    {
        public int id { get; set; }
        public string name { get; set; }
        public int userid { get; set; }
        public double count { get; set; }
        public DateTime createdate { get; set; }
        public examnswerCount TxCount { get; set; } = new examnswerCount();
        public examnswerCount YzCount { get; set; } = new examnswerCount();
        public examnswerCount ZyCount { get; set; } = new examnswerCount();

        public double TxScore => (TxCount.count / count) * 100;
        public double YzScore => (YzCount.count / count) * 100;
        public double ZyScore => (ZyCount.count / count) * 100;
    }
    public class examnswerCount
    {
        public int examid { get; set; }
        public bool isyk { get; set; } = false;
        public int type { get; set; }
        public double count { get; set; }
    }
}
