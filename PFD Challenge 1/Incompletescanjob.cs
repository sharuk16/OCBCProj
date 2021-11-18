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
        public Task Execute(IJobExecutionContext context)
        {

            Console.WriteLine("--> Database check completed");
            // transactionContext.CheckIncompleteExists();
            if (transactionContext.CheckIncompleteExists() != null)
            {
                //Console.WriteLine(transactionContext.CheckIncompleteExists().TransacID);
                return Task.FromResult<Transaction>(transactionContext.CheckIncompleteExists());
                
            }
            //var task_r = Task.FromResult<int>(transactionContext.CheckIncompleteExists().TransacID);
            

            return Task.FromResult<Transaction>(null);

        }



    }
}
