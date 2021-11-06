using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PFD_Challenge_1.Controllers
{
    public class FundTransferController : Controller
    {
        BankAccountDAL bankAccountContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        FutureTransferDAL futureTransferContext = new FutureTransferDAL();
        TransactionDAL transactionDAL = new TransactionDAL();

        /*
        public ActionResult Index()
        {
            if ((HttpContext.Session.GetString("Account") == null) ||
            (HttpContext.Session.GetString("Account") != "User"))
            {
                return RedirectToAction("Index", "Home");
            }
            List<BankUser> bUList = bankUserContext.GetAllBankUser();
            return View(bUList);
        }
        */

        private List<BankAccount> GetAllBankAccount()
        {
            // Get a list of competition from database
            List<BankAccount> bankAccountList = bankAccountContext.GetAllBankAccount();
            // Adding a select prompt at the first row of the competition list
            bankAccountList.Insert(0, new BankAccount
            {
                Balance = 0,
            });
            return bankAccountList;
        }

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
