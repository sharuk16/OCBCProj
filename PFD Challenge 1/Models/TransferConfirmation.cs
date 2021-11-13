using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class TransferConfirmation
    {
        [Display(Name = "Recipient")]
        [Required]
        public string Recipient { get; set; }
        [Display(Name = "Bank Account")]
        [Required]
        public string BankAccount { get; set; }
        [Display(Name = "Transfer Amount")]
        [Required]
        public decimal TransferAmount { get; set; }
        [Display(Name = "Future Transfer")]
        [Required]
        public string FutureTransfer { get; set; }
        [Display(Name = "Planned time of transfer")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}")]
        public DateTime? TimeTransfer { get; set; }
    }
}
