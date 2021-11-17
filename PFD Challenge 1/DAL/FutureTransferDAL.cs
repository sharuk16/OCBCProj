using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PFD_Challenge_1.Models;

namespace PFD_Challenge_1.DAL
{
    public class FutureTransferDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;
        public FutureTransferDAL()
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

        public List<BankAccount> GetAllBankAccount()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement
            cmd.CommandText = @"SELECT * FROM BankAccount ORDER BY AccNo";
            //Open a database connection
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();

            //Read all records until the end, save data into a competition list
            List<BankAccount> bankAccountList = new List<BankAccount>();
            while (reader.Read())
            {
                bankAccountList.Add(
                    new BankAccount
                    {
                        AccNo = reader.GetString(0),
                        Balance = reader.GetInt32(1),
                        Nric = reader.GetString(2),
                    }
                );
            }

            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return bankAccountList;
        }

        public List<FutureTransfer> GetAllFutureTransfer()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement
            cmd.CommandText = @"SELECT * FROM FutureTransfer ORDER BY FutureID";
            //Open a database connection
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();

            //Read all records until the end, save data into a future transfer list
            List<FutureTransfer> futureTransferList = new List<FutureTransfer>();
            while (reader.Read())
            {
                futureTransferList.Add(
                    new FutureTransfer
                    {
                        FutureId = reader.GetInt32(0), //0: 1st column
                        Recipient = !reader.IsDBNull(1) ?
                                    reader.GetString(1) : null,
                        Sender = !reader.IsDBNull(2) ?
                                    reader.GetString(2) : null,
                        Amount = reader.GetInt32(3),
                        PlanTime = reader.GetDateTime(4),
                        Notified = reader.GetString(5),
                        Completed = reader.GetString(6),
                    }
                );
            }

            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return futureTransferList;
        }

        public FutureTransfer ScanFutureTransfer()
        {
            FutureTransfer futureTrans = new FutureTransfer();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Transactions
                                WHERE Type = 'Future' and TimeTransfer > GETDATE()";
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    futureTrans.FutureId = reader.GetInt32(0);
                    futureTrans.Recipient = reader.GetString(1);
                    futureTrans.Sender = reader.GetString(2);
                    futureTrans.Amount = reader.GetDecimal(3);
                    futureTrans.PlanTime = reader.GetDateTime(4);
                    futureTrans.Notified = reader.GetString(5);
                    futureTrans.Completed = reader.GetString(6);
                }
            }
            reader.Close();
            conn.Close();
            return futureTrans;
        }
        public bool UpdateFutureBalance(FutureTransfer futureTrans)
        {
            SqlCommand cmd = conn.CreateCommand();
            //SQL query to update both Balances from Recipient and Sender (Future Transfer)
            cmd.CommandText = @"UPDATE BankAccount SET Balance = CASE AccNo
                                    WHEN @senderID THEN Balance - @moneySent
                                    WHEN @recipientID THEN Balance + @moneySent
                                    ELSE Balance
                                    END
                                WHERE AccNo IN(@senderID, @recipientID)";
            cmd.Parameters.AddWithValue("@senderID", futureTrans.Sender);
            cmd.Parameters.AddWithValue("@recipientID", futureTrans.Recipient);
            cmd.Parameters.AddWithValue("@moneySent", futureTrans.Amount);
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

        public bool UpdateFutureComplete(FutureTransfer futureTrans)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE FutureTransfer SET Notified = 'T'
                                WHERE FutureTransfer = @FutureID"; //Updates the Transactions's Notified Status
            cmd.Parameters.AddWithValue("@FutureID", futureTrans.FutureId);
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
