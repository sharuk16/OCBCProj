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
       
        public int AddTransactionRecord(Transaction transac)
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
            cmd.Parameters.AddWithValue("@sender", transac.Sender);
            cmd.Parameters.AddWithValue("@amount", transac.Amount);
            cmd.Parameters.AddWithValue("@timetransfer", transac.TimeTransfer);
            cmd.Parameters.AddWithValue("@notified", "F");
            cmd.Parameters.AddWithValue("@completed", "F");
            cmd.Parameters.AddWithValue("@type", transac.Type);

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

        public bool UpdateTransactionNotified(int transacID) //Insert transaction object
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Transactions SET Notified = 'T'
                                WHERE TransacID = @TransacID"; //Updates the Transactions's Notified Status
            cmd.Parameters.AddWithValue("@TransacID", transacID);
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

        public Transaction CheckIncompleteExists() //Checks for Immediate Transactions that are still incomplete
        {
            Transaction transac = null;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Transactions
                                WHERE Completed = 'F' AND Type = 'Immediate'";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    transac.TransacID = reader.GetInt32(0);
                    transac.Recipient = reader.GetString(1);
                    transac.Sender = reader.GetString(2);
                    transac.Amount = reader.GetDecimal(3);
                    transac.TimeTransfer = reader.GetDateTime(4);
                    transac.Notified = reader.GetString(5);
                    transac.Completed = reader.GetString(6);
                    transac.Type = reader.GetString(7);
                }
            }
            reader.Close();
            conn.Close();
            if (transac == null)
            {
                return null;
            }
            return transac;
        }

        public bool ValidateTransactionLimit(BankAccount bankAcc, decimal transAmt) //Checks if transfer amount exceeds transfer limit
        {
            bool validLimit  = false;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TransLimit, DailySpend FROM BankUser
                                WHERE NRIC = @NRIC";
            cmd.Parameters.AddWithValue("@NRIC", bankAcc.Nric);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.HasRows)
            {
                while (reader.Read())
                {
                    if (reader.GetDecimal(0) < transAmt)
                    {
                        validLimit = false;
                    }
                    else
                    {
                        if(reader.GetDecimal(0) < reader.GetDecimal(1)+transAmt)
                        {
                            validLimit = false;
                        }
                        else
                        {
                            validLimit = true;
                        }
                    }
                }
            }
            reader.Close();
            conn.Close();
            return validLimit;
        }

        public bool UpdateDailySpend(string nric, decimal amount)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE BankUser SET DailySpend = DailySpend + @amount
                                WHERE NRIC = @nric";
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@nric", nric);
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

        public bool ResetDailySpend()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE BankUser SET DailySpend = 0 ";
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

        public string TransactionStatusMsg(bool status)
        {
            string message;
            if (status==true)
            {
                message = "Transaction Successful.";
            }
            else
            {
                message = "Transaction Unsuccessful. Redirecting to Home Page.";
            }
            return message;
        }

        public bool DeleteTransactionRecord(int transacID)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE * FROM Transactions
                                WHERE TransacID = @TransacID";
            cmd.Parameters.AddWithValue("@TransacID", transacID);
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
