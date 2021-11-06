using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class FundTransferVM
    {
        public BankAccount account { get; set; }
        public Transaction transactions { get; set; }
    }
}
