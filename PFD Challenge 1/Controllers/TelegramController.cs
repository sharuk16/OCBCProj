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
            if (HttpContext.Session.GetString("NRIC") == null|| HttpContext.Session.GetString("TelegramChatID")=="true")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public async Task<ActionResult> RegisterUser()
        {
            if (HttpContext.Session.GetString("NRIC") == null || HttpContext.Session.GetString("TelegramChatID") == "true")
            {
                return RedirectToAction("Index", "Home");
            }
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/getUpdates");
            HttpResponseMessage response = await client.GetAsync("/api/books");
            if (response.IsSuccessStatusCode)
            {
                int? chatID=null;
                string data = await response.Content.ReadAsStringAsync();
                Root result = JsonConvert.DeserializeObject<Root>(data);
                List<Message> messageList =new List<Message>();
                foreach(Result r in result.result)
                {
                    if(r.message.text == HttpContext.Session.GetString("NRIC")){
                        chatID= r.message.chat.id;
                        break;
                    }
                }
                if(chatID != null)
                {
                    bool updateResult = bankUserContext.UpdateUserChatID(chatID.Value, HttpContext.Session.GetString("NRIC"));
                    if (updateResult)
                    {
                        HttpContext.Session.SetString("TelegramChatID", "false");
                        Notification newNotification = new Notification
                        {
                            Chat_id = chatID.Value,
                            Text = "Notifications of your transaction will be sent to you.",
                        };
                        HttpClient client2 = new HttpClient();
                        client2.BaseAddress = new Uri("https://api.telegram.org/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws");
                        string json = JsonConvert.SerializeObject(newNotification);
                        StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                        HttpResponseMessage response2 = await client.PostAsync("/sendMessage", notificationContent);
                        //need handle response?
                        ViewData["Message"] = "Success";
                        return View();
                    }
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
