using System;
using PFD_Challenge_1.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PFD_Challenge_1.Controllers
{
    public class MailController : Controller
    {

        static async Task SendEmail()
        {

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("joelong161@gmail.com", "yay User");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("joelong161@gmail.com", "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}