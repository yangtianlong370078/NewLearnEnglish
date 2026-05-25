namespace LearnEnglish.Models
{
    public class UserErrorViewModel
    {
        public string? RequestId { get; set; }

        public string Msg { get; set; }

        public int Type { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
