using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class FutureTransfer
    {
        [Display(Name = "Future ID")]
        public int FutureId { get; set; }

        [Display(Name = "Recipient")]
        public string Recipient { get; set; }

        [Display(Name = "Sender")]
        public string Sender { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        //Singapore Time Zone
        [Display(Name = "Plan Time")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}")]
        public DateTime PlanTime { get; set; }

        [Display(Name = "Notified")]
        public string Notified { get; set; }

        [Display(Name = "Completed")]
        public string Completed { get; set; }
    }
}
