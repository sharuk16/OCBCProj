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
    public class Futurescanjob : IJob
    {
        TransactionDAL transactionContext = new TransactionDAL();
        FutureTransferDAL futureTransContext = new FutureTransferDAL();
        BankAccountDAL bankAccContext = new BankAccountDAL();

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
                                Transaction newTransac = new Transaction //Create new transaction object
                                {
                                    Recipient = bankAccContext.GetBankAccount(f.Recipient).AccNo,
                                    Sender = bankAccContext.GetBankAccount(f.Sender).AccNo,
                                    Amount = f.Amount,
                                    TimeTransfer = f.PlanTime,
                                    Type = "Future"
                                };
                                int returnTransac= transactionContext.AddTransactionRecord(newTransac);
                                transactionContext.UpdateDailySpend(bankAccContext.GetBankAccount(f.Sender).Nric, f.Amount);
                                futureTransContext.UpdateFutureComplete(f);
                                transactionContext.UpdateTransactionComplete(returnTransac);
                                Console.WriteLine("Initaitng Future Transfer scan") ;
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
    }
}
