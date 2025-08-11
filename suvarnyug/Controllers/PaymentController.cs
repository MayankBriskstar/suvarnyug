using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using suvarnyug.Models;
using Suvarnyug.Data;
using Suvarnyug.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using static QRCoder.PayloadGenerator.SwissQrCode.Iban;
using suvarnyug.Services;

namespace suvarnyug.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _mail;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public PaymentController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IEmailService mail, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _mail = mail;
            _configuration = configuration;
            _environment = environment;
        }

        [Authorize]
        public IActionResult SubscriptionDetails()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);

            var subscription = _context.Subscriptions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            if (subscription == null)
            {
                ViewData["IsSubscribed"] = false;
                return View();
            }

            ViewData["IsSubscribed"] = true;
            ViewData["CurrentPlan"] = subscription.PlanType.ToString();
            return View(subscription);
        }

        [Authorize]
        public IActionResult Subscribe(string planType)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("login", "account");
            }

            var userId = int.Parse(userIdClaim.Value);

            var existingSubscription = _context.Subscriptions
                .FirstOrDefault(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.Now);

            var planPrices = new Dictionary<string, decimal>
            {
                { "Premium", 1999 }
            };

            if (!planPrices.ContainsKey(planType))
            {
                return RedirectToAction("SubscriptionDetails");
            }

            decimal newPlanPrice = planPrices[planType];

            if (existingSubscription != null)
            {
                decimal currentPlanPrice = planPrices[existingSubscription.PlanType.ToString()];

                if (newPlanPrice <= currentPlanPrice)
                {
                    return Json(new { success = false, message = "You are already on an equal or higher plan." });
                }

                decimal upgradeCost = newPlanPrice - currentPlanPrice;

                var transactionReference = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("TransactionReference", transactionReference);

                ViewBag.PlanType = planType;
                ViewBag.Amount = upgradeCost;

                var paymentUrl = $"upi://pay?pa=sanghanimayank143-1@okhdfcbank&pn=Suvarnyug&am={upgradeCost}&cu=INR&tr={transactionReference}";

                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(paymentUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                ViewBag.QrCodeImage = "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);

                return View();
            }

            var transactionRef = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("TransactionReference", transactionRef);

            ViewBag.PlanType = planType;
            ViewBag.Amount = newPlanPrice;

            var newPaymentUrl = $"upi://pay?pa=sanghanimayank143-1@okhdfcbank&pn=Suvarnyug&am={newPlanPrice}&cu=INR&tr={transactionRef}";

            using var newQrGenerator = new QRCodeGenerator();
            var newQrCodeData = newQrGenerator.CreateQrCode(newPaymentUrl, QRCodeGenerator.ECCLevel.Q);
            using var newQrCode = new PngByteQRCode(newQrCodeData);
            var newQrCodeImage = newQrCode.GetGraphic(20);

            ViewBag.QrCodeImage = "data:image/png;base64," + Convert.ToBase64String(newQrCodeImage);

            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ProcessPayment([FromBody] JsonElement data)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var userId = int.Parse(userIdClaim.Value);
            if (!data.TryGetProperty("planType", out JsonElement planTypeElement))
            {
                return Json(new { success = false, message = "Invalid request data." });
            }
            string planType = planTypeElement.GetString();

            var transactionReference = HttpContext.Session.GetString("TransactionReference");
            if (string.IsNullOrEmpty(transactionReference) || !VerifyPayment(transactionReference))
            {
                return Json(new { success = false, message = "Payment failed. Try again." });
            }
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }
            var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == userId);

            if (subscription != null)
            {
                subscription.StartDate = DateTime.Now;
                subscription.EndDate = DateTime.Now.AddYears(1);
                subscription.IsActive = true;
                subscription.PaymentStatus = "Completed";
                subscription.PlanType = Enum.Parse<PlanType>(planType);
                _context.Subscriptions.Update(subscription);
            }
            else
            {
                subscription = new Subscription
                {
                    UserId = userId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    IsActive = true,
                    PaymentStatus = "Completed",
                    PlanType = Enum.Parse<PlanType>(planType)
                };

                _context.Subscriptions.Add(subscription);
            }

            _context.SaveChanges();
            SendSubscriptionEmail(user, subscription);
            return Json(new { success = true, message = "Subscription successful!", redirectUrl = "/Payment/SubscriptionDetails" });
        }

        private bool VerifyPayment(string transactionReference)
        {
            return true;
        }

        [Authorize]
        public IActionResult DirectPayment()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitDirectPayment(IFormFile PaymentScreenshot, string UserName, string Email, string MobileNo, string Description)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest("Invalid User ID.");
            }

            if (PaymentScreenshot == null || PaymentScreenshot.Length == 0)
            {
                ModelState.AddModelError("", "Payment Screenshot is required.");
                return View("DirectPayment");
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "payment");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(PaymentScreenshot.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await PaymentScreenshot.CopyToAsync(stream);
            }

            var payment = new DirectPayment
            {
                UserId = userId,
                UserName = UserName,
                Email = Email,
                MobileNo = MobileNo,
                Description = Description,
                PaymentScreenshot = "/images/payment/" + fileName,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.DirectPayments.Add(payment);
            await _context.SaveChangesAsync();
            SendDirectPaymentEmail(payment);
            return RedirectToAction("dashboard", "home");
        }

        private void SendSubscriptionEmail(User user, Subscription subscription)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var fromAddress = _configuration["Email:FromAddress"];
            var fromPassword = _configuration["Email:FromPassword"];

            var fromMailAddress = new MailAddress(fromAddress, "Suvarnyug");
            var toMailAddress = new MailAddress(user.Email, $"{user.FirstName} {user.LastName}");
            const string subject = "Subscription Confirmation - Suvarnyug";

            string templatePath = Path.Combine(_environment.WebRootPath, "template", "Subscription.html");
            string emailTemplate = System.IO.File.ReadAllText(templatePath);
            emailTemplate = emailTemplate.Replace("[FirstName]", user.FirstName)
                                          .Replace("[LastName]", user.LastName)
                                          .Replace("[PlanType]", subscription.PlanType.ToString())
                                          .Replace("[StartDate]", subscription.StartDate.ToString("yyyy-MM-dd"))
                                          .Replace("[EndDate]", subscription.EndDate.ToString("yyyy-MM-dd"))
                                          .Replace("[PaymentStatus]", subscription.PaymentStatus);

            var smtp = new SmtpClient
            {
                Host = smtpServer,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromMailAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromMailAddress, toMailAddress)
            {
                Subject = subject,
                Body = emailTemplate,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
        private void SendDirectPaymentEmail(DirectPayment payment)
        {
            var fromAddress = new MailAddress(payment.Email, payment.UserName);
            var toAddress = new MailAddress("mayanks.briskstar@gmail.com", "Admin");
            const string fromPassword = "lrym mtlk ypbm jvas";
            const string subject = "New Direct Payment Submission";

            string templatePath = Path.Combine(_environment.WebRootPath, "template", "DirectPayment.html");
            string emailTemplate = System.IO.File.ReadAllText(templatePath);

            emailTemplate = emailTemplate.Replace("[UserName]", payment.UserName)
                                         .Replace("[Email]", payment.Email)
                                         .Replace("[MobileNo]", payment.MobileNo)
                                         .Replace("[Description]", payment.Description)
                                         .Replace("[PaymentScreenshot]", "http://192.168.1.79/" + payment.PaymentScreenshot);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mayanks.briskstar@gmail.com", fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = emailTemplate,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
}