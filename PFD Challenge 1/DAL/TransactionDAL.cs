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

        public bool UpdateTransactionChanges //Updates recipient and sender BankAccount
            (BankAccount recipientAcc, BankAccount senderAcc, decimal moneySent)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE BankAccount SET Balance = CASE AccNo
                                    WHEN @senderID THEN Balance - @moneySent
                                    WHEN @recipientID THEN Balance + @moneySent
                                    ELSE Balance
                                    END
                                WHERE AccNo IN(@senderID, @recipientID)";
            cmd.Parameters.AddWithValue("@senderID", senderAcc.AccNo);
            cmd.Parameters.AddWithValue("@recipientID", recipientAcc.AccNo);
            cmd.Parameters.AddWithValue("@moneySent", moneySent);
            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
       

        public int AddTransactionRecord(Transaction transac)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Transactions
                                (Recipient, Sender, Amount, 
                                TimeTransfer, Notified, Completed, Type)
                                OUTPUT INSERTED.TransacID
                                VALUES(@transacID, @recipient, @sender, @amount,
                                @timetransfer, @notified, @completed, @type)";
            cmd.Parameters.AddWithValue("@recipient", transac.Recipient);
            cmd.Parameters.AddWithValue("@sender", transac.Sender);
            cmd.Parameters.AddWithValue("@amount", transac.Amount);
            cmd.Parameters.AddWithValue("@timetransfer", transac.TimeTransfer);
            cmd.Parameters.AddWithValue("@notified", "N");
            cmd.Parameters.AddWithValue("@completed", "N");
            cmd.Parameters.AddWithValue("@type", transac.Type);

            conn.Open();

            transac.TransacID = (int)cmd.ExecuteScalar();

            conn.Close();

            return transac.TransacID;
        }
        public bool UpdateTransactionComplete(Transaction transac)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Transactions SET Completed = 'T'
                                WHERE TransacID = @TransacID"; //Updates the Transactions's Completed Status
            cmd.Parameters.AddWithValue("@TransacID", transac.TransacID);
            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Transaction> GetAllTransaction(string accNo)
        {
            List<Transaction> t = new List<Transaction>();
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement
            cmd.CommandText = @"Select * from Transactions where Sender = @AccNo or Recipient = @AccNo order by TimeTransfer desc";
            cmd.Parameters.AddWithValue("@AccNo", accNo);
            //Open a database connection
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    t.Add(new Transaction
                    {
                        TransacID = reader.GetInt32(0),
                        Recipient = reader.GetString(1),
                        Sender = reader.GetString(2),
                        Amount = reader.GetDecimal(3),
                        TimeTransfer = reader.GetDateTime(4),
                        Notified = reader.GetString(5),
                        Completed = reader.GetString(6),
                        Type = reader.GetString(7),
                    }
                    );
                }
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return t;
        }
        public bool ReverseTransactionChanges //Reverses database changes from the current transaction
            (BankAccount recipientAcc, BankAccount senderAcc, decimal moneySent)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE BankAccount SET Balance = CASE AccNo
                                    WHEN @senderID THEN Balance + @moneySent
                                    WHEN @recipientID THEN Balance - @moneySent
                                    ELSE Balance
                                    END
                                WHERE AccNo IN(@senderID, @recipientID)";
            cmd.Parameters.AddWithValue("@senderID", senderAcc.AccNo);
            cmd.Parameters.AddWithValue("@recipientID", recipientAcc.AccNo);
            cmd.Parameters.AddWithValue("@moneySent", moneySent);
            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateTransactionNotified(Transaction transac) //Insert transaction object
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Transactions SET Notified = 'T'
                                WHERE TransacID = @TransacID"; //Updates the Transactions's Notified Status
            cmd.Parameters.AddWithValue("@TransacID", transac.TransacID);
            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();
            if (count > 0)  //Returns true/false based on whether update is successful
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
