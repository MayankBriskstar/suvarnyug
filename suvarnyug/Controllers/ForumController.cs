using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using suvarnyug.Models;
using Suvarnyug.Data;
using System.Security.Claims;

namespace suvarnyug.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ForumController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Forums()
        {
            var forums = await _context.Forums
                .Include(f => f.User)
                .Include(f => f.Replies)
                .ThenInclude(r => r.User)
                .Include(f => f.Replies).ThenInclude(r => r.Replies).Where(r => !r.IsDeleted && !r.User.IsActive)
                .Include(f => f.Replies).ThenInclude(r => r.Likes)
                .Include(f => f.ForumLikes)
                .Where(f => !f.IsDeleted && !f.User.IsActive)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
            foreach (var forum in forums)
            {
                forum.Replies = forum.Replies.Where(r => !r.IsDeleted && !r.User.IsActive).ToList();
                foreach (var reply in forum.Replies)
                {
                    reply.Replies = reply.Replies.Where(r => !r.IsDeleted && !r.User.IsActive).ToList();
                }
            }
            return View(forums);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddForum()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddForum(Forum forum)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("login", "account");
            }

            forum.CreatedDate = DateTime.Now;
            forum.UserId = int.Parse(userIdClaim.Value);

            _context.Forums.Add(forum);
            await _context.SaveChangesAsync();

            return RedirectToAction("forums");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReply(int forumId, string content, int? parentReplyId = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                ModelState.AddModelError("", "User not logged in.");
                return RedirectToAction("forums");
            }

            var reply = new Reply
            {
                ForumId = forumId,
                Content = content,
                CreatedDate = DateTime.Now,
                UserId = int.Parse(userIdClaim.Value),
                ParentReplyId = parentReplyId
            };

            _context.Replies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("forums");
        }
        [Authorize]
        [HttpPost]
        public IActionResult AddReplyAjax(int forumId, string content, int? parentReplyId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var reply = new Reply
            {
                ForumId = forumId,
                Content = content,
                CreatedDate = DateTime.Now,
                IsDeleted = false,
                ParentReplyId = parentReplyId,
                UserId = userId
            };

            _context.Replies.Add(reply);
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            return Json(new
            {
                success = true,
                replyId = reply.ReplyId,
                content = reply.Content,
                createdDate = reply.CreatedDate.ToString("dd/MM/yyyy"),
                userFullName = $"{user?.FirstName} {user?.LastName}",
                profilePhotoPath = !string.IsNullOrEmpty(user?.ProfilePhotoPath) ? user.ProfilePhotoPath : Url.Content("~/images/default-profile.jpg"),
                forumId = reply.ForumId,
                parentReplyId = reply.ParentReplyId
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikeReply(int replyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            var existingLike = await _context.ReplyLikes
                .FirstOrDefaultAsync(l => l.ReplyId == replyId && l.UserId == userId);

            if (existingLike == null)
            {
                var likeEntity = new ReplyLike
                {
                    ReplyId = replyId,
                    UserId = userId
                };
                _context.ReplyLikes.Add(likeEntity);
                await _context.SaveChangesAsync();

                var updatedLikeCount = await _context.ReplyLikes
                    .Where(l => l.ReplyId == replyId)
                    .CountAsync();

                return Ok(new { likeCount = updatedLikeCount, liked = true });
            }
            else
            {
                _context.ReplyLikes.Remove(existingLike);
                await _context.SaveChangesAsync();

                var updatedLikeCount = await _context.ReplyLikes
                    .Where(l => l.ReplyId == replyId)
                    .CountAsync();

                return Ok(new { likeCount = updatedLikeCount, liked = false });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikeForum(int forumId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var existingLike = await _context.ForumLikes
                .FirstOrDefaultAsync(fl => fl.ForumId == forumId && fl.UserId == userId);

            bool liked;
            if (existingLike == null)
            {
                var like = new ForumLike
                {
                    ForumId = forumId,
                    UserId = userId
                };
                _context.ForumLikes.Add(like);
                liked = true;
            }
            else
            {
                _context.ForumLikes.Remove(existingLike);
                liked = false;
            }

            await _context.SaveChangesAsync();

            // Get new like count
            int likeCount = await _context.ForumLikes.CountAsync(fl => fl.ForumId == forumId);

            return Json(new { liked, likeCount });
        }

    }
}
