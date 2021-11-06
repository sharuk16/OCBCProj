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
    public class BankAccountDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;
        public BankAccountDAL()
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
    }
}
