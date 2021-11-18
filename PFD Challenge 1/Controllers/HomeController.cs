using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PFD_Challenge_1.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PFD_Challenge_1.DAL;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using PFD_Challenge_1.Listeners;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using PFD_Challenge_1.TelegramModel;
using Newtonsoft.Json;
using System.Text;

namespace PFD_Challenge_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        BankAccountDAL bankAccountContext = new BankAccountDAL();
        BankUserDAL bankUserContext = new BankUserDAL();
        FutureTransferDAL futureTransferContext = new FutureTransferDAL();
        TransactionDAL transactionContext = new TransactionDAL();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //JobScheduler.Start();
            //JobScheduler.FutureScanJob();
            return View();
        }
        public IActionResult Login(IFormCollection formData)
        {
            string userLogin= formData["Username"];
            string pass = formData["password"];
            bool result =bankUserContext.AuthenticateUser(userLogin, pass);
            if(!result)
            {
                return RedirectToAction("Login");
            }
            else
            {
                BankUser bu = bankUserContext.GetBankUser(userLogin);
                HttpContext.Session.SetString("NRIC", bu.Nric);
                if (bankUserContext.GetUserChatID(bu.Nric) == null)
                {
                    HttpContext.Session.SetString("TelegramSetUp", "true");
                    return RedirectToAction("Index", "Telegram");
                }
                else
                {
                    HttpContext.Session.SetString("TelegramSetUp", "false");
                    //To change to 2FA
                    return RedirectToAction("Authentication");
                }
            }

        }
        public async Task<IActionResult> AuthenticationAsync()
        {
            if (HttpContext.Session.GetString("NRIC") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int chat_id = bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")).Value;
            DateTime now = DateTime.Now;
            Random rand = new Random();
            string digit_7 = rand.Next(1000000, 9999999).ToString();
            ViewData["Code"] = digit_7;
            HttpContext.Session.SetString("Code", digit_7);
            HttpContext.Session.SetString("Time", now.ToString());
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.telegram.org");
            int chatID = bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")).Value;
            Notification newNotification = new Notification
            {
                chat_id = chatID,
                text = "Your code is " + digit_7,
            };
            string json = JsonConvert.SerializeObject(newNotification);
            StringContent notificationContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("/bot2113305321:AAEX37w64aTAImIvrqmAO6yF1gQO4eG7-ws/sendMessage", notificationContent);
            if (response.IsSuccessStatusCode)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult Authentication(IFormCollection formData)
        {
            if (HttpContext.Session.GetString("NRIC") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            DateTime started = Convert.ToDateTime(HttpContext.Session.GetString("Time"));
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan dtNow = started.Subtract(dt);
            int seconds = Convert.ToInt32(dtNow.TotalSeconds);
            DateTime now = DateTime.Now;
            TimeSpan nowDiff = now.Subtract(dt);
            int secondsPassed = Convert.ToInt32(nowDiff.TotalSeconds);
            string code = HttpContext.Session.GetString("Code");
            if (secondsPassed - seconds>= 0 && secondsPassed - seconds <= 60)
            {
                if (code == formData["Code"])
                {
                    HttpContext.Session.SetString("Login", "true");
                    return RedirectToAction("FundTransfer", "FundTransfer");
                }
                else
                {
                    return RedirectToAction("LogOut");
                }
            }
            return RedirectToAction("LogOut");
        }
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
