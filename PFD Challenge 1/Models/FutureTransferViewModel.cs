﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class FutureTransferViewModel
    {
        public BankAccount account { get; set; }
        public string recipient { get; set; }
    }
}
