using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suvarnyug.Models;
using Suvarnyug.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using suvarnyug.Models;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.HttpResults;
using AspNetCore.ReCaptcha;
using System.Net.Mail;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System;
using static System.Net.Mime.MediaTypeNames;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IReCaptchaService _recaptcha;
    private readonly IWebHostEnvironment _environment;
    public HomeController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IReCaptchaService recaptcha, IWebHostEnvironment environment)
    {
        _context = context;
        _hubContext = hubContext;
        _recaptcha = recaptcha;
        _environment = environment;
    }
    public async Task<IActionResult> Index(User user, IFormFile ProfilePhoto)
    {
        int totalBiodata = _context.Biodata.Count();
        int totalUsers = _context.Users.Count();
        int totalMaleBiodata = _context.Biodata.Count(b => b.Gender == "Male");
        int totalFemaleBiodata = _context.Biodata.Count(b => b.Gender == "Female");
        int nriCount = _context.Users.Count(u => u.Country.CountryId != 1);
        ViewBag.Countries = _context.Countries.ToList();
        var latestMale = _context.Biodata.Where(b => b.Gender == "Male" && !b.User.IsActive == true )
            .Select(b => new
            {
                b.FirstName,
                b.LastName,
                b.DOB,
                b.Education,
                b.MotherTongue,
                b.MaritalStatus,
                b.CreatedAt,
                b.IsDeleted,
                Image = b.Images.Where(i => i.IsDefault).Select(i => i.ImageUrl).FirstOrDefault() ?? b.Images.FirstOrDefault().ImageUrl ?? "/path/to/default/image.jpg",
                BiodataId = b.BiodataId
            }).Where(b => b.IsDeleted == false)
            .AsEnumerable().GroupBy(b => b.BiodataId).Select(g => g.OrderByDescending(b => b.CreatedAt).FirstOrDefault())
            .OrderByDescending(b => b.CreatedAt).Take(10).ToList();

        var latestFemale = _context.Biodata.Where(b => b.Gender == "Female" && !b.User.IsActive == true).Select(b => new
        {
            b.FirstName,
            b.LastName,
            b.DOB,
            b.MaritalStatus,
            b.Education,
            b.MotherTongue,
            b.CreatedAt,
            b.IsDeleted,
            Image = b.Images.Where(i => i.IsDefault).Select(i => i.ImageUrl).FirstOrDefault() ?? b.Images.FirstOrDefault().ImageUrl ?? "/path/to/default/image.jpg",
            BiodataId = b.BiodataId
        }).Where(b => b.IsDeleted == false)
            .AsEnumerable().GroupBy(b => new { b.BiodataId, b.FirstName, b.LastName, b.DOB, b.Education, b.MaritalStatus, b.CreatedAt })
            .Select(g => g.OrderByDescending(b => b.CreatedAt).FirstOrDefault())
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .ToList();

        var forums = await _context.Forums.Include(f => f.User).Include(f => f.Replies).Include(f => f.ForumLikes).Where(f => !f.IsDeleted).OrderByDescending(f => f.CreatedDate).Take(2)
            .Select(f => new
            {
                f.ForumId,
                f.Title,
                f.Description,
                f.CreatedDate,
                f.User.ProfilePhotoPath,
                Author = f.User.FirstName + " " + f.User.LastName,
                TotalReplies = f.Replies.Count(r => !r.IsDeleted && !r.User.IsActive),
                TotalLikes = f.ForumLikes.Count(),
                Replies = f.Replies
                .Where(f => !f.IsDeleted)
            .OrderByDescending(r => r.CreatedDate)
            .Take(2)
            .Select(r => new { r.ReplyId, r.Content, r.CreatedDate, ReplyAuthor = r.User.FirstName + " " + r.User.LastName, r.IsDeleted }).Where(r => !r.IsDeleted)
            .ToList()
            })
            .ToListAsync();

        ViewData["TotalBiodata"] = totalBiodata;
        ViewData["TotalUsers"] = totalUsers;
        ViewData["NriCount"] = nriCount;
        ViewBag.LatestMale = latestMale;
        ViewBag.LatestFemale = latestFemale;
        ViewBag.Forum = forums;
        ViewBag.TotalMaleBiodata = totalMaleBiodata;
        ViewBag.TotalFemaleBiodata = totalFemaleBiodata;

        return View();
    }

    [Authorize]
    public IActionResult Dashboard()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return RedirectToAction("login", "account");
        }

        var userId = int.Parse(userIdClaim.Value);
        var biodataList = _context.Biodata
            .Where(b => b.UserId == userId)
            .Include(b => b.Images.OrderByDescending(i => i.IsDefault))
            .Include(b => b.User)
            .Include(b => b.ProfileViews)
            .Where(b => b.IsDeleted == false)
            .ToList();
        var selfBiodata = biodataList.Where(b => b.IsForSelf).ToList();
        var onBehalfOfBiodata = biodataList.Where(b => !b.IsForSelf).ToList();
        var subscription = _context.Subscriptions
         .Where(s => s.UserId == userId && (s.IsActive || s.PaymentStatus == "Pending"))
         .OrderByDescending(s => s.StartDate)
         .FirstOrDefault();

        if (subscription != null && (subscription.PaymentStatus == "Pending" || !subscription.IsActive))
        {
            var lastShownDate = HttpContext.Session.GetString("PaymentModalLastShownDate");

            if (lastShownDate != DateTime.Now.ToString("yyyy-MM-dd"))
            {
                ViewBag.ShowPaymentModal = true;
                ViewBag.SubscriptionEndDate = subscription.EndDate.ToString("dd MMM yyyy");
                HttpContext.Session.SetString("PaymentModalLastShownDate", DateTime.Now.ToString("yyyy-MM-dd"));
            }
            else
            {
                ViewBag.ShowPaymentModal = false;
            }
        }
        else
        {
            ViewBag.ShowPaymentModal = false;
        }

        var viewHistory = _context.ProfileViewHistory.Where(pvh => pvh.UserId == userId).ToList();

        var model = new ViewBiodataViewModel
        {
            SelfBiodata = selfBiodata,
            OnBehalfOfBiodata = onBehalfOfBiodata,
            ViewHistory = viewHistory
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Contact(ContactUs contactUs)
    {
        if (!ModelState.IsValid)
        {
            return View(contactUs);
        }
        await _context.ContactUs.AddAsync(contactUs);
        await _context.SaveChangesAsync();
        SendEmail(contactUs);

        return RedirectToAction("contact", "home");
    }

    private void SendEmail(ContactUs contactUs)
    {
        var fromAddress = new MailAddress("mayanks.briskstar@gmail.com", "Suvarnyug");
        var toAddress = new MailAddress("mayanks.briskstar@gmail.com", "Suvarnyug");
        const string fromPassword = "lrym mtlk ypbm jvas";
        const string subject = "New Contact Us Message";

        string templatePath = Path.Combine(_environment.WebRootPath, "template", "Getintouch.html");
        string emailTemplate = System.IO.File.ReadAllText(templatePath);
        emailTemplate = emailTemplate.Replace("[FullName]", contactUs.FullName)
                               .Replace("[PhoneNumber]", contactUs.PhoneNumber)
                               .Replace("[Email]", contactUs.Email)
                               .Replace("[Message]", contactUs.Message);
        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
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
    public IActionResult About()
    {
        int totalBiodata = _context.Biodata.Count();
        int totalForum = _context.Forums.Count();

        ViewData["TotalBiodata"] = totalBiodata;
        ViewData["TotalForum"] = totalForum;
        return View();
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> LikeForumindex(int forumId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return RedirectToAction("login", "account");
        }

        var userId = int.Parse(userIdClaim.Value);
        var existingLike = await _context.ForumLikes
            .FirstOrDefaultAsync(fl => fl.ForumId == forumId && fl.UserId == userId);

        if (existingLike == null)
        {
            var like = new ForumLike
            {
                ForumId = forumId,
                UserId = userId
            };
            _context.ForumLikes.Add(like);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("index");
    }
    public IActionResult TermsandConditions()
    {
        return View();
    }
    public IActionResult privacypolicy()
    {
        return View();
    }
}

