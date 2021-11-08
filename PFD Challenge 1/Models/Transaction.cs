using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PFD_Challenge_1.Models
{
    public class Transaction
    {
        [Required]
        [Display(Name = "ID")]
        public int TransacID { get; set; }

        [Required(ErrorMessage = "You must enter the name of the recipient.")]
        [Display(Name = "Recipient")]
        [StringLength(11, ErrorMessage = "Invalid Account Number")]
        public string Recipient { get; set; }

        [Required]
        [Display(Name = "Sender")]
        [StringLength(11, ErrorMessage = "Invalid Account Number")]
        public string Sender { get; set; }

        [Required(ErrorMessage = "You must enter an amount to be sent.")]
        [Display(Name = "Amount Sent")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$")] //Validation for maximum 2 decimal places
        [Range(1, 9999999999999999.99, ErrorMessage = "Please enter a value bigger than 0")] //Validation for transfer amount more than 0
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}")]
        public DateTime TimeTransfer { get; set; }

        [Required]
        public string Notified { get; set; }

        [Required]
        public string Completed { get; set; }

        [Required]
        [ValidateTransactionType]
        public string Type { get; set; }
    }
}
