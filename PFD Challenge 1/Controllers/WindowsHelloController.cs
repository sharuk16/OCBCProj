using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PFD_Challenge_1.DAL;
using PFD_Challenge_1.Models;

namespace PFD_Challenge_1.Controllers
{
    public class WindowsHelloController : Controller
    {
        BankUserDAL bankUserContext = new BankUserDAL();
        private HttpClient _httpClient;
        private readonly static string API_SECRET = "Xiang:secret:3143a81074c24036b3aae94e2ff231c9";

        public WindowsHelloController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://apiv2.passwordless.dev/");
            _httpClient.DefaultRequestHeaders.Add("ApiSecret", API_SECRET);

            if (API_SECRET == "YOUR_API_SECRET") { throw new Exception("Please set your API SECRET"); }

        }
        //JS post to this action to register user
        [HttpGet("/createtoken")]
        public async Task<ActionResult<string>> createtoken()
        {
            string userId = HttpContext.Session.GetString("NRIC");
            BankUser bu = bankUserContext.GetBankUser(userId);
            var json = JsonSerializer.Serialize(new
            {
                userId = userId,
                username = bu.Nric,
                DisplayName = bu.Name
            }) ;
            var request = await _httpClient.PostAsync("register/token", new StringContent(json, Encoding.UTF8, "application/json"));
            request.EnsureSuccessStatusCode();
            var token = await request.Content.ReadAsStringAsync();
            return token;
        }

        [HttpGet]
        [Route("/verify-signin")]
        public async Task<ActionResult<string>> VerifySignInToken(string token)
        {

            // check if the fido2 authentication was valid and for what user
            var json = JsonSerializer.Serialize(new
            {
                token
            });

            var request = await _httpClient.PostAsync("signin/verify", new StringContent(json, Encoding.UTF8, "application/json"));
            request.EnsureSuccessStatusCode();
            var response = await request.Content.ReadAsStringAsync();
            var signin = JsonSerializer.Deserialize<SignInDto>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Response r = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(response);
            if (signin.Success == true)
            {
                // Sign in the user, set cookies etc
                BankUser bu = bankUserContext.GetBankUser(r.userId);
                //if person does not exists in the database
                if (bu == null)
                {
                }
                else
                {
                    HttpContext.Session.SetString("NRIC",r.userId);
                    HttpContext.Session.SetString("TelegramSetUp", "false");
                    HttpContext.Session.SetString("Login", "true");
                }
                
            }
            return "true";
        }
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("NRIC") == null
                ||
                HttpContext.Session.GetString("NRIC")=="")
            {
                return RedirectToAction("Index", "Home");
            }
            string nric = HttpContext.Session.GetString("NRIC");
            bool windowsEnabled = bankUserContext.WindowsHelloExists(nric);
            if (windowsEnabled)
            {
                HttpContext.Session.SetString("windowsHello", "1");
            }
            else
            {
                HttpContext.Session.SetString("windowsHello", "0");
            }
            return View();
        }
        [HttpGet]
        [Route("/update")]
        public async Task<ActionResult<string>> update()
        {
            if (HttpContext.Session.GetString("NRIC") == null || HttpContext.Session.GetString("NRIC") == "")
            {
                return RedirectToAction("LogOut", "Home");
            }
            string userId = HttpContext.Session.GetString("NRIC");
            var jsonreq = JsonSerializer.Serialize(new
            {
                userId = userId,
            });

            var secondrequest = await _httpClient.PostAsync("credentials/list", new StringContent(jsonreq, Encoding.UTF8, "application/json"));
            secondrequest.EnsureSuccessStatusCode();
            var get = await secondrequest.Content.ReadAsStringAsync();
            List<CredentialList> cl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CredentialList>>(get);
            CredentialList a = cl[0];
            HttpContext.Session.SetString("windowsHello", "1");
            bool success = bankUserContext.UpdateWindowsHello(userId, a.descriptor.id);
            return "true";
        }
        [HttpGet]
        [Route("/delete")]
        public async Task<ActionResult<string>> DeleteWindowsHelloAsync()
        {

            string nric = HttpContext.Session.GetString("NRIC");
            string userId = bankUserContext.GetWindowsHello(nric);
            var json = JsonSerializer.Serialize(new
            {
                CredentialId = userId,
            });
            var request = await _httpClient.PostAsync("credentials/delete", new StringContent(json, Encoding.UTF8, "application/json"));
            request.EnsureSuccessStatusCode();
            bool update = bankUserContext.UpdateWindowsHello(nric, "");
            if (update)
            {
                HttpContext.Session.SetString("windowsHello", "0");
                ViewData["Message"] = "Sucessfully disable Windows Hello!";
            }
            else
            {
                ViewData["Message"] = "Windows Hello is not disabled";
            }
            return "true";
        }

        public class SignInDto
        {
            public SignInDto()
            {

            }
            public bool Success { get; set; }

            public string UserId { get; set; }
            public DateTime Timestamp { get; set; }
            public string RPID { get; set; }
            public string Origin { get; set; }
            public string Device { get; set; }
            public string Country { get; set; }
            public string Nickname { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
