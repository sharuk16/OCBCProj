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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

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
            return View();
        }
        public IActionResult faceLogin(int id)
        {
            if (id > 28000 && id < 32000)
            {
                HttpContext.Session.SetString("NRIC", "T1234567A");
                if (bankUserContext.GetUserChatID(HttpContext.Session.GetString("NRIC")) != null)
                {
                    HttpContext.Session.SetString("TelegramChatID", "true");
                }
                else
                {
                    HttpContext.Session.SetString("TelegramChatID", "false");
                }
                return RedirectToAction("FundTransfer", "FundTransfer");
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task LogOut()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        public IActionResult LogIn()
        {
            HttpContext.Session.SetString("NRIC", "T11223344A");
            return RedirectToAction("FundTransfer", "FundTransfer");
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
