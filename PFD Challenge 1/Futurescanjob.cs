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
    public class Futurescanjob : IJob
    {
        TransactionDAL transactionContext = new TransactionDAL();
        FutureTransferDAL futureTransContext = new FutureTransferDAL();
        BankAccountDAL bankAccContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        public Task Execute(IJobExecutionContext context)
        {
 
            if (futureTransContext.ScanFutureTransfer().Count() != 0)
            {
                List<FutureTransfer> incompleteTransList = futureTransContext.ScanFutureTransfer();
                foreach(FutureTransfer f in incompleteTransList)
                {
                    if (f.Amount <= bankAccContext.GetBankAccount(f.Sender).Balance)
                    {
                        if(transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(f.Sender), f.Amount)==true)
                        {
                            if(futureTransContext.UpdateFutureBalance(f) == true)
                            {
                                int transacID =-1;
                                Transaction newTransac = new Transaction //Create new transaction object
                                {
                                    Recipient = bankAccContext.GetBankAccount(f.Recipient).AccNo,
                                    Sender = bankAccContext.GetBankAccount(f.Sender).AccNo,
                                    Amount = f.Amount,
                                    TimeTransfer = f.PlanTime,
                                    Type = "Future"
                                };
                                if(futureTransContext.FutureTransferExists(f) == false)
                                {
                                    transacID=transactionContext.AddTransactionRecord(newTransac);
                                }
                                futureTransContext.DeleteFutureTransfer(f);
                                //Checks and validations to ensure transaction can be completed
                                transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(f.Sender).Nric, f.Amount);
                                futureTransContext.UpdateFutureComplete(f);
                                if (transacID != -1)
                                {
                                    //Get information of the transfer
                                    transactionContext.UpdateTransactionComplete(transacID);
                                    BankAccount ba = bankAccContext.GetBankAccount(f.Sender);
                                    BankAccount sa = bankAccContext.GetBankAccount(f.Recipient);
                                    BankUser bu = bankUserContext.GetBankUser(ba.Nric);
                                    BankUser su = bankUserContext.GetBankUser(bu.Nric);
                                    if (bu != null)
                                    {
                                        int? chatID = bankUserContext.GetUserChatID(bu.Nric);
                                        if (chatID != null)
                                        {//Method to send telegram notification
                                            SendNotificationAsync(chatID, bu, su, transacID, f.Amount).ContinueWith(t => Console.WriteLine(t.Exception),
        TaskContinuationOptions.OnlyOnFaulted);
                                        }                                        
                                    }                                    
                                }
                                Console.WriteLine("Initiating Future Transfer scan") ;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                return Task.FromResult<List<FutureTransfer>>(null);
            }

            return Task.FromResult<List<FutureTransfer>>(null);
        }
        public async Task SendNotificationAsync(int? chatID, BankUser bu, BankUser su, int? transacID, decimal amount)
        {
            //Method to send Telegram notification
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org");
            //Create notification object
            Notification newNotification = new Notification
            {
                chat_id = chatID.Value,
                text = "Dear " + bu.Name + "! You have successfully transfered $" + amount.ToString() + " to " + su.Name + "! Date and Time of Transfer: " + DateTime.Now.ToString(),
            };
            string json = JsonConvert.SerializeObject(newNotification);
            StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            //Sending telegram notifications and if response is 200 will update database
            HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
            if (response.IsSuccessStatusCode)
            {
                transactionContext.UpdateTransactionNotified(transacID.Value);
            }
        }
    }
}
