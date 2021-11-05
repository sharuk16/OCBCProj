using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class BankAccount
    {
        [Display(Name = "Account No")]
        public string AccNo { get; set; }

        [Display(Name = "Balance")]
        public double Balance { get; set; }

        [Display(Name = "NRIC")]
        public int Nric { get; set; }
    }
}
