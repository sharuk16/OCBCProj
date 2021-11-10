using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class FundTransferReview
    {
        public string Recipient { get; set; }
        public decimal Balance { get; set; }
        public decimal TransferAmount { get; set; }
        public bool futureTransfer { get; set; }
        public DateTime? TimeTransfer { get; set; }
    }
}
