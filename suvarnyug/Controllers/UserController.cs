﻿using Microsoft.AspNetCore.Mvc;
using Suvarnyug.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using suvarnyug.Models;
using Suvarnyug.Models;
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [Route("user/profile/{biodataId}")]
    public IActionResult Profile(int biodataId)
    {
        var biodata = _context.Biodata.Include(b => b.Images.OrderByDescending(i => i.IsDefault))
            .Include(b => b.Country).Include(b => b.State).Include(b => b.City).FirstOrDefault(b => b.BiodataId == biodataId);
        if (biodata == null)
        {
            return NotFound();
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int userIdInt = int.Parse(userId);

        var user = _context.Users.FirstOrDefault(u => u.UserId == userIdInt);
        var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == userIdInt && s.IsActive && s.EndDate > DateTime.Now);
        bool isSubscribed = subscription != null;
        if (user.Role == "Admin")
        {
            ViewData["IsSubscribed"] = true;
        }
        else
        {
            ViewData["IsSubscribed"] = isSubscribed;
        }
        var hasViewed = _context.ProfileViewHistory
            .Any(v => v.UserId == userIdInt && v.BiodataId == biodataId);
        if (!hasViewed)
        {
            var viewHistory = new ProfileViewHistory
            {
                UserId = userIdInt,
                BiodataId = biodataId,
                ViewedAt = DateTime.Now
            };

            _context.ProfileViewHistory.Add(viewHistory);
            _context.SaveChanges();
        }
        
        return View(biodata);
    }
    
    [Authorize]
    [HttpPost]
    public IActionResult GetUploadedProfiles(int biodataId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return RedirectToAction("login", "account");
        }

        var userId = int.Parse(userIdClaim.Value);

        var biodataList = _context.Biodata
            .Where(b => b.UserId == userId && b.IsDeleted == false)
            .Include(b => b.Images.OrderByDescending(i => i.IsDefault))
            .Include(b => b.User)
            .Include(b => b.ProfileViews)
            .ToList();
        ViewData["OnBehalfOfBiodata"] = biodataList;

        return PartialView("_BiodataListPartial", biodataList);
    }

}
