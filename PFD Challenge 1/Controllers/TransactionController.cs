using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Controllers
{
    //Controller to manage the display of all the transaction history.
    // Controller to manage the display of all the future transaction.
    public class TransactionController : Controller
    {
        BankAccountDAL bankAccountContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        FutureTransferDAL futureTransferContext = new FutureTransferDAL();
        TransactionDAL transactionContext = new TransactionDAL();
        public IActionResult Index()
        {
            BankAccount ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            List<Transaction> tList = transactionContext.GetAllTransaction(ba.AccNo);
            List<TransactionHistory> thList = new List<TransactionHistory>();
            //This is for the display of transaction history of user
            foreach(Transaction t in tList)
            {
                if (t.Sender == ba.AccNo)
                {
                    thList.Add(new TransactionHistory
                    {
                        Name = bankUserContext.GetBankUser(bankAccountContext.GetBankAccount(t.Recipient).Nric).Name,
                        Amount = t.Amount,
                        TimeTransfer = t.TimeTransfer,
                        sender = true,
                    });
                }
                else
                {
                    thList.Add(new TransactionHistory
                    {
                        Name = bankUserContext.GetBankUser(bankAccountContext.GetBankAccount(t.Sender).Nric).Name,
                        Amount = t.Amount,
                        TimeTransfer = t.TimeTransfer,
                        sender = false,
                    });
                }
            }
            return View(thList);
        }
    }
}
