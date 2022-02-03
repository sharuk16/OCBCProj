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
using System.Text.RegularExpressions;

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
                    Console.WriteLine("Check not notified.");
                    if (bu != null)
                    {
                        //Check if user have telegram notification added
                        int? chatID = bankUserContext.GetUserChatID(bu.Nric);
                        Console.WriteLine((chatID == null).ToString());

                        if (chatID != null)
                        {
                            string msgtext = "Dear " + bu.Name + ", We are pleased to inform you that your transfer of funds to " +
                                su.Name + " is successful! Have a great day ahead!";
                            SendNotificationAsync(chatID,notNotified.TransacID,msgtext).ContinueWith(t => Console.WriteLine(t.Exception),
                                                    TaskContinuationOptions.OnlyOnFaulted);
                        }
                    }
                }
                // check for incomplete transactions
                Console.WriteLine("Restdb Scan");
                Task task = getRestDBIncomplete();


                //Check for incomplete transactions
                //if (transactionContext.CheckIncompleteExists() != null)
                //{
                //    Transaction incompleteTrans = transactionContext.CheckIncompleteExists();
                //    //Check for transactions that cannot occur due to user reaching transaction limit
                //    if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount) //If the amount exceeds transaction limit
                //            == false)
                //    {
                //        return Task.FromResult<Transaction>(null);
                //    }
                //    else if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount) //If the amount does not exceed the transaction limit
                //        == true)
                //    {
                //        if (incompleteTrans.Amount <= bankAccContext.GetBankAccount(incompleteTrans.Sender).Balance)
                //        {
                //            bool updatedAccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(incompleteTrans.Recipient),
                //            bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount); //Updates bank account balance records
                //            if (updatedAccounts == true) //If balance updates successfully
                //            {
                //                if (transactionContext.GetTelegramConfirmValue(incompleteTrans.TransacID) == false
                //                    && (DateTime.UtcNow.Subtract(incompleteTrans.TimeTransfer).TotalMinutes >= 20
                //                    && transactionContext.CheckTransactionConfirm(incompleteTrans.TransacID) == false))
                //                {
                //                    transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                //                }
                //                else
                //                {
                //                    transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(incompleteTrans.Sender).Nric, incompleteTrans.Amount);
                //                    transactionContext.UpdateTransactionComplete(incompleteTrans.TransacID); //Updates transaction "Completed" status
                //                    transactionContext.UpdateTransactionConfirm(incompleteTrans.TransacID); //Updates transaction "Confirm" status
                //                    string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
                //                    CheckRestDBIncomplete(incompleteTrans).Wait();
                //                    Console.WriteLine(message);
                //                    return Task.FromResult<Transaction>(incompleteTrans);
                //                }
                //            }
                //            else
                //            {
                //                string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
                //                Console.WriteLine(message);
                //                return Task.FromResult<Transaction>(incompleteTrans);
                //            }

                //            return Task.FromResult<Transaction>(incompleteTrans);
                //        }
                //        else
                //        {
                //            transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                //        }
                //    }
                //}

            }

            return Task.FromResult<Transaction>(null);
        }
        //Method to send telegram notification
        public async Task SendNotificationAsync(int? chatID, int? transacID, string msgtext)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org");
            //Create Notification object
            Notification newNotification = new Notification
            {
                chat_id = chatID.Value,
                text = msgtext,
            };
            string json = JsonConvert.SerializeObject(newNotification);
            StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            //Send the message and await respond if successful will update the transactions had been notified
            HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
            if (response.IsSuccessStatusCode)
            {
                if (transacID != null)
                {
                    transactionContext.UpdateTransactionNotified(transacID.Value);
                }                
            }
        }
        public async Task getRestDBIncomplete()
        {
            // time limit variable declared.
            //For demo purposes it will be kept at 0.5 or 30 seconds actual can be 10 mins or 20 mins
            double timeLimit = 0.5;
            // max time limit for demo it will be kept at 20 mins
            // This is so that those transaction that are still unresolved will be deleted.
            double maxTimeLimit = 20;
            //Check if there are existing transactions left in the restdb
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://ocbcdatabase-0c55.restdb.io");
            client.DefaultRequestHeaders.Add("x-api-key", "61f2742d7e55272295017175");
            HttpResponseMessage response = await client.GetAsync("/rest/temptransac");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                List<TempTransac> removal = new List<TempTransac>();
                List<TempTransac> complicated = new List<TempTransac>();
                //if there are records
                if (data != null)
                {
                    
                    List<TempTransac> tempTransacList2 = JsonConvert.DeserializeObject<List<TempTransac>>(data);
                    //
                    foreach (TempTransac t in tempTransacList2)
                    {
                        Console.WriteLine(DateTime.UtcNow.Subtract(t.TimeTransfer).TotalMinutes);
                        //check if the records have exceed the time limit
                        if(DateTime.UtcNow.Subtract(t.TimeTransfer).TotalMinutes <= timeLimit)
                        {
                            continue;
                        }
                        else if (DateTime.UtcNow.Subtract(t.TimeTransfer).TotalMinutes >= timeLimit && DateTime.UtcNow.Subtract(t.TimeTransfer).TotalMinutes <= maxTimeLimit)
                        {
                            if (t.Checkpoint1 == "False" || t.Checkpoint2 == "False")
                            {
                                removal.Add(t);
                            }
                            else
                            {
                                complicated.Add(t);
                            }
                        }                        
                        else
                        {
                            string senderId = t.Nric;
                            string receiverId = t.Recipient;
                            Regex bankacc = new Regex(@"[0-9]{3}-[0-9]{6}-[0-9]{3}");
                            BankAccount senderAccount = bankAccContext.GetBankAccount(receiverId);
                            //Validation
                            //Check if recipient exists
                            string receiverNRIC = "";
                            if (bankacc.IsMatch(receiverId))
                            {
                                BankAccount ba = bankAccContext.GetBankAccount(receiverId);
                                receiverNRIC = ba.Nric;
                            }
                            BankUser sender = bankUserContext.GetBankUser(senderId);
                            BankUser receiver = null;
                            if (receiverNRIC != "")
                            {
                                receiver = bankUserContext.GetBankUser(receiverNRIC);
                            }
                            else
                            {
                                receiver = bankUserContext.GetBankUser(receiverNRIC);
                            }
                            string receiverName = "non-existent user";
                            if (receiver != null)
                            {
                                receiverName = receiver.Name;
                            }
                            int? chatID = bankUserContext.GetUserChatID(senderId);
                            int? transacID = null;
                            if (chatID != null)
                            {
                                string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                    receiverName + " is not successful! No funds were deducted. Have a good day! :)";
                                await SendNotificationAsync(chatID, transacID, msgtext);
                            }
                            //delete record from the restdb
                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + t._id);
                            Transaction incompleteTrans = transactionContext.GetTransaction(t.TransacID);
                            if (incompleteTrans != null)
                            {
                                transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                            }
                        }
                    }
                    if (removal.Count() > 0)
                    {
                        // inform user about the failed transfer.
                        foreach (TempTransac b in removal)
                        {
                            string senderId = b.Nric;
                            string receiverId = b.Recipient;
                            BankUser sender = bankUserContext.GetBankUser(senderId);
                            BankUser receiver = bankUserContext.GetBankUser(receiverId);
                            string receiverName = "non-existent user";
                            if (receiver != null)
                            {
                                receiverName = receiver.Name;
                            }
                            int? chatID = bankUserContext.GetUserChatID(senderId);
                            int? transacID = null;
                            if (chatID != null)
                            {
                                string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                    receiverName + " is not successful! No funds were deducted. Have a good day! :)";
                                await SendNotificationAsync(chatID, transacID, msgtext);
                            }
                            //delete record from the restdb
                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                        }
                    }
                    //For those that lack confirmation or it is confirmed but failed to transfer
                    if (complicated.Count > 0)
                    {
                        foreach (TempTransac b in complicated)
                        {
                            if (b.Checkpoint3 == "False" || b.Checkpoint4 == "False")
                            {
                                Transaction incompleteTrans = transactionContext.GetTransaction(b.TransacID);
                                //catch no transid in restdb
                                if (incompleteTrans == null)
                                {
                                    string senderId = b.Nric;
                                    string receiverId = b.Recipient;
                                    Regex bankacc = new Regex(@"[0-9]{3}-[0-9]{6}-[0-9]{3}");
                                    BankAccount senderAccount = bankAccContext.GetBankAccount(receiverId);
                                    //Validation
                                    //Check if recipient exists
                                    string receiverNRIC = "";
                                    if (bankacc.IsMatch(receiverId))
                                    {
                                        BankAccount ba = bankAccContext.GetBankAccount(receiverId);
                                        receiverNRIC = ba.Nric;
                                    }
                                    BankUser sender = bankUserContext.GetBankUser(senderId);
                                    BankUser receiver = null;
                                    if (receiverNRIC != "")
                                    {
                                        receiver = bankUserContext.GetBankUser(receiverNRIC);
                                    }
                                    else
                                    {
                                        receiver = bankUserContext.GetBankUser(receiverNRIC);
                                    }
                                    string receiverName = "non-existent user";
                                    if (receiver != null)
                                    {
                                        receiverName = receiver.Name;
                                    }
                                    int? chatID = bankUserContext.GetUserChatID(senderId);
                                    int? transacID = null;
                                    if (chatID != null)
                                    {
                                        string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                            receiverName + " is not successful! No funds were deducted. Have a good day! :)";
                                        await SendNotificationAsync(chatID, transacID, msgtext);
                                    }
                                    //delete record from the restdb
                                    HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                }
                                else
                                {//If the amount exceeds transaction limit
                                    if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount)
                                == false)
                                    {
                                        string senderId = incompleteTrans.Sender;
                                        string receiverId = incompleteTrans.Recipient;
                                        BankAccount ba = bankAccContext.GetBankAccount(senderId);
                                        BankAccount sa = bankAccContext.GetBankAccount(receiverId);
                                        BankUser sender = bankUserContext.GetBankUser(ba.Nric);
                                        BankUser receiver = bankUserContext.GetBankUser(sa.Nric);
                                        string receiverName = receiverName = receiver.Name;
                                        int? chatID = bankUserContext.GetUserChatID(senderId);
                                        int? transacID = null;
                                        if (chatID != null)
                                        {
                                            string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                receiverName + " is not successful! No funds were deducted due to transaction limit reached." +
                                                "If you would like to make the transfer again. Please set your transaction limit higher. Have a good day! :)";
                                            await SendNotificationAsync(chatID, transacID, msgtext);

                                        }
                                        //delete record from the restdb
                                        HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                        //delete record from sqldb
                                        transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);

                                    }
                                    else if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount)
                                        == true)//If the amount does not exceed the transaction limit
                                    {
                                        DateTime? teleConfirmSent = transactionContext.getTelegramDate(incompleteTrans.TransacID);
                                        if (teleConfirmSent == null)
                                        {
                                            string senderId = incompleteTrans.Sender;
                                            string receiverId = incompleteTrans.Recipient;
                                            BankAccount ba = bankAccContext.GetBankAccount(senderId);
                                            BankAccount sa = bankAccContext.GetBankAccount(receiverId);
                                            BankUser sender = bankUserContext.GetBankUser(ba.Nric);
                                            BankUser receiver = bankUserContext.GetBankUser(sa.Nric);
                                            string receiverName = receiverName = receiver.Name;
                                            int? chatID = bankUserContext.GetUserChatID(senderId);
                                            int? transacID = null;
                                            if (chatID != null)
                                            {
                                                string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                    receiverName + " has no confirmation! To confirm the transfer please reply with your nric within 15 mins! To cancel the transaction do not reply to this message.";
                                                await SendNotificationAsync(chatID, transacID, msgtext);
                                                transactionContext.setTelegramDate(incompleteTrans.TransacID, DateTime.UtcNow);
                                            }
                                            else
                                            {
                                                //delete record from the restdb
                                                HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                            }

                                        }
                                        else
                                        {
                                            if (teleConfirmSent.Value.Subtract(DateTime.UtcNow).TotalMinutes <= 15)
                                            {
                                                string senderId = incompleteTrans.Sender;
                                                string receiverId = incompleteTrans.Recipient;
                                                BankAccount ba = bankAccContext.GetBankAccount(senderId);
                                                BankAccount sa = bankAccContext.GetBankAccount(receiverId);
                                                BankUser sender = bankUserContext.GetBankUser(ba.Nric);
                                                BankUser receiver = bankUserContext.GetBankUser(sa.Nric);
                                                string nric = sender.Nric;
                                                if (bankUserContext.GetUserChatID(nric) != null)
                                                {
                                                    if (incompleteTrans.Amount <= bankAccContext.GetBankAccount(incompleteTrans.Sender).Balance)
                                                    {
                                                        bool telegramConfirm = await CheckTeleConfirm(nric, bankUserContext.GetUserChatID(nric).Value, teleConfirmSent.Value);
                                                        if (!telegramConfirm)
                                                        {
                                                            string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                            receiver.Name + " has no confirmation! The transfer will be terminated. No funds have been transferred.";
                                                            await SendNotificationAsync(bankUserContext.GetUserChatID(nric).Value, null, msgtext);
                                                            transactionContext.setTelegramDate(incompleteTrans.TransacID, DateTime.UtcNow);

                                                            //delete record from the restdb
                                                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                            transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                                        }
                                                        else
                                                        {
                                                            bool updatedaccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(incompleteTrans.Recipient),
                                                            bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount);
                                                            if (updatedaccounts == true) //if balance updates successfully
                                                            {

                                                                transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(incompleteTrans.Sender).Nric, incompleteTrans.Amount);
                                                                transactionContext.UpdateTransactionConfirm(incompleteTrans.TransacID); //updates transaction "completed" status
                                                                transactionContext.UpdateTransactionConfirm(incompleteTrans.TransacID); //updates transaction "confirm" status
                                                                string message = transactionContext.TransactionStatusMsg(updatedaccounts); //notification message string for success
                                                                Console.WriteLine(message);

                                                            }
                                                            else
                                                            {
                                                                string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                                receiver.Name + " has an error occured! The transfer will be terminated. No funds have been transferred.";
                                                                await SendNotificationAsync(bankUserContext.GetUserChatID(nric).Value, null, msgtext);

                                                                //delete record from the restdb
                                                                HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                                transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        int? chatID = bankUserContext.GetUserChatID(senderId);
                                                        int? transacID = null;
                                                        if (chatID != null)
                                                        {
                                                            string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                            receiver.Name + " has lack of funds! The transfer will be terminated. No funds have been transferred.";
                                                            await SendNotificationAsync(chatID, transacID, msgtext);
                                                            transactionContext.setTelegramDate(incompleteTrans.TransacID, DateTime.UtcNow);
                                                            //delete record from the restdb
                                                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                            transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                                        }
                                                        else
                                                        {
                                                            //delete record from the restdb
                                                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                            transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                                        }
                                                    }
                                                }
                                                else
                                                {

                                                    //delete record from the restdb
                                                    HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                    transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Transaction incompleteTrans = transactionContext.CheckIncompleteExists();
                                if (incompleteTrans == null)
                                {
                                    string senderId = b.Nric;
                                    string receiverId = b.Recipient;
                                    BankUser sender = bankUserContext.GetBankUser(senderId);
                                    BankUser receiver = bankUserContext.GetBankUser(receiverId);
                                    string receiverName = "non-existent user";
                                    if (receiver != null)
                                    {
                                        receiverName = receiver.Name;
                                    }
                                    int? chatID = bankUserContext.GetUserChatID(senderId);
                                    int? transacID = null;
                                    if (chatID != null)
                                    {
                                        string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                            receiverName + " is not successful! No funds were deducted. Have a good day! :)";
                                        await SendNotificationAsync(chatID, transacID, msgtext);
                                    }
                                    //delete record from the restdb
                                    HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                }
                                else
                                {
                                    //Check for transactions that cannot occur due to user reaching transaction limit
                                    if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount) //If the amount exceeds transaction limit
                                            == false)
                                    {
                                        string senderId = incompleteTrans.Sender;
                                        string receiverId = incompleteTrans.Recipient;
                                        BankAccount ba = bankAccContext.GetBankAccount(senderId);
                                        BankAccount sa = bankAccContext.GetBankAccount(receiverId);
                                        BankUser sender = bankUserContext.GetBankUser(ba.Nric);
                                        BankUser receiver = bankUserContext.GetBankUser(sa.Nric);
                                        string receiverName = receiverName = receiver.Name;
                                        int? chatID = bankUserContext.GetUserChatID(senderId);
                                        int? transacID = null;
                                        if (chatID != null)
                                        {
                                            string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                receiverName + " is not successful! No funds were deducted due to transaction limit reached." +
                                                "If you would like to make the transfer again. Please set your transaction limit higher. Have a good day! :)";
                                            await SendNotificationAsync(chatID, transacID, msgtext);

                                        }
                                        //delete record from the restdb
                                        HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                        //delete record from sqldb
                                        transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                    }
                                    else if (transactionContext.ValidateTransactionLimit(bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount) //If the amount does not exceed the transaction limit
                                        == true)
                                    {
                                        if (incompleteTrans.Amount <= bankAccContext.GetBankAccount(incompleteTrans.Sender).Balance)
                                        {
                                            bool updatedaccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(incompleteTrans.Recipient),
                                                            bankAccContext.GetBankAccount(incompleteTrans.Sender), incompleteTrans.Amount);
                                            if (updatedaccounts == true) //if balance updates successfully
                                            {

                                                transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(incompleteTrans.Sender).Nric, incompleteTrans.Amount);
                                                transactionContext.UpdateTransactionConfirm(incompleteTrans.TransacID); //updates transaction "completed" status
                                                transactionContext.UpdateTransactionConfirm(incompleteTrans.TransacID); //updates transaction "confirm" status
                                                string message = transactionContext.TransactionStatusMsg(updatedaccounts); //notification message string for success
                                                Console.WriteLine(message);

                                            }
                                            else
                                            {
                                                string senderId = incompleteTrans.Sender;
                                                string receiverId = incompleteTrans.Recipient;
                                                BankAccount ba = bankAccContext.GetBankAccount(senderId);
                                                BankAccount sa = bankAccContext.GetBankAccount(receiverId);
                                                BankUser sender = bankUserContext.GetBankUser(ba.Nric);
                                                BankUser receiver = bankUserContext.GetBankUser(sa.Nric);
                                                string receiverName = receiverName = receiver.Name;
                                                int? chatID = bankUserContext.GetUserChatID(senderId);
                                                int? transacID = null;
                                                if (chatID != null)
                                                {
                                                    string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                                receiver.Name + " has an error occured! The transfer will be terminated. No funds have been transferred.";
                                                    await SendNotificationAsync(chatID, transacID, msgtext);

                                                }
                                                //delete record from the restdb
                                                HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                                //delete record from sqldb
                                                transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                            }
                                        }
                                        else
                                        {
                                            string senderId = incompleteTrans.Sender;
                                            string receiverId = incompleteTrans.Recipient;
                                            BankAccount ba = bankAccContext.GetBankAccount(senderId);
                                            BankAccount sa = bankAccContext.GetBankAccount(receiverId);
                                            BankUser sender = bankUserContext.GetBankUser(ba.Nric);
                                            BankUser receiver = bankUserContext.GetBankUser(sa.Nric);
                                            string receiverName = receiverName = receiver.Name;
                                            int? chatID = bankUserContext.GetUserChatID(senderId);
                                            int? transacID = null;
                                            if (chatID != null)
                                            {
                                                string msgtext = "Dear " + sender.Name + ", we would like to inform you that your transfer of funds to " +
                                                            receiver.Name + " has lack of funds! The transfer will be terminated. No funds have been transferred.";
                                                await SendNotificationAsync(chatID, transacID, msgtext);

                                            }
                                            //delete record from the restdb
                                            HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + b._id);
                                            //delete record from sqldb
                                            transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public async Task<bool> CheckTeleConfirm(string nric,int chatId, DateTime teleConfirm)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org");
            HttpResponseMessage response = await client.GetAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/getUpdates");
            string data = await response.Content.ReadAsStringAsync();
            Root result = JsonConvert.DeserializeObject<Root>(data);
            foreach (Result r in result.result)
            {
                //Check if message is the same as code
                if (r.message.text == nric && r.message.chat.id== chatId)
                {
                    if (teleConfirm.Subtract(Convert.ToDateTime(r.message.date)).TotalMinutes < 15)
                    {
                        return true;
                    }
                    //messageCapture should be later than time accessed to the page
                    //endtime should be within duration                    
                }
            }
            return false;
        }
        ////Scans the database for False in checkpoints 3 and 4 and should send a Telegram message to inform them of action taken
        //public async Task CheckRestDBIncomplete(Transaction transac)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri("https://ocbcdatabase-0c55.restdb.io");
        //    client.DefaultRequestHeaders.Add("x-api-key", "61f2742d7e55272295017175");
        //    HttpResponseMessage getResponse = await client.GetAsync("/rest/temptransac");
        //    if (getResponse.IsSuccessStatusCode)
        //    {
        //        string data = await getResponse.Content.ReadAsStringAsync();
        //        if (data != null)
        //        {
        //            List<TempTransac> tempTransacList = JsonConvert.DeserializeObject<List<TempTransac>>(data);
        //            foreach (TempTransac tempTransac in tempTransacList)
        //            {
        //                //If either 3 or 4 are false, delete the record.
        //                if (tempTransac.Checkpoint3 == "False" || tempTransac.Checkpoint4 == "False")
        //                {
        //                    HttpResponseMessage deleteResponse = await client.DeleteAsync("/rest/temptransac/" + tempTransac._id);
        //                }
        //                //If Checkpoint 4 is true, carry out the transaction in the backend.
        //                else if (tempTransac.Checkpoint4 == "True")
        //                {
        //                    bool updatedAccounts = transactionContext.UpdateTransactionChanges(bankAccContext.GetBankAccount(transac.Recipient),
        //                        bankAccContext.GetBankAccount(transac.Sender), transac.Amount); //Updates bank account balance records
        //                    if (updatedAccounts == true) //If balance updates successfully
        //                    {
        //                        transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(transac.Sender).Nric, transac.Amount);
        //                        transactionContext.UpdateTransactionComplete(transac.TransacID); //Updates transaction "Completed" status
        //                        transactionContext.UpdateTransactionConfirm(transac.TransacID); //Updates transaction "Confirm" status
        //                        string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for success
        //                        Console.WriteLine(message);
        //                    }
        //                    else
        //                    {
        //                        string message = transactionContext.TransactionStatusMsg(updatedAccounts); //Notification message string for failure
        //                        Console.WriteLine(message);
        //                    }
        //                }
        //            }
        //        }
        //    }
        
    }
}
