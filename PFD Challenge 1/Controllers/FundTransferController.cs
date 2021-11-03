using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Controllers
{
    public class FundTransferController : Controller
    {
        public IActionResult FundTransfer()
        {
            return View();
        }
        public IActionResult FundTransferReview()
        {
            return View();
        }
    }
}
