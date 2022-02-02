using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class TempTransac
    {
        public string _id { get; set; }
        public string Checkpoint4 { get; set; }
        public string Checkpoint3 { get; set; }
        public string Checkpoint2 { get; set; }
        public string Nric { get; set; }
        public string Checkpoint1 { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeTransfer { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public int TransacID { get; set; }
    }
}
