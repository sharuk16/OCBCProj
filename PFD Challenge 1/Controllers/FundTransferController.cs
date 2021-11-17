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
using PFD_Challenge_1.TelegramModel;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

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
            if (HttpContext.Session.GetString("NRIC") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Regex bankacc = new Regex(@"[0-9]{3}-[0-9]{6}-[0-9]{3}");
            BankUser bu;
            BankAccount ba;
            BankAccount senderAccount = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            BankUser sender = bankUserContext.GetBankUser(HttpContext.Session.GetString("NRIC"));
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
            //Validation
            //Check if recipient exists
            if(bu == null || ba == null)
            {
                if(bu == null)
                {
                    ViewData["Message"] = "Bank user does not exists!";
                }
                else
                {
                    ViewData["Message"] = "Bank account does not exists!";
                }
                return View(ftr);
            }
            if(transactionContext.ValidateTransactionLimit(senderAccount, ftr.TransferAmount) == false)
            {
                ViewData["Message"] = "Daily transfer limit reached!";
                return View(ftr);
            }
            // Check if Time of Transfer is not the day before
            if (ftr.TimeTransfer <= DateTime.Now)
            {
                ViewData["Message"] = "Future Transfer accepts date from tomorrow onwards!";
                return View(ftr);
            }
            // Check if recipient is sender
            if(bu.Nric == HttpContext.Session.GetString("NRIC"))
            {
                ViewData["Message"] = "You are not allowed to transfer to yourself";
                return View(ftr);
            }
            // Check if fundtransfer can be sent
            if(ftr.TransferAmount <=0 || ftr.TransferAmount > senderAccount.Balance)
            {
                if (ftr.TransferAmount <= 0)
                {
                    ViewData["Message"] = "Unable to transfer 0 dollars or less than 0 dollars!";
                }
                else
                {
                    ViewData["Message"] = "Unable to transfer more than balance in account!";
                }
                
                return View(ftr);
            }
            //Check if FutureTransfer value is true and if time of transfer is empty.
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

        public IActionResult Confirmation(string recipient, string bankAccount, decimal? transferAmount, string futureTransfer, DateTime? timeTransfer)
        {
            if (recipient == null || recipient == "" || bankAccount == null || bankAccount == "" || transferAmount == null || futureTransfer == null || futureTransfer == "")
            {
                return RedirectToAction("Index", "Home");
            }
            if (futureTransfer == "true" && timeTransfer == null)
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
        public async Task<IActionResult> ConfirmationAsync(TransferConfirmation tc)
        {
            string message="";
            int transacID = -1;
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
            try
            {
                if (tc.FutureTransfer == "true")
                {
                    if (tc.TimeTransfer == DateTime.Now)
                    {
                        //I think this requires Quartz?
                    }
                }
                else
                {
                    if (transactionContext.ValidateTransactionLimit(senderAccount, transferAmount) //If the amount exceeds transaction limit
                        == false)
                    {
                        TempData["LimitExceed"] = "The transaction you are trying to make exceeds your daily limit." +
                            "Change your daily transaction limit or make a smaller transaction.";
                        return RedirectToAction("Index", "Home");
                    }
                    else if (transactionContext.ValidateTransactionLimit(senderAccount, transferAmount) //If the amount does not exceed the transaction limit
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
                            transacID = transactionContext.AddTransactionRecord(newTransac); //Add transaction object to database
                            bool updatedAccounts = transactionContext.UpdateTransactionChanges(receiverAccount, senderAccount, transferAmount); //Updates bank account balance records
                            if (updatedAccounts == true) //If balance updates successfully
                            {
                                transactionContext.UpdateTransactionComplete(transacID); //Updates transaction "Completed" status
                                message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                            }
                            else
                            {
                                transactionContext.ReverseTransactionChanges(receiverAccount, senderAccount, transferAmount); //Reverses update of bank account balance records
                                message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                            }
                        }
                    }
                if (bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")) != null)
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://api.telegram.org");
                    int chatID = bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")).Value;
                    BankUser bu = bankUserContext.GetBankUser(HttpContext.Session.GetString("NRIC"));
                    BankUser su = bankUserContext.GetBankUser(receiverAccount.Nric);
                    string data = "Dear " + bu.Name + "! Your funds transfer of $" + transferAmount.ToString() + " to " + su.Name + " is successful! Date: " + DateTime.Now;
                    Notification newNotification = new Notification
                    {
                        chat_id = chatID,
                        text = data,
                    };
                    string json = JsonConvert.SerializeObject(newNotification);
                    StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
                    if (response.IsSuccessStatusCode)
                    {
                        transactionContext.UpdateTransactionNotified(transacID);
                    }
                }
                ViewData["Message"] = message;
                return RedirectToAction("Index", "Transaction");
            }
            catch (TimeoutException)
            {
                ViewData["TimeoutMessage"] = "The website has taken too long to process your request and has timed out. Your transaction has not gone through.";
                return View();
            }
        }
    }
}
