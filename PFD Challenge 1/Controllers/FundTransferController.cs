using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace PFD_Challenge_1.Controllers
{
    public class FundTransferController : Controller
    {
        BankAccountDAL bankAccountContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        FutureTransferDAL futureTransferContext = new FutureTransferDAL();
        TransactionDAL transactionContext = new TransactionDAL();
        public IActionResult FundTransfer()
        {
            //if ((HttpContext.Session.GetString("Account") == null) ||
            //(HttpContext.Session.GetString("Account") != "User"))
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            
            HttpContext.Session.SetString("NRIC", "T1234567A");
            BankAccount ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            if (ba == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Transaction t = transactionContext.GetLatestTransaction(ba.AccNo);
            FundTransferVM ft = new FundTransferVM
            {
                account = ba,
                transactions = t,
            };
            return View(ft);
        }
        public IActionResult FundTransferReview()
        {
            return View();
        }
        [HttpPost]
        public IActionResult FundTransferReview(FutureTransferViewModel ftvm)
        {
            //Regex rgx = new Regex(@"[A-Za-z0-9._%+-]+@lcu.edu.sg");
            //if (rgx.IsMatch(ftvm.recipient))
            //{
            //    //BankAccount ba = bankAccountContext.GetBankAccountEmail(HttpContext.Session.GetString("NRIC"));
            //}
            //BankAccount ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            //BankAccount ba = bankAccountContext.GetBankAccountPH(HttpContext.Session.GetString("NRIC"));
            //if (ftvm.recipient)
            return View();
        }
    }
}
