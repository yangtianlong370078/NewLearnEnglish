namespace LearnEnglish.Models.Dtos
{
    public class ShowTranslateDto
    {
        public int id { get; set; }

        public int userId { get; set; }
        public int coursecontentId { get; set; }
        public int lexiconId { get; set; }

        public int iscollect { get; set; }
        public int numbersum { get; set; }
        public int zynumber { get; set; }
        public int yznumber { get; set; }
        public int txnumber { get; set; }
        public int fynumber { get; set; }

        public string en { get; set; }

        public string cn { get; set; }

        public string name { get; set; }

        public string value { get; set; }

        public int zt { get; set; }

        public int isenaudio { get; set; }

        public int isusaudio { get; set; }

        public bool isUpdate { get; set; }
    }
}
