using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace PFD_Challenge_1.Controllers
{
    public class FundTransferController : Controller
    {
        BankAccountDAL bankAccountContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        FutureTransferDAL futureTransferContext = new FutureTransferDAL();
        TransactionDAL transactionDAL = new TransactionDAL();
        public IActionResult FundTransfer()
        {
            //if ((HttpContext.Session.GetString("Account") == null) ||
            //(HttpContext.Session.GetString("Account") != "User"))
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            //ADD in a hard-coded NRIC

            return View();
        }
        public IActionResult FundTransferReview()
        {
            return View();
        }
    }
}
