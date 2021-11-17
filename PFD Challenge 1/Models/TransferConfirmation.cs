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
        [Required(ErrorMessage = "Recipient cannot be empty.")]
        public string Recipient { get; set; }

        [Display(Name = "Bank Account")]
        [Required]
        public string BankAccount { get; set; }

        [Display(Name = "Transfer Amount")]
        [Required(ErrorMessage = "You must enter an amount to be sent.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$")] //Validation for maximum 2 decimal places
        [Range(1, 9999999999999999.99, ErrorMessage = "Please enter a value bigger than 0")] //Validation for transfer amount more than 0
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
