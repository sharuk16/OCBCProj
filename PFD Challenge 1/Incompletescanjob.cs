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
                            if (transactionContext.CheckTransactionConfirm(incompleteTrans.TransacID) == true)
                            {
                                bool updatedAccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(incompleteTrans.Recipient),
                                bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount); //Updates bank account balance records
                                if (updatedAccounts == true) //If balance updates successfully
                                {
                                    transactionContext.UpdateTransactionComplete(incompleteTrans.TransacID); //Updates transaction "Completed" status
                                    transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(incompleteTrans.Sender).Nric, incompleteTrans.Amount);
                                    string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                                    Console.WriteLine("Database check completed");
                                    return Task.FromResult<Transaction>(incompleteTrans);

                                }
                                else
                                {
                                    string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                                    return Task.FromResult<Transaction>(incompleteTrans);
                                }
                            }
                            else
                            {
                                return Task.FromResult<Transaction>(incompleteTrans);
                            }
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
    }
}
