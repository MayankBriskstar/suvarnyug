using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using suvarnyug.Models;
using Suvarnyug.Data;
using Microsoft.EntityFrameworkCore;
using Suvarnyug.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Net;
using suvarnyug.Services;
using static QRCoder.PayloadGenerator;
using System.Configuration;
using System;
namespace suvarnyug.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _mail;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public AdminController(ApplicationDbContext context, IEmailService mail, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _mail = mail;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            int totalBiodata = _context.Biodata.Count();
            int totalUsers = _context.Users.Count();
            int totalForums = _context.Forums.Count();
            int totalSubscribedUsers = _context.Subscriptions.Count(s => s.IsActive);

            var usersYearlyCounts = GetUsersYearlyCounts(_context.Users);
            var forumsYearlyCounts = GetForumsYearlyCounts(_context.Forums);
            var biodataYearlyCounts = GetBiodataYearlyCounts(_context.Biodata);
            var subscribedUsersYearlyCounts = GetSubscribedUsersYearlyCounts(_context.Subscriptions);
            var latestUsers = _context.Users.Include(u => u.Country).OrderByDescending(u => u.CreatedAt).Take(5).ToList();
            var notifications = _context.Notifications.Where(n => !n.IsRead).OrderByDescending(n => n.UserId).ToList();

            var model = new AdminViewModel
            {
                Notifications = notifications,
                NotificationCount = notifications.Count,
            };
            ViewBag.LatestUsers = latestUsers;
            ViewData["TotalBiodata"] = totalBiodata;
            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalForums"] = totalForums;
            ViewData["TotalSubscribedUsers"] = totalSubscribedUsers;
            ViewBag.UsersYearlyCounts = usersYearlyCounts;
            ViewBag.ForumsYearlyCounts = forumsYearlyCounts;
            ViewBag.BiodataYearlyCounts = biodataYearlyCounts;
            ViewBag.SubscribedUsersYearlyCounts = subscribedUsersYearlyCounts;
            return View();
        }
        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public IActionResult Index1()
        //{
        //    int totalBiodata = _context.Biodata.Count();
        //    int totalUsers = _context.Users.Count();
        //    int totalForums = _context.Forums.Count();
        //    int totalSubscribedUsers = _context.Subscriptions.Count(s => s.IsActive);

        //    var usersYearlyCounts = GetUsersYearlyCounts(_context.Users);
        //    var forumsYearlyCounts = GetForumsYearlyCounts(_context.Forums);
        //    var biodataYearlyCounts = GetBiodataYearlyCounts(_context.Biodata);
        //    var subscribedUsersYearlyCounts = GetSubscribedUsersYearlyCounts(_context.Subscriptions);
        //    var latestUsers = _context.Users.Include(u => u.Country).OrderByDescending(u => u.CreatedAt).Take(5).ToList();
        //    var notifications = _context.Notifications.Where(n => !n.IsRead).OrderByDescending(n => n.UserId).ToList();

        //    var model = new AdminViewModel
        //    {
        //        Notifications = notifications,
        //        NotificationCount = notifications.Count,
        //    };
        //    ViewData["TotalBiodata"] = totalBiodata;
        //    ViewData["TotalUsers"] = totalUsers;
        //    ViewData["TotalForums"] = totalForums;
        //    ViewData["TotalSubscribedUsers"] = totalSubscribedUsers;
        //    ViewBag.LatestUsers = latestUsers;

        //    ViewBag.UsersYearlyCounts = usersYearlyCounts;
        //    ViewBag.ForumsYearlyCounts = forumsYearlyCounts;
        //    ViewBag.BiodataYearlyCounts = biodataYearlyCounts;
        //    ViewBag.SubscribedUsersYearlyCounts = subscribedUsersYearlyCounts;

        //    return View(model);
        //}
        private Dictionary<int, int[]> GetUsersYearlyCounts(DbSet<User> dbSet)
        {
            var yearsWithData = dbSet.Select(u => u.CreatedAt.Year).Distinct().OrderByDescending(y => y).ToList();
            var yearlyCounts = new Dictionary<int, int[]>();

            foreach (var year in yearsWithData)
            {
                int[] monthlyCounts = new int[12];
                for (int month = 1; month <= 12; month++)
                {
                    var count = dbSet
                        .Where(u => u.CreatedAt.Year == year && u.CreatedAt.Month == month)
                        .Count();
                    monthlyCounts[month - 1] = count;
                }
                yearlyCounts[year] = monthlyCounts;
            }

            return yearlyCounts;
        }
        private Dictionary<int, int[]> GetForumsYearlyCounts(DbSet<Forum> dbSet)
        {
            var yearsWithData = dbSet.Select(f => f.CreatedDate.Year).Distinct().OrderByDescending(y => y).ToList();
            var yearlyCounts = new Dictionary<int, int[]>();

            foreach (var year in yearsWithData)
            {
                int[] monthlyCounts = new int[12];
                for (int month = 1; month <= 12; month++)
                {
                    var count = dbSet
                        .Where(f => f.CreatedDate.Year == year && f.CreatedDate.Month == month)
                        .Count();
                    monthlyCounts[month - 1] = count;
                }
                yearlyCounts[year] = monthlyCounts;
            }

            return yearlyCounts;
        }
        private Dictionary<int, int[]> GetBiodataYearlyCounts(DbSet<Biodata> dbSet)
        {
            var yearsWithData = dbSet.Select(b => b.CreatedAt.Year).Distinct().OrderByDescending(y => y).ToList();
            var yearlyCounts = new Dictionary<int, int[]>();

            foreach (var year in yearsWithData)
            {
                int[] monthlyCounts = new int[12];
                for (int month = 1; month <= 12; month++)
                {
                    var count = dbSet
                        .Where(b => b.CreatedAt.Year == year && b.CreatedAt.Month == month)
                        .Count();
                    monthlyCounts[month - 1] = count;
                }
                yearlyCounts[year] = monthlyCounts;
            }

            return yearlyCounts;
        }
        private Dictionary<int, int[]> GetSubscribedUsersYearlyCounts(DbSet<Subscription> dbSet)
        {
            var yearsWithData = dbSet
                .Where(s => s.IsActive && s.PaymentStatus == "Completed")
                .Select(s => s.StartDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            var yearlyCounts = new Dictionary<int, int[]>();

            foreach (var year in yearsWithData)
            {
                int[] monthlyCounts = new int[12];
                for (int month = 1; month <= 12; month++)
                {
                    var count = dbSet
                        .Where(s => s.StartDate.Year == year && s.StartDate.Month == month && s.IsActive && s.PaymentStatus == "Completed")
                        .Count();
                    monthlyCounts[month - 1] = count;
                }
                yearlyCounts[year] = monthlyCounts;
            }

            return yearlyCounts;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            var users = _context.Users.Include(u => u.Country).OrderByDescending(u => u.UserId).ToList();
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ViewUser(int id)
        {
            var user = _context.Users.Include(u => u.Country).FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult UserAccess()
        {
            var users = _context.Users.OrderByDescending(u => u.UserId).ToList();
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUserRole(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUserRole(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (user.Role == "Admin" && newRole != "Admin")
            {
                var adminCount = _context.Users.Count(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    return Json(new { success = false, message = "At least one admin must remain in the system." });
                }
            }

            user.Role = newRole;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult MarkAsRead()
        {
            var notifications = _context.Notifications.Where(n => !n.IsRead).ToList();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users.Include(u => u.Country).FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Countries = await _context.Countries.ToListAsync();
            if (userId == null)
            {
                return RedirectToAction("login", "account");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProfile(User user, IFormFile ProfilePhoto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingUser = await _context.Users.Include(u => u.Country).FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (existingUser == null || existingUser.Role != "Admin")
            {
                return Unauthorized();
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.MobileNo = user.MobileNo;
            existingUser.Email = user.Email;
            existingUser.CountryId = user.CountryId;

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profilePhotos/", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(stream);
                }

                existingUser.ProfilePhotoPath = "/images/profilePhotos/" + fileName;
            }
            else
            {
                user.ProfilePhotoPath = existingUser.ProfilePhotoPath;
            }
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("ProfilePhoto");
            ModelState.Remove("Country");
            ModelState.Remove("UserLogin");
            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var profilePhotoClaim = claimsIdentity.FindFirst("ProfilePhotoPath");

                if (profilePhotoClaim != null)
                {
                    claimsIdentity.RemoveClaim(profilePhotoClaim);
                }

                claimsIdentity.AddClaim(new Claim("ProfilePhotoPath", existingUser.ProfilePhotoPath));
                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("index", "home");
            }

            return View("myprofile", existingUser);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult MatrimonialManagement()
        {
            var biodataList = _context.Biodata.OrderByDescending(u => u.BiodataId).Include(u => u.Country).Where(u => u.VipBiodata== false).ToList();
            return View(biodataList);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult vipmatrimonialmanagement()
        {
            var biodataList = _context.Biodata.OrderByDescending(u => u.BiodataId).Include(u => u.Country).Where(u => u.VipBiodata == true).ToList();
            return View(biodataList);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBiodata(int id)
        {
            var biodata = await _context.Biodata.FindAsync(id);
            if (biodata == null)
            {
                return NotFound();
            }
            biodata.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewUserProfile(int id)
        {
            var biodataadmin = await _context.Biodata.Include(b => b.Images.OrderByDescending(i => i.IsDefault))
                .Include(b => b.Country).Include(b => b.State).Include(b => b.City).FirstOrDefaultAsync(b => b.BiodataId == id);
            if (biodataadmin == null)
            {
                return NotFound();
            }

            return View("ViewUserProfile", biodataadmin);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Forums()
        {
            var forums = await _context.Forums
               .Include(f => f.User)
               .Include(f => f.Replies)
                   .ThenInclude(r => r.User)
               .Include(f => f.Replies).ThenInclude(r => r.Replies)
               .Include(f => f.Replies).ThenInclude(r => r.Likes)
               .Include(f => f.ForumLikes)
               .OrderByDescending(f => f.CreatedDate)
               .ToListAsync();
            foreach (var forum in forums)
            {
                forum.Replies = forum.Replies.ToList();
            }
            return View(forums);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteForum(int forumId)
        {
            var forum = await _context.Forums.FirstOrDefaultAsync(f => f.ForumId == forumId);

            if (forum == null)
            {
                return NotFound();
            }

            forum.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("forums");
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteReply(int replyId)
        {
            var reply = await _context.Replies.FirstOrDefaultAsync(r => r.ReplyId == replyId);

            if (reply == null)
            {
                return NotFound();
            }

            reply.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("forums");
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
            return RedirectToAction("index", "admin");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Subscribed()
        {
            var subscriptions = _context.Subscriptions
           .Include(s => s.User)
           .ToList();

            return View(subscriptions);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult SubscribedDetails(int id)
        {
            var subscription = _context.Subscriptions
                .Include(s => s.User)
                .FirstOrDefault(s => s.SubscriptionId == id);

            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        #region Country State and City Management
        // ========================= Country MANAGEMENT =========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageCountries()
        {
            var countries = await _context.Countries.ToListAsync();
            return View(countries);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCountries([FromBody] CountryListModel model)
        {
            if (model == null || model.Countries == null) return BadRequest("Invalid data.");

            var existingCountries = await _context.Countries.ToListAsync();
            var updatedCountryIds = model.Countries.Select(c => c.CountryId).ToList();
            var countriesToDelete = existingCountries.Where(c => !updatedCountryIds.Contains(c.CountryId)).ToList();
            List<string> cannotDeleteCountries = new List<string>();

            foreach (var country in countriesToDelete)
            {
                bool isCountryUsed = await _context.Biodata.AnyAsync(b => b.CountryId == country.CountryId);
                if (isCountryUsed)
                {
                    cannotDeleteCountries.Add(country.CountryName);
                }
            }
            if (cannotDeleteCountries.Any())
            {
                string errorMessage = string.Join(", ", cannotDeleteCountries.Select(c => $"<strong style='color: #d9534f;'>{c}</strong>"));
                errorMessage += " <br>Cannot delete these countries because they are associated with existing Matrimonial Profiles records.";

                return BadRequest(errorMessage);
            }
            _context.Countries.RemoveRange(countriesToDelete);

            foreach (var country in model.Countries)
            {
                if (country.CountryId == 0)
                {
                    _context.Countries.Add(new Country { CountryName = country.CountryName });
                }
                else
                {
                    var existingCountry = await _context.Countries.FindAsync(country.CountryId);
                    if (existingCountry != null)
                    {
                        existingCountry.CountryName = country.CountryName;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Countries saved successfully!" });
        }
        public class CountryListModel
        {
            public List<Country> Countries { get; set; }
        }

        // ========================= STATE MANAGEMENT =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageStates()
        {
            var countries = await _context.Countries.Include(c => c.States).ToListAsync();
            return View(countries);
        }

        [HttpPost]
        public async Task<IActionResult> SaveStates([FromBody] StateListModel model)
        {
            if (model == null || model.States == null) return BadRequest("Invalid data.");

            var existingStates = await _context.States
                .Where(s => s.CountryId == model.States.First().CountryId)
                .ToListAsync();

            var updatedStateIds = model.States.Select(s => s.StateId).ToList();
            var statesToDelete = existingStates.Where(s => !updatedStateIds.Contains(s.StateId)).ToList();
            List<string> cannotDeleteStates = new List<string>();

            foreach (var state in statesToDelete)
            {
                bool isStateUsed = await _context.Biodata.AnyAsync(b => b.StateId == state.StateId);
                if (isStateUsed)
                {
                    cannotDeleteStates.Add(state.StateName);
                }
            }
            if (cannotDeleteStates.Any())
            {
                string errorMessage = string.Join(", ", cannotDeleteStates.Select(s => $"<strong style='color: #d9534f;'>{s}</strong>"));
                errorMessage += " <br>Cannot delete these States because they are associated with existing Matrimonial Profiles.";

                return BadRequest(errorMessage);
            }
            _context.States.RemoveRange(statesToDelete);

            foreach (var state in model.States)
            {
                if (state.StateId == 0)
                {
                    _context.States.Add(new State
                    {
                        CountryId = state.CountryId,
                        StateName = state.StateName
                    });
                }
                else
                {
                    var existingState = await _context.States.FindAsync(state.StateId);
                    if (existingState != null)
                    {
                        existingState.StateName = state.StateName;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "States saved successfully!" });
        }

        public class StateListModel
        {
            public List<State> States { get; set; }
        }

        [HttpGet("Admin/GetStates")]
        public async Task<IActionResult> GetAllStates()
        {
            var states = await _context.States.ToListAsync();
            return Ok(states);
        }

        // ========================= CITY MANAGEMENT =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageCities()
        {
            var countries = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .ToListAsync();
            return View(countries);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCities([FromBody] CityRequest request)
        {
            if (request == null || request.Cities == null) return BadRequest("Invalid data.");

            var existingCities = await _context.Cities
                .Where(c => request.Cities.Select(r => r.StateId).Contains(c.StateId))
                .ToListAsync();

            var updatedCityIds = request.Cities.Select(c => c.CityId).ToList();
            var citiesToDelete = existingCities.Where(c => !updatedCityIds.Contains(c.CityId)).ToList();
            List<string> cannotDeleteCities = new List<string>();
            foreach (var city in citiesToDelete)
            {
                bool isCityUsed = await _context.Biodata.AnyAsync(b => b.CityId == city.CityId);
                if (isCityUsed)
                {
                    cannotDeleteCities.Add(city.CityName);
                }
            }

            if (cannotDeleteCities.Any())
            {
                string errorMessage = string.Join(", ", cannotDeleteCities.Select(s => $"<strong style='color: #d9534f;'>{s}</strong>"));
                errorMessage += " <br>Cannot delete these City because they are associated with existing Matrimonial Profiles.";
                return BadRequest(errorMessage);
            }

            _context.Cities.RemoveRange(citiesToDelete);

            foreach (var city in request.Cities)
            {
                if (city.CityId == 0)
                {
                    _context.Cities.Add(new City { StateId = city.StateId, CityName = city.CityName });
                }
                else
                {
                    var existingCity = await _context.Cities.FindAsync(city.CityId);
                    if (existingCity != null)
                    {
                        existingCity.CityName = city.CityName;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cities saved successfully!" });
        }

        public class CityRequest
        {
            public List<City> Cities { get; set; }
        }

        

        // ========================= AJAX HELPERS FOR DROPDOWN =========================

        [HttpGet("Admin/GetStatesByCountry/{countryId}")]
        public async Task<IActionResult> GetStatesByCountry(int countryId)
        {
            var states = await _context.States
                .Where(s => s.CountryId == countryId)
                .Select(s => new { s.StateId, s.StateName })
                .ToListAsync();

            return Ok(states);
        }

        [HttpGet]
        public async Task<JsonResult> GetCitiesByState(int stateId)
        {
            var cities = await _context.Cities.Where(c => c.StateId == stateId).ToListAsync();
            return Json(cities);
        }
        #endregion

        #region Payment 
        [Authorize(Roles = "Admin")]
        public IActionResult DirectPaymentUsers()
        {
            var payments = _context.DirectPayments.OrderByDescending(u => u.CreatedAt).ToList();
            return View(payments);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult DirectPaymentView(int id)
        {
            var payment = _context.DirectPayments.Find(id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DirectPaymentEdit(int id)
        {
            var payment = _context.DirectPayments.Find(id);
            return View(payment);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult ApprovePayment(int PaymentId, string PlanType)
        {
            var payment = _context.DirectPayments.Find(PaymentId);
            if (payment == null) return NotFound();

            // Convert string to enum
            if (!Enum.TryParse(PlanType, out PlanType selectedPlan))
            {
                return BadRequest("Invalid Plan Type");
            }

            var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == payment.UserId);
            if (subscription == null)
            {
                subscription = new Subscription
                {
                    UserId = payment.UserId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    IsActive = true,
                    PaymentStatus = "Completed",
                    PlanType = selectedPlan
                };
                _context.Subscriptions.Add(subscription);
            }
            else
            {
                subscription.StartDate = DateTime.Now;
                subscription.EndDate = DateTime.Now.AddYears(1);
                subscription.IsActive = true;
                subscription.PaymentStatus = "Completed";
                subscription.PlanType = selectedPlan;
                _context.Subscriptions.Update(subscription);
            }

            payment.PaymentStatus = "Approved";
            _context.DirectPayments.Update(payment);
            _context.SaveChanges();
            var user = _context.Users.FirstOrDefault(u => u.UserId == payment.UserId);
            if (user != null)
            {
                SendApprovalEmail(user, subscription);
            }
            return RedirectToAction("DirectPaymentUsers");
        }

        private void SendApprovalEmail(User user, Subscription subscription)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var fromAddress = _configuration["Email:FromAddress"];
            var fromPassword = _configuration["Email:FromPassword"];

            var fromMailAddress = new MailAddress(fromAddress, "Suvarnyug");
            var toMailAddress = new MailAddress(user.Email, $"{user.FirstName} {user.LastName}");
            const string subject = "Subscription Approved - Suvarnyug";

            string templatePath = Path.Combine(_environment.WebRootPath, "template", "SubscriptionApproval.html");
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
        #endregion
    }
}
