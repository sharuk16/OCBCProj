using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PFD_Challenge_1.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PFD_Challenge_1.TelegramModel;
using Newtonsoft.Json;
using System.Text;

namespace PFD_Challenge_1.Controllers
{
    public class TelegramController : Controller
    {
        BankAccountDAL bankAccountContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        FutureTransferDAL futureTransferContext = new FutureTransferDAL();
        TransactionDAL transactionContext = new TransactionDAL();
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("NRIC") == null|| HttpContext.Session.GetString("TelegramSetUp") =="false")
            {
                return RedirectToAction("Index", "Home");
            }
            DateTime now = DateTime.Now;
            Random rand = new Random();
            string digit_7 = rand.Next(1000000,9999999).ToString();
            ViewData["Code"] = digit_7;
            HttpContext.Session.SetString("Code", digit_7);
            HttpContext.Session.SetString("Time", now.ToString());
            return View();
        }
        public async Task<ActionResult> RegisterUser()
        {
            if (HttpContext.Session.GetString("NRIC") == null || HttpContext.Session.GetString("TelegramSetUp") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            string code = HttpContext.Session.GetString("Code");
            HttpContext.Session.SetString("Code", "");
            DateTime started =Convert.ToDateTime(HttpContext.Session.GetString("Time"));
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan dtNow = started.Subtract(dt);
            int seconds = Convert.ToInt32(dtNow.TotalSeconds);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org");
            HttpResponseMessage response = await client.GetAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/getUpdates");
            bool toolong = false;
            if (response.IsSuccessStatusCode)
            {
                int? chatID = null;
                string data = await response.Content.ReadAsStringAsync();
                Root result = JsonConvert.DeserializeObject<Root>(data);
                result.result.Reverse();
                foreach(Result r in result.result)
                {
                    if (r.message.text == code) 
                    {
                        long messageCapture = r.message.date + 8*60*60;
                        long endtime = messageCapture-seconds;
                        //messageCapture should be later than time accessed to the page
                        //endtime should be within duration
                        if (endtime >= 0 && endtime <= 60)
                        { 
                            chatID = r.message.chat.id;
                        }
                        else
                        {
                            chatID = null;
                            toolong = true;
                        }
                        break;
                    }
                }
                if(chatID != null)
                {
                    HttpContext.Session.SetString("TelegramSetUp", "false");
                    bool updateResult = bankUserContext.UpdateUserChatID(chatID.Value, HttpContext.Session.GetString("NRIC"));
                    if (updateResult)
                    {
                        HttpContext.Session.SetString("TelegramChatID", "true");
                        Notification newNotification = new Notification
                        {
                            chat_id = chatID.Value,
                            text = "Notifications of your transaction will be sent to you.",
                        };
                        string json = JsonConvert.SerializeObject(newNotification);
                        StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                        response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
                        if (response.IsSuccessStatusCode)
                        {
                            ViewData["Message"] = "Successfully added telegram notification.";
                            return View();
                        }
                        else
                        {
                            ViewData["Message"] = "Sucessfully added telegram notification. Failed to send message.";
                            return View();
                        }
                    }
                }
                if (toolong)
                {
                    ViewData["Message"] = "Took too long to respond.";
                    return View();
                }
                ViewData["Message"] = "Unable to retrieve chat_ID";
                return View();
            }
            else
            {
                ViewData["Message"] = "An Error Occured";
                return View();
            }
        }
    }
}
