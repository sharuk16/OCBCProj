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
    public class BankUserDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;
        public BankUserDAL()
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

        public List<BankUser> GetAllBankUser()
        {
            //Create a SqlCommand object from connection object
            SqlCommand cmd = conn.CreateCommand();
            //Specify the SELECT SQL statement
            cmd.CommandText = @"SELECT * FROM BankUser ORDER BY NRIC";
            //Open a database connection
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();

            //Read all records until the end, save data into a competition list
            List<BankUser> bankUserList = new List<BankUser>();
            while (reader.Read())
            {
                bankUserList.Add(
                    new BankUser
                    {
                        Nric = reader.GetString(0),
                        Email = reader.GetString(1),
                        Phone = reader.GetString(2),
                        Name = reader.GetString(3),
                        TransLimit = reader.GetDecimal(4),
                        Password = reader.GetString(5),
                    }
                );
            }

            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return bankUserList;
        }
    }
}
