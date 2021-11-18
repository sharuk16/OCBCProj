using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;

namespace PFD_Challenge_1
{
    public class Incompletescanjob : IJob
    {
        TransactionDAL transactionContext = new TransactionDAL();
        BankAccountDAL bankAccContext = new BankAccountDAL();
        public Task Execute(IJobExecutionContext context)
        {
            // transactionContext.CheckIncompleteExists();
            if (transactionContext.CheckIncompleteExists() == null)
            {
                return Task.FromResult<Transaction>(null);
            }
            else
            {
                Transaction incompleteTrans = transactionContext.CheckIncompleteExists();
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
                        transactionContext.DeleteTransactionRecord(incompleteTrans.TransacID);
                    }
                }

            }
            return Task.FromResult<Transaction>(null);
        }



    }
}
