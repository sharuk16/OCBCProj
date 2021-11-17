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
            HttpContext.Session.SetString("NRIC", "T1234567A");
            if (bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")) != null)
            {
                HttpContext.Session.SetString("TelegramChatID", "true");
            }
            else
            {
                HttpContext.Session.SetString("TelegramChatID", "false");
            }
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
            //Get bank Account details
            BankAccount ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            if (ba == null)
            {
                return RedirectToAction("Index", "Home");
            }
            FundTransferReview ftr = new FundTransferReview
            {
                Balance = ba.Balance,
                FutureTransfer = "false",
            };
            return View(ftr);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FundTransferReview(FundTransferReview ftr)
        {
            if (!ModelState.IsValid)
            {
                return View(ftr);
            }
            if(HttpContext.Session.GetString("NRIC")== null)
            {
                return RedirectToAction("Index", "Home");
            }
            Regex bankacc = new Regex(@"[0-9]{3}-[0-9]{6}-[0-9]{3}");
            BankUser bu;
            BankAccount ba;
            if (bankacc.IsMatch(ftr.Recipient))
            {
                ba = bankAccountContext.GetBankAccount(ftr.Recipient);
                bu = bankUserContext.GetBankUser(ba.Nric);
            }
            else
            {
                bu = bankUserContext.GetBankUser(ftr.Recipient);
                ba = bankAccountContext.GetBankAccount(bu.Nric);
            }
            if(bu == null || ba == null)
            {
                return View(ftr);
            }
            if(ftr.TimeTransfer < DateTime.Now)
            {
                return View(ftr);
            }
            if(ftr.FutureTransfer == "true" && ftr.TimeTransfer == null)
            {
                ftr.FutureTransfer = "false";
            }
            TransferConfirmation tc = new TransferConfirmation
            {
                Recipient = bu.Name,
                BankAccount = ba.AccNo,
                TransferAmount = ftr.TransferAmount,
                FutureTransfer = ftr.FutureTransfer,
                TimeTransfer = ftr.TimeTransfer,
            };
            return RedirectToAction("Confirmation", "FundTransfer", tc);
        }

        public IActionResult Confirmation(string? recipient, string? bankAccount, decimal? transferAmount, string? futureTransfer, DateTime? timeTransfer)
        {
            if (recipient == null || recipient == "" || bankAccount == null || bankAccount == "" || transferAmount == null || futureTransfer == null || futureTransfer == "")
            {
                return RedirectToAction("Index", "Home");
            }
            if(futureTransfer == "true" && timeTransfer == null)
            {
                return RedirectToAction("Index", "Home");
            }
            TransferConfirmation tc = new TransferConfirmation
            {
                Recipient = recipient,
                BankAccount = bankAccount,
                TransferAmount = transferAmount.Value,
                FutureTransfer = futureTransfer,
                TimeTransfer = timeTransfer,
            };
            return View(tc);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmation(TransferConfirmation tc)
        {
            if (!ModelState.IsValid)
            {
                return View(tc);
            }
            if (HttpContext.Session.GetString("NRIC") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            BankAccount senderAccount = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            BankAccount receiverAccount = bankAccountContext.GetBankAccount(tc.BankAccount);
            decimal transferAmount = tc.TransferAmount;
            if(tc.FutureTransfer == "true")
            {
                if(tc.TimeTransfer == DateTime.Now)
                {
                    //I think this requires Quartz?
                }
            }
            else
            {
                if(transactionContext.ValidateTransactionLimit(senderAccount, transferAmount) //If the amount exceeds transaction limit
                    ==false)
                {
                    return RedirectToAction("Index", "Home");
                }
                else if(transactionContext.ValidateTransactionLimit(senderAccount, transferAmount) //If the amount does not exceed the transaction limit
                    == true)
                {
                        Transaction newTransac = new Transaction //Create new transaction object
                        {
                            Recipient = receiverAccount.AccNo,
                            Sender = senderAccount.AccNo,
                            Amount = tc.TransferAmount,
                            TimeTransfer = DateTime.Now,
                            Type = "Immediate"
                        };
                        int transacID = transactionContext.AddTransactionRecord(newTransac); //Add transaction object to database
                        bool updatedAccounts = transactionContext.UpdateTransactionChanges(receiverAccount, senderAccount, transferAmount); //Updates bank account balance records
                        if(updatedAccounts == true) //If balance updates successfully
                        {
                            transactionContext.UpdateTransactionComplete(transacID); //Updates transaction "Completed" status
                            string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            transactionContext.ReverseTransactionChanges(receiverAccount, senderAccount, transferAmount); //Reverses update of bank account balance records
                            string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            return View(tc);
        }
    }
}
