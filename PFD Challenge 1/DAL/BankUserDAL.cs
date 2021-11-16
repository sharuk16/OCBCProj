using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PFD_Challenge_1.Models;
using System.Text.RegularExpressions;

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
        public BankUser GetBankUser(string pattern)
        {
            BankUser user = null;
            SqlCommand cmd = conn.CreateCommand();
            string text = "SELECT * FROM BankUser Where NRIC = @select";
            Regex email = new Regex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}");
            Regex ph = new Regex(@"[0-9]{8}");
            if (email.IsMatch(pattern))
            {
                text = "SELECT * FROM BankUser Where Email = @select";
            }else if (ph.IsMatch(pattern))
            {
                text = "SELECT * FROM BankUser Where Phone = @select";
            }
            cmd.CommandText = @""+text;
            cmd.Parameters.AddWithValue("@select", pattern);
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    user = new BankUser
                    {
                        Nric = reader.GetString(0),
                        Email = reader.GetString(1),
                        Phone = reader.GetString(2),
                        Name = reader.GetString(3),
                        TransLimit = reader.GetDecimal(4),
                        Password = reader.GetString(5),
                    };
                }
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return user;
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
        public int? GetUserChatID(string nric)
        {
            int? chatID = null;
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM BankUser Where NRIC = @select";
            cmd.Parameters.AddWithValue("@select", nric);
            conn.Open();
            //Execute the SELECT SQL through a DataReader
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    chatID = !reader.IsDBNull(6) ? reader.GetInt32(6) : (int?)null;
                }
            }
            //Close DataReader
            reader.Close();
            //Close the database connection
            conn.Close();
            return chatID;
        }
        public bool UpdateUserChatID(int chatID,string nric)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE BankUser SET ChatID=@chatid WHERE NRIC = @nric";
            cmd.Parameters.AddWithValue("@chatid", chatID);
            cmd.Parameters.AddWithValue("@nric", nric);
            conn.Open();
            int count = cmd.ExecuteNonQuery();
            return true;
        }
    }
}
