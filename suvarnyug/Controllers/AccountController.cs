using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suvarnyug.Data;
using Suvarnyug.Models;
using System.Security.Claims;
using suvarnyug.Models;
using Microsoft.AspNetCore.SignalR;
using System.Net.Mail;
using System.Net;
using AspNetCore.ReCaptcha;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;
using suvarnyug.Services;
using Microsoft.AspNetCore.Authorization;
namespace Suvarnyug.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IReCaptchaService _recaptcha;
        private readonly IEmailService _mail;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AccountController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IReCaptchaService recaptcha, IEmailService mail, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _hubContext = hubContext;
            _recaptcha = recaptcha;
            _mail = mail;
            _configuration = configuration;
            _environment = environment;
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("dashboard", "home");
            }
            ViewBag.Countries = await _context.Countries.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, IFormFile ProfilePhoto)
        {
            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            var recaptchaResult = await _recaptcha.GetVerifyResponseAsync(recaptchaResponse);

            if (!recaptchaResult.Success)
            {
                ModelState.AddModelError("", "Captcha validation failed. Please try again.");
            }
            bool emailExists = _context.Users.Any(u => u.Email == user.Email);
            bool mobileExists = _context.Users.Any(u => u.MobileNo == user.MobileNo);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }
            if (mobileExists)
            {
                ModelState.AddModelError("MobileNo", "Mobile number already exists.");
            }
            ModelState.Remove("ProfilePhoto");
            ModelState.Remove("Country");
            ModelState.Remove("");
            ModelState.Remove("UserLogin");
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation errors occurred.",
                    errors = ModelState.ToDictionary(
                        k => k.Key,
                        k => k.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profilePhotos/", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(stream);
                }

                user.ProfilePhotoPath = "/images/profilePhotos/" + fileName;
            }
            else
            {
                user.ProfilePhotoPath = "/images/default-profile.jpg";
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            var otp = GenerateOTP();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("TempUserEmail", user.Email);
            HttpContext.Session.SetString("TempUserData", Newtonsoft.Json.JsonConvert.SerializeObject(user));

            SendOTPEmail(user.Email, otp);

            return Json(new { success = true, message = "OTP sent to your email." });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string enteredOTP)
        {
            var sessionOTP = HttpContext.Session.GetString("OTP");
            var tempUserEmail = HttpContext.Session.GetString("TempUserEmail");
            var tempUserData = HttpContext.Session.GetString("TempUserData");

            if (sessionOTP == enteredOTP)
            {
                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(tempUserData);
                user.CreatedAt = DateTime.Now;
                user.IsActive = false;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                SendRegistrationEmail(user);

                var notification = new Notification
                {
                    Message = $"New user registered:<br>{user.FirstName} {user.LastName}<br>(ID: {user.UserId})",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UserId = user.UserId
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Message);

                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("TempUserEmail");
                HttpContext.Session.Remove("TempUserData");

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Invalid OTP. Please try again." });
            }
        }

        private void SendOTPEmail(string email, string otp)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var fromAddress = _configuration["Email:FromAddress"];
            var fromPassword = _configuration["Email:FromPassword"];

            var fromMailAddress = new MailAddress(fromAddress, "Suvarnyug");
            var toMailAddress = new MailAddress(email, "User");
            const string subject = "OTP Verification Code";

            string templatePath = Path.Combine(_environment.WebRootPath, "template", "OTP.html");
            string emailTemplate = System.IO.File.ReadAllText(templatePath);
            emailTemplate = emailTemplate.Replace("[OTP]", otp);

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

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        private void SendRegistrationEmail(User user)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var fromAddress = _configuration["Email:FromAddress"];
            var fromPassword = _configuration["Email:FromPassword"];

            var fromMailAddress = new MailAddress(fromAddress, "Suvarnyug");
            var toMailAddress = new MailAddress(user.Email, $"{user.FirstName} {user.LastName}");
            const string subject = "Welcome to Suvarnyug!";

            string templatePath = Path.Combine(_environment.WebRootPath, "template", "Register.html");
            string emailTemplate = System.IO.File.ReadAllText(templatePath);
            emailTemplate = emailTemplate.Replace("[FirstName]", user.FirstName)
                                         .Replace("[LastName]", user.LastName)
                                         .Replace("[Email]", user.Email)
                                         .Replace("[PhoneNumber]", user.MobileNo);

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

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("dashboard", "home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || user.IsActive || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid Email and Password.");
                return BadRequest(ModelState);
            }
            var userLogin = new UserLogin
            {
                UserId = user.UserId,
                LoginTime = DateTime.Now
            };
            _context.UserLogins.Add(userLogin);
            await _context.SaveChangesAsync();
            var claims = new List<Claim>
            {
            new (ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new (ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.Role, user.Role),
            new Claim("ProfilePhotoPath", user.ProfilePhotoPath ?? "/images/default-profile.jpg"),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return Json(new { redirectUrl = Url.Action("dashboard", "home") });
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "This Email Is Not Registered.");
                return View(model);
            }
            if (user.IsActive)
            {
                ModelState.AddModelError("Email", "This Email is inactive. Please contact support.");
                return View(model);
            }
            StringBuilder sbMailBody = new StringBuilder();
            sbMailBody.Append(Utils.GetDataFromFile(_environment.WebRootFileProvider.GetFileInfo("template/forgotpassword.html").PhysicalPath));
            sbMailBody.Replace("[FirstName]", user.FirstName.ToString());
            sbMailBody.Replace("[LastName]", user.LastName.ToString());
            sbMailBody.Replace("[Password]", user.ConfirmPassword.ToString());
            bool emailSent = _mail.SendEmail1(user.Email.ToString(), "", "", "Forgot Password", sbMailBody.ToString());

            if (emailSent)
            {
                ViewBag.Message = $"Your password has been sent to your registered email address, {user.Email} Check your inbox!";
            }
            else
            {
                ModelState.AddModelError("", "There was an issue sending the email. Please try again.");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (loggedInUserId != null && id.ToString() == loggedInUserId)
            {
                return Json(new { success = false, message = "You cannot delete the logged-in user." });
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsActive = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Old password does not match.");
                return View(model);
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ConfirmPassword = model.NewPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Countries = await _context.Countries.ToListAsync();
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(User user, IFormFile ProfilePhoto)
        {

            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ProfilePhoto.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profilePhotos/", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(stream);
                }

                existingUser.ProfilePhotoPath = "/images/profilePhotos/" + fileName;
            }
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.MobileNo = user.MobileNo;
            existingUser.CountryId = user.CountryId;
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, existingUser.FirstName + " " + existingUser.LastName),
                new Claim(ClaimTypes.Email, existingUser.Email),
                new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString()),
                new Claim("ProfilePhotoPath", existingUser.ProfilePhotoPath ?? string.Empty)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Profile updated successfully.";

            return RedirectToAction("index", "home");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> VerifyAadhaar(string aadhaarNumber)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (userIdClaim == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    int userId = int.Parse(userIdClaim.Value);

        //    // Validate Aadhaar number format (Basic regex for 12-digit check)
        //    if (!Regex.IsMatch(aadhaarNumber, @"^\d{12}$"))
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid Aadhaar number format.");
        //        return View();
        //    }

        //    // Check if the Aadhaar number is already registered
        //    var existingVerification = await _context.UserVerification
        //        .FirstOrDefaultAsync(v => v.AadhaarNumber == aadhaarNumber);

        //    if (existingVerification != null)
        //    {
        //        ModelState.AddModelError(string.Empty, "This Aadhaar number is already in use.");
        //        return View();
        //    }

        //    // Simulating Date of Birth Fetch (Since there's no free Aadhaar API)
        //    DateTime? extractedDob = SimulateDOBRetrieval(aadhaarNumber);

        //    if (extractedDob == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "Could not verify Aadhaar details. Try again.");
        //        return View();
        //    }

        //    int age = CalculateAge(extractedDob.Value);
        //    if (age < 18)
        //    {
        //        ModelState.AddModelError(string.Empty, "You are not eligible as you are below 18 years old.");
        //        return View();
        //    }

        //    var userVerification = new UserVerification
        //    {
        //        UserId = userId,
        //        AadhaarNumber = aadhaarNumber,
        //        DateOfBirth = extractedDob.Value,
        //        IsVerified = true
        //    };

        //    _context.UserVerification.Add(userVerification);
        //    var user = await _context.Users.FindAsync(userId);
        //    if (user != null)
        //    {
        //        user.IsVerified = true;
        //    }

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("CreateProfile", "Matrimonial");
        //}

        //private DateTime? SimulateDOBRetrieval(string aadhaarNumber)
        //{
        //    // Simulated logic: The last two digits of Aadhaar determine birth year (just for testing)
        //    if (aadhaarNumber.Length == 12)
        //    {
        //        int year = 1900 + int.Parse(aadhaarNumber.Substring(10, 2));
        //        DateTime dob = new DateTime(year, 1, 1);
        //        return dob;
        //    }
        //    return null;
        //}

        //private int CalculateAge(DateTime dob)
        //{
        //    int age = DateTime.Today.Year - dob.Year;
        //    if (dob.Date > DateTime.Today.AddYears(-age)) age--;
        //    return age;
        //}
    }
}
