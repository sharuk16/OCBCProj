using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PFD_Challenge_1.Models
{
    public class ValidateTransactionType : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value.ToString() == "Immediate" || value.ToString() == "Future")
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Invalid Payment Type.");
        }
    }
}
