using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnEnglish.Redis
{
    public class RedisOption
    {
        public string Connection { get; set; }
        public string InstanceName { get; set; }
        public int DefaultDb { get; set; }
    }
}
