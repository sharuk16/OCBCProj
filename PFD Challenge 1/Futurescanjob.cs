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
        public Task Execute(IJobExecutionContext context)
        {

            // transactionContext.CheckIncompleteExists();
            if (transactionContext.CheckIncompleteExists() != null)
            {
                Console.WriteLine("future scan works");
                //Console.WriteLine(transactionContext.CheckIncompleteExists().TransacID);
                return Task.FromResult<Transaction>(transactionContext.CheckIncompleteExists());

            }
            //var task_r = Task.FromResult<int>(transactionContext.CheckIncompleteExists().TransacID);


            return Task.FromResult<Transaction>(null);

        }



    }
}
