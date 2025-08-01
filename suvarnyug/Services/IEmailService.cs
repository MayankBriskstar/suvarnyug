using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using suvarnyug.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace suvarnyug.Services
{
    public interface IEmailService
    {
        bool SendEmail1(string toEmail, string ccEmail, string bccEmail, string subject, string body);
    }
}
