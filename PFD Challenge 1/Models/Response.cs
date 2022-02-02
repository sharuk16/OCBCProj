using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class Response
    {
        public bool success { get; set; }
        public string userId { get; set; }
        public DateTime timestamp { get; set; }
        public string rpid { get; set; }
        public string origin { get; set; }
        public string device { get; set; }
        public string country { get; set; }
        public object nickname { get; set; }
        public DateTime expiresAt { get; set; }
    }
}
