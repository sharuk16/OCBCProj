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
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}")]
        public DateTime PlannedTime { get; set; }

        [Required]
        public bool Notified { get; set; }

        [Required]
        public bool Completed { get; set; }

        [Required]
        [ValidateTransactionType]
        public string Type { get; set; }
    }
}
