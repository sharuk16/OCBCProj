using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class TransactionHistory
    {
        [Required]
        [Display(Name = "Name:")]
        public string Name { get; set; }

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
        public bool sender { get; set; }
    }
}
