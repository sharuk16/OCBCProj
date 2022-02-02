using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;
using System.Net.Http;
using PFD_Challenge_1.TelegramModel;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.AspNetCore.Mvc;

namespace PFD_Challenge_1
{
    public class Incompletescanjob : IJob
    {
        TransactionDAL transactionContext = new TransactionDAL();
        BankAccountDAL bankAccContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        public Task Execute(IJobExecutionContext context)
        {
            if (transactionContext.CheckIncompleteExists() == null&& transactionContext.CheckNotNotified()==null)
            {
                return Task.FromResult<Transaction>(null);
            }
            else
            {
                //Check for transactions that are not notified
                if(transactionContext.CheckNotNotified()!= null)
                {
                    //Get Details of the transaction
                    Transaction notNotified = transactionContext.CheckNotNotified();
                    BankAccount ba = bankAccContext.GetBankAccount(notNotified.Sender);
                    BankAccount sa = bankAccContext.GetBankAccount(notNotified.Recipient);
                    BankUser bu = bankUserContext.GetBankUser(ba.Nric);
                    BankUser su = bankUserContext.GetBankUser(bu.Nric);
                    Console.WriteLine("test1");
                    if (bu != null)
                    {
                        //Check if user have telegram notification added
                        int? chatID = bankUserContext.GetUserChatID(bu.Nric);
                        Console.WriteLine((chatID == null).ToString());

                        if (chatID != null)
                        {
                            Console.WriteLine("test2");
                            SendNotificationAsync(chatID, bu, su, notNotified.TransacID, notNotified.Amount,notNotified.TimeTransfer).ContinueWith(t => Console.WriteLine(t.Exception),
                                                    TaskContinuationOptions.OnlyOnFaulted);
                        }
                    }
                }
                //Check for incomplete transactions
                if (transactionContext.CheckIncompleteExists() != null)
                {
                    Transaction incompleteTrans = transactionContext.CheckIncompleteExists();
                    //Check for transactions that cannot occur due to user reaching transaction limit
                    if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount) //If the amount exceeds transaction limit
                            == false)
                    {
                        return Task.FromResult<Transaction>(null);
                    }
                    else if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount) //If the amount does not exceed the transaction limit
                        == true)
                    {
                        if (incompleteTrans.Amount <= bankAccContext.GetBankAccount(incompleteTrans.Sender).Balance)
                        {
                            bool updatedAccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(incompleteTrans.Recipient),
                            bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount); //Updates bank account balance records
                            if (updatedAccounts == true) //If balance updates successfully
                            {
                                if (transactionContext.GetTelegramConfirmValue(incompleteTrans.TransacID) == false 
                                    && (DateTime.UtcNow.Subtract(incompleteTrans.TimeTransfer).TotalMinutes >= 20 
                                    && transactionContext.CheckTransactionConfirm(incompleteTrans.TransacID) == false))
                                {
                                    transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                }
                                else
                                {
                                    transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(incompleteTrans.Sender).Nric, incompleteTrans.Amount);
                                    transactionContext.UpdateTransactionComplete(incompleteTrans.TransacID); //Updates transaction "Completed" status
                                    transactionContext.UpdateTransactionConfirm(incompleteTrans.TransacID); //Updates transaction "Confirm" status
                                    string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                                    CheckRestDBIncomplete(incompleteTrans).Wait();
                                    Console.WriteLine(message);
                                    return Task.FromResult<Transaction>(incompleteTrans);
                                }
                            }
                            else
                            {
                                string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                                Console.WriteLine(message);
                                return Task.FromResult<Transaction>(incompleteTrans);
                            }
                            
                            return Task.FromResult<Transaction>(incompleteTrans);
                        }
                        else
                        {
                            transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                        }
                    }
                }

            }

            return Task.FromResult<Transaction>(null);
        }
        //Method to send telegram notification
        public async Task SendNotificationAsync(int? chatID, BankUser bu, BankUser su, int? transacID, decimal amount,DateTime transfer)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org");
            //Create Notification object
            Notification newNotification = new Notification
            {
                chat_id = chatID.Value,
                text = "Dear " + bu.Name + "! You have successfully transfered $" + amount.ToString() + " to " + su.Name + "! Date and Time of Transfer: " + transfer.ToString(),
            };
            string json = JsonConvert.SerializeObject(newNotification);
            StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            //Send the message and await respond if successful will update the transactions had been notified
            HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
            if (response.IsSuccessStatusCode)
            {
                transactionContext.UpdateTransactionNotified(transacID.Value);
            }
        }

        //Scans the database for False in checkpoints 3 and 4 and should send a Telegram message to inform them of action taken
        public async Task CheckRestDBIncomplete(Transaction transac)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://ocbcdatabase-0c55.restdb.io");
            client.DefaultRequestHeaders.Add("x-api-key", "61f2742d7e55272295017175");
            HttpResponseMessage getResponse = await client.GetAsync("/rest/temptransac");
            if (getResponse.IsSuccessStatusCode)
            {
                string data = await getResponse.Content.ReadAsStringAsync();
                if (data != null)
                {
                    List<TempTransac> tempTransacList = JsonConvert.DeserializeObject<List<TempTransac>>(data);
                    foreach (TempTransac tempTransac in tempTransacList)
                    {
                        //If either 3 or 4 are false, delete the record.
                        if (tempTransac.Checkpoint3 == "False" || tempTransac.Checkpoint4 == "False")
                        {
                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + tempTransac._id);
                        }
                        //If Checkpoint 4 is true, carry out the transaction in the backend.
                        else if (tempTransac.Checkpoint4 == "True")
                        {
                            bool updatedAccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(transac.Recipient),
                                bankAccContext.GetBankAccount(transac.Sender), transac.Amount); //Updates bank account balance records
                            if (updatedAccounts == true) //If balance updates successfully
                            {
                                transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(transac.Sender).Nric, transac.Amount);
                                transactionContext.UpdateTransactionComplete(transac.TransacID); //Updates transaction "Completed" status
                                transactionContext.UpdateTransactionConfirm(transac.TransacID); //Updates transaction "Confirm" status
                                string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                                Console.WriteLine(message);
                            }
                            else
                            {
                                string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                                Console.WriteLine(message);
                            }
                        }
                    }
                }
            }
        }
    }
}
