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
using System.Net.Http;
using PFD_Challenge_1.TelegramModel;
using Newtonsoft.Json;
using System.Text;
using System.Net.Mail;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            if (HttpContext.Session.GetString("NRIC") == null)
            {
                return RedirectToAction("Index", "Home");
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
            BankUser bu = bankUserContext.GetBankUser(ba.Nric);

            ViewData["User"] = bu.Name;
            return View(ft);
        }
        public async Task<ActionResult> FundTransferReview()
        {
            BankAccount ba;
            FundTransferReview ftr;
            //Check if there are existing transactions left in the restdb
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://ocbcdatabase-0c55.restdb.io");
            client.DefaultRequestHeaders.Add("x-api-key", "61f2742d7e55272295017175");
            HttpResponseMessage response = await client.GetAsync("/rest/temptransac");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                if(data != null)
                {
                    List<TempTransac> tempTransacList = JsonConvert.DeserializeObject<List<TempTransac>>(data);
                    foreach(TempTransac tempTransac in tempTransacList)
                    {
                        if (HttpContext.Session.GetString("NRIC") == tempTransac.Nric)
                        {
                            //Get bank Account details
                            ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
                            if (ba == null)
                            {
                                return RedirectToAction("Index", "Home");
                            }
                            ftr = new FundTransferReview
                            {
                                Recipient = tempTransac.Recipient,
                                Balance = ba.Balance,
                                TransferAmount = tempTransac.Amount,
                                FutureTransfer = "false",
                            };
                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/"+tempTransac._id);
                            if (transactionContext.GetTransaction(tempTransac.TransacID) != null)
                            {
                                transactionContext.DeleteTransactionRecord(tempTransac.TransacID);
                            }
                            return View(ftr);
                        }
                        else
                        {
                            //Get bank Account details
                           ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
                            if (ba == null)
                            {
                                return RedirectToAction("Index", "Home");
                            }
                            ftr = new FundTransferReview
                            {
                                Balance = ba.Balance,
                                FutureTransfer = "false",
                            };
                            return View(ftr);
                        }
                    }
                }
            }
            //Get bank Account details
            ba = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            if (ba == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ftr = new FundTransferReview
            {
                Balance = ba.Balance,
                FutureTransfer = "false",
            };
            return View(ftr);
        }
        //Receiving the transaction information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FundTransferReview(FundTransferReview ftr)
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
            BankAccount senderAccount = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
            //Validation
            //Check if recipient exists
            if (bankacc.IsMatch(ftr.Recipient))
            {
                ba = bankAccountContext.GetBankAccount(ftr.Recipient);
                if (ba== null)
                {
                    ViewData["Message"] = "Bank account does not exists!";
                    return View(ftr);
                }
                bu = bankUserContext.GetBankUser(ba.Nric);
                if(bu == null)
                {
                    ViewData["Message"] = "Bank user does not exists!";
                }
            }
            else
            {
                bu = bankUserContext.GetBankUser(ftr.Recipient);
                if (bu != null)
                {
                    ba = bankAccountContext.GetBankAccount(bu.Nric);
                    if(ba == null)
                    {
                        ViewData["Message"] = "Bank account does not exists!";
                        return View(ftr);
                    }
                }
                else
                {
                    ViewData["Message"] = "Bank user does not exists!";
                    return View(ftr);
                }
                
            }
            
            if (transactionContext.ValidateTransactionLimit(senderAccount, ftr.TransferAmount) == false)
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
            if (bu.Nric == HttpContext.Session.GetString("NRIC"))
            {
                ViewData["Message"] = "You are not allowed to transfer to yourself";
                return View(ftr);
            }
            // Check if fundtransfer can be sent
            if (ftr.TransferAmount <= 0 || ftr.TransferAmount > senderAccount.Balance)
            {
                if (ftr.TransferAmount <= 0)
                {
                    ViewData["Message"] = "Unable to transfer zero dollars or less than zero dollars!";
                }
                else
                {
                    ViewData["Message"] = "Unable to transfer more than balance in account!";
                }

                return View(ftr);
            }
            //Check if FutureTransfer value is true and if time of transfer is empty.
            if (ftr.FutureTransfer == "true" && ftr.TimeTransfer == null)
            {
                ftr.FutureTransfer = "false";
            }
            
            TransferConfirmation tc;
            if (ftr.FutureTransfer == "true")
            {

                tc = new TransferConfirmation
                {
                    Recipient = bu.Name,
                    BankAccount = ba.AccNo,
                    TransferAmount = ftr.TransferAmount,
                    FutureTransfer = ftr.FutureTransfer,
                    TimeTransfer = ftr.TimeTransfer.Value.ToString(),
                };
                HttpContext.Session.SetInt32("transacID", 0);
            }
            else
            {
                // Create a record and post to Rest DB (done in JS)
                // Update Checkpoint 1
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://ocbcdatabase-0c55.restdb.io");
                client.DefaultRequestHeaders.Add("x-api-key", "61f2742d7e55272295017175");
                HttpResponseMessage response = await client.GetAsync("/rest/temptransac");
                HttpResponseMessage getResponse = await client.GetAsync("/rest/temptransac");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    if (data != null)
                    {
                        List<TempTransac> tempTransacList = JsonConvert.DeserializeObject<List<TempTransac>>(data);
                        foreach (TempTransac tempTransac in tempTransacList)
                        {
                            if (HttpContext.Session.GetString("NRIC") == tempTransac.Nric)
                            {
                                var checkpoint1Transac = JsonSerializer.Serialize(new
                                {
                                    _id = tempTransac._id,
                                    checkpoint1 = "True"
                                });
                                HttpContent updateCheckpoint1 = new StringContent(checkpoint1Transac, Encoding.UTF8, "application/json");
                                HttpResponseMessage updateResponse = await client.PutAsync("/rest/temptransac/" + tempTransac._id, updateCheckpoint1);
                                string updateData = await updateResponse.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
                
                //Insert Transaction Record
                Transaction transac = new Transaction
                {
                    Recipient = ba.AccNo,
                    Sender = senderAccount.AccNo,
                    Amount = ftr.TransferAmount,
                    TimeTransfer = DateTime.UtcNow,
                    Type = "Immediate"
                };

                int transacID = transactionContext.AddTransactionRecord(transac);
                HttpContext.Session.SetInt32("transacID", transacID);

                // Update Checkpoint 2
               if (getResponse.IsSuccessStatusCode)
                {
                    string data = await getResponse.Content.ReadAsStringAsync();
                    if (data != null)
                    {
                        List<TempTransac> tempTransacList = JsonConvert.DeserializeObject<List<TempTransac>>(data);
                        foreach (TempTransac tempTransac in tempTransacList)
                        {
                            if (HttpContext.Session.GetString("NRIC") == tempTransac.Nric)
                            {
                                var checkpoint2Transac = JsonSerializer.Serialize(new
                                {
                                    _id = tempTransac._id,
                                    transacID = transacID,
                                    checkpoint2 = "True"
                                });
                                HttpContent updateCheckpoint2 = new StringContent(checkpoint2Transac, Encoding.UTF8, "application/json");
                                HttpResponseMessage updateResponse = await client.PutAsync("/rest/temptransac/" + tempTransac._id, updateCheckpoint2);
                                string updateData = await updateResponse.Content.ReadAsStringAsync();
                                Console.WriteLine(JsonConvert.DeserializeObject<TempTransac>(updateData).Checkpoint2);
                                HttpContext.Session.SetString("RestID", tempTransac._id);
                            }
                        }
                    }
                }
               

                //
                tc = new TransferConfirmation
                {
                    Recipient = bu.Name,
                    BankAccount = ba.AccNo,
                    TransferAmount = ftr.TransferAmount,
                    FutureTransfer = ftr.FutureTransfer,
                    TimeTransfer = "",
                };
            }
            
            return RedirectToAction("Confirmation", tc);
        }

        public IActionResult Confirmation(string recipient, string bankAccount, decimal? transferAmount, string futureTransfer, string timeTransfer)
        {
            if (recipient == null || recipient == "" || bankAccount == null || bankAccount == "" || transferAmount == null || futureTransfer == null || futureTransfer == "")
            {
                return RedirectToAction("Index", "Home");
            }
            
            if (futureTransfer == "true"&& timeTransfer == "")
            {
                ViewData["Message"] = "For future transfer please state time.";
                return RedirectToAction("FundTransferReview");
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
            string message = "";
            
            if (!ModelState.IsValid)
            {
                return View(tc);
            }     
            if (tc.TimeTransfer == "" && tc.FutureTransfer == "true")
            {
                ViewData["Message"] = "For future transfer please state time.";
                return RedirectToAction("FundTransferReview");
            }
            if (HttpContext.Session.GetString("NRIC") == null || HttpContext.Session.GetInt32("transacID")==null)
            {
                return RedirectToAction("Index", "Home");
            }
            BankUser bu = bankUserContext.GetBankUser(HttpContext.Session.GetString("NRIC"));
            string data="";
            
            try
            {
                //Execute this portion if tranaction is a future transfer. This is to store the future transfer object
                if (tc.FutureTransfer == "true")
                {//Get relevant information from the confirmation page
                    BankAccount senderAccount = bankAccountContext.GetBankAccount(HttpContext.Session.GetString("NRIC"));
                    BankAccount receiverAccount = bankAccountContext.GetBankAccount(tc.BankAccount);
                    int transacID = -1;
                    
                    decimal transferAmount = tc.TransferAmount;
                    BankUser su = bankUserContext.GetBankUser(receiverAccount.Nric);
                    FutureTransfer newFutureTrans = new FutureTransfer
                    {
                        Recipient = receiverAccount.AccNo,
                        Sender = senderAccount.AccNo,
                        Amount = tc.TransferAmount,
                        PlanTime = Convert.ToDateTime(tc.TimeTransfer),
                    };
                    transacID = futureTransferContext.AddFutureRecord(newFutureTrans);
                    //Information to alert user
                    data = "Dear " + bu.Name + "! You have saved future funds transfer of $" + transferAmount.ToString() + " to " + su.Name + " is successful!";
                    //Method to send the transaction messages
                    await SendTelegramAsync(data, true, transacID);
                    return RedirectToAction("Success");
                }
                else
                {
                    // Load data from the records
                    int transacID = HttpContext.Session.GetInt32("transacID").Value;
                    Transaction savedTrans = transactionContext.GetTransaction(transacID);
                    BankAccount senderAccount = bankAccountContext.GetBankAccount(savedTrans.Sender);
                    BankAccount receiverAccount = bankAccountContext.GetBankAccount(savedTrans.Recipient);
                    decimal transferAmount = savedTrans.Amount;
                    BankUser ru = bankUserContext.GetBankUser(receiverAccount.Nric);

                    if (transactionContext.ValidateTransactionLimit(senderAccount, savedTrans.Amount) //If the amount exceeds transaction limit
                        == false)
                    {
                        //Notification message for user
                        data = "Dear " + bu.Name + "! Your funds transfer of $" + savedTrans.Amount.ToString() + " to " + ru.Name + " is Unsuccessful! Date of transfer: " + DateTime.Now +
                            " Reason for failed transaction: The transaction you are trying to make exceeds your daily limit. Change your daily transaction limit or make a smaller transaction.";
                        //Method to send the transaction messages
                        await SendTelegramAsync(data, true, transacID);
                        ViewData["Message"] = "Transaction limit reached";
                        //Redirect to user to transaction history
                        return RedirectToAction("Failure");
                    }
                    else if (transactionContext.ValidateTransactionLimit(senderAccount, transferAmount) //If the amount does not exceed the transaction limit
                        == true)
                    {
                        //Update Checkpoint 4: User Clicks on Confirm on Confirmation page
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://ocbcdatabase-0c55.restdb.io");
                        client.DefaultRequestHeaders.Add("x-api-key", "61f2742d7e55272295017175");
                        HttpResponseMessage getResponse = await client.GetAsync("/rest/temptransac");
                        if (getResponse.IsSuccessStatusCode)
                        {
                            string restData = await getResponse.Content.ReadAsStringAsync();
                            if (restData != null)
                            {
                                List<TempTransac> tempTransacList = JsonConvert.DeserializeObject<List<TempTransac>>(restData);
                                foreach (TempTransac tempTransac in tempTransacList)
                                {
                                    if (HttpContext.Session.GetString("NRIC") == tempTransac.Nric)
                                    {
                                        var checkpoint4Transac = JsonSerializer.Serialize(new
                                        {
                                            _id = tempTransac._id,
                                            checkpoint4 = "True"
                                        });
                                        HttpContent updateCheckpoint4 = new StringContent(checkpoint4Transac, Encoding.UTF8, "application/json");
                                        HttpResponseMessage updateResponse = await client.PutAsync("/rest/temptransac/" + tempTransac._id, updateCheckpoint4);
                                        string updateData = await updateResponse.Content.ReadAsStringAsync();
                                        Console.WriteLine(JsonConvert.DeserializeObject<TempTransac>(updateData).Checkpoint1);
                                    }
                                }
                            }
                        }
                        if (transferAmount <= senderAccount.Balance)
                        {
                            bool updatedAccounts = transactionContext.UpdateTransactionChanges(receiverAccount, senderAccount, transferAmount); //Updates bank account balance records
                            if (updatedAccounts == true) //If balance updates successfully
                            {
                                transactionContext.UpdateDailySpend(senderAccount.Nric, transferAmount);
                                //Updates Confirm column in database to "T", as user has confirmed the transfer
                                transactionContext.UpdateTransactionConfirm(transacID);
                                //transactionContext.UpdateTransactionComplete(); //Updates transaction "Completed" status, change transacID
                                message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                                data = "Dear " + bu.Name + "! You have successfully transfered $" + transferAmount.ToString() + " to " + ru.Name + "! Date and Time of Transfer" + DateTime.Now.ToString();
                                //Method to send the transaction messages
                                await SendTelegramAsync(data, true, transacID);
                                ViewData["Message"] = message;
                                transactionContext.UpdateTransactionComplete(transacID);
                                //Delete record from NoSQL database due to transaction complete.
                                HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + HttpContext.Session.GetString("RestID"));
                                //Redirect to user to transaction history
                                return RedirectToAction("Success");
                            }
                            else
                            {
                                message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                                data = "Dear " + bu.Name + "! You are unsuccessful in transfering $" + transferAmount.ToString() + " to " + ru.Name + "! Date and Time of attempted Transfer" + DateTime.Now.ToString();
                                //Method to send the transaction messages
                                await SendTelegramAsync(data, false, transacID);
                                ViewData["Message"] = "Balanced failed to update.";
                                //Delete record from NoSQL database if transaction fails due to unexpected issues.
                                HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + HttpContext.Session.GetString("RestID"));
                                //Redirect to user to transaction history
                                return RedirectToAction("Failure");
                            }
                        }
                        else
                        {
                            data = "Dear " + bu.Name + "! You do not have enough balance left in your account to complete this transaction. Please make a smaller transaction.";
                            //Method to send the transaction messages
                            await SendTelegramAsync(data, false, transacID);
                            ViewData["Message"] = "Insufficient Balance";
                            //Delete record from NoSQL database if transaction fails due to reasons other than connection issues.
                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + HttpContext.Session.GetString("RestID"));
                            //Redirect to user to transaction history
                            return RedirectToAction("Failure");
                        }
                    }

                }
                ViewData["Message"] = "Error";
                //Redirect to user to transaction history
                return RedirectToAction("Failure");
            }
            catch (TimeoutException)
            {//Method to catch if any calls from database takes too long to execute
                try
                {
                    if (bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")) != null)
                    {
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://api.telegram.org");
                        int chatID = bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")).Value;
                        Notification newNotification = new Notification
                        {
                            chat_id = chatID,
                            text = "The website has taken too long to process your request and has timed out. Your transaction has not gone through.",
                        };
                        string json = JsonConvert.SerializeObject(newNotification);
                        StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
                        if (response.IsSuccessStatusCode)
                        {
                            
                        }
                    }
                }catch(Exception e)
                {
                    return RedirectToAction("Index", "Transaction");
                }
                return RedirectToAction("Index", "Transaction");
            }
        }
        public IActionResult NotificationPage()
        {
            return View();
        }
        public async Task SendTelegramAsync(string s, bool status, int? transacID)
        {
            if (bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")) != null)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.telegram.org");
                int chatID = bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")).Value;
                Notification newNotification = new Notification
                {
                    chat_id = chatID,
                    text = s,
                };
                string json = JsonConvert.SerializeObject(newNotification);
                StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
                if (response.IsSuccessStatusCode)
                {
                    if (status)
                    {
                        transactionContext.UpdateTransactionNotified(transacID.Value);
                    }

                }
            }
        }
        public IActionResult Success()
        {
            return View();
        }public IActionResult Failure()
        {
            return View();
        }
    }
}

