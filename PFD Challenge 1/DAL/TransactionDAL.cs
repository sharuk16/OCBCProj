using Microsoft.Extensions.Configuration;
using PFD_Challenge_1.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.DAL
{
    public class TransactionDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;
        public TransactionDAL()
        {
            //Read ConnectionString from appsettings.json file
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString(
            "CompetifyConnectionString");
            //Instantiate a SqlConnection object with the
            //Connection String read.
            conn = new SqlConnection(strConn);
        }
        public Transaction GetLatestTransaction(string accNo)
        {
            Transaction t = null;
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement
            cmd.CommandText = @"Select * from Transactions where Sender = @AccNo";
            cmd.Parameters.AddWithValue("@AccNo", accNo);
            //Open a database connection
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    t = new Transaction
                    {
                        TransacID = reader.GetInt32(0),
                        Recipient = reader.GetString(1),
                        Sender = reader.GetString(2),
                        Amount = reader.GetDecimal(3),
                        TimeTransfer = reader.GetDateTime(4),
                        Notified = reader.GetString(5),
                        Completed = reader.GetString(6),
                        Type = reader.GetString(7),
                    };
                }
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return t;
        }
    }
}
