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
            //SQL query to update both Balances from Recipient and Sender
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
       

        public int AddTransactionRecord(TransferConfirmation transac)
        {
            SqlCommand cmd = conn.CreateCommand();
            //SQL query to create a new Transactions object in the database for records.
            cmd.CommandText = @"INSERT INTO Transactions
                                (Recipient, Sender, Amount, 
                                TimeTransfer, Notified, Completed, Type)
                                OUTPUT INSERTED.TransacID
                                VALUES(@recipient, @sender, @amount,
                                @timetransfer, @notified, @completed, @type)";
            cmd.Parameters.AddWithValue("@recipient", transac.Recipient);
            cmd.Parameters.AddWithValue("@sender", transac.BankAccount);
            cmd.Parameters.AddWithValue("@amount", transac.TransferAmount);
            cmd.Parameters.AddWithValue("@timetransfer", transac.TimeTransfer);
            cmd.Parameters.AddWithValue("@notified", "N");
            cmd.Parameters.AddWithValue("@completed", "N");
            string transType;
            if (transac.FutureTransfer == "true")
            {
                transType = "Future";
            }
            else
            {
                transType = "Immediate";
            }
            cmd.Parameters.AddWithValue("@type", transType);

            conn.Open();

            int transacID = (int)cmd.ExecuteScalar();

            conn.Close();

            return transacID;
        }
        public bool UpdateTransactionComplete(int transacID)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Transactions SET Completed = 'T'
                                WHERE TransacID = @TransacID"; //Updates the Transactions's Completed Status
            cmd.Parameters.AddWithValue("@TransacID", transacID);
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
            //SQL query to reverse both Balances from Recipient and Sender to the state before the transaction.
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

        public bool CheckIncompleteExists() //Checks for Immediate Transactions that are still incomplete
        {
            bool incompleteExists;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Completed FROM Transactions
                                WHERE Completed = 'F' AND Type <> 'Future'";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                incompleteExists = true;
            }
            else
            {
                incompleteExists = false;
            }
            reader.Close();
            conn.Close();
            return incompleteExists;
        }

        public bool ValidateTransactionLimit(BankAccount bankAcc, decimal transAmt)
        {
            bool validLimit;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TransLimit FROM BankUser
                                WHERE NRIC = @NRIC";
            cmd.Parameters.AddWithValue("@NRIC", bankAcc.Nric);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.GetDecimal(0) < transAmt)
            {
                validLimit = false;
            }
            else
            {
                validLimit = true;
            }
            reader.Close();
            conn.Close();
            return validLimit;
        }
    }
}
