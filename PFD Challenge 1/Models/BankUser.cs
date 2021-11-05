using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class BankUser
    {
        [Display(Name = "NRIC")]
        public int Nric { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public int Phone { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Transaction Limit")]
        public double TransLimit { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
