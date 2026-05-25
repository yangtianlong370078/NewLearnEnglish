namespace LearnEnglish.Models.Dtos
{
    public class ExamContentOutDto
    {
        public int id { get; set; }
        public int learnid { get; set; }
        public int isenaudio { get; set; }
        public int isusaudio { get; set; }
        public int lexiconid { get; set; }
        public string en { get; set; }
        public string cn { get; set; }
        public int examid { get; set; }
        public int type { get; set; }
        public bool? isok { get; set; }
        public string answer { get; set; }
        public string name { get; set; }
        public string value { get; set; }

    }
}
