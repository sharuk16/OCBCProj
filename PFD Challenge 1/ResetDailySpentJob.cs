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
    public class ResetDailySpentJob : IJob
    {
        TransactionDAL transactionContext = new TransactionDAL();
        FutureTransferDAL futureTransContext = new FutureTransferDAL();
        BankAccountDAL bankAccContext = new BankAccountDAL();

        public Task Execute(IJobExecutionContext context)
        {
            transactionContext.ResetDailySpend();
            Console.WriteLine("Initiatting daily spent amount");
            return Task.FromResult(0);
        }
    }
}
