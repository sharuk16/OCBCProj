using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class FutureTransferViewModel
    {
        public List<FutureTransfer> futureTransferList { get; set; }
        public List<BankAccount> bankAccountList { get; set; }
        public List<BankUser> bankUserList { get; set; }

        public FutureTransferViewModel()
        {
            futureTransferList = new List<FutureTransfer>();
            bankAccountList = new List<BankAccount>();
            bankUserList = new List<BankUser>();
        }
    }
}
