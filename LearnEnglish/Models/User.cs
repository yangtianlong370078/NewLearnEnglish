using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Models
{
    public class User
    {
        public int id { get; set; }
        public int age { get; set; }
        public string loginid { get; set; }

        public string phone { get; set; }
        public string name { get; set; }

        public string password { get; set; }

        public int courseId { get; set; }
        public int status { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }

       
    }

    
}
