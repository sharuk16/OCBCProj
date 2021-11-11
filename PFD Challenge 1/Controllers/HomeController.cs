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

namespace PFD_Challenge_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

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
            if (id > 28000 && id < 31000)
            {
                HttpContext.Session.SetString("NRIC", "T1234567A");
                return RedirectToAction("FundTransfer", "FundTransfer");
            }
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
