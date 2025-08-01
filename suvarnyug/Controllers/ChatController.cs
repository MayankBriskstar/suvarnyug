using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Suvarnyug.Data;
using Suvarnyug.Models;
using suvarnyug.Hubs;
using System.Linq;
using System.Threading.Tasks;
using suvarnyug.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace suvarnyug.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            //var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            //if (user.Role != "Admin")
            //{
            //    var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.Now);

            //    if (subscription == null)
            //    {
            //        return RedirectToAction("SubscriptionDetails", "Payment");
            //    }
            //}
            var users = await _context.Users
            .Where(u => u.UserId != userId && !u.IsActive)
            .Where(u => _context.ChatRooms.Any(cr => (cr.User1Id == userId && cr.User2Id == u.UserId) ||(cr.User2Id == userId && cr.User1Id == u.UserId)))
            .Select(u => new
            {
                u.UserId,
                FullName = u.FirstName + " " + u.LastName,
                u.ProfilePhotoPath,
                UnreadMessagesCount = _context.ChatMessages
                .Count(m => m.ReceiverId == userId && m.SenderId == u.UserId && !m.IsRead),
                LatestMessage = _context.ChatMessages
                .Where(m => (m.SenderId == u.UserId && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == u.UserId))
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.MessageText)
                .FirstOrDefault(),
                LatestMessageTime = _context.ChatMessages
                .Where(m => (m.SenderId == u.UserId && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == u.UserId))
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.SentAt)
                .FirstOrDefault()
            })
            .OrderByDescending(u => u.LatestMessageTime)
            .ToListAsync();

            return View(users);
        }

        [Authorize]
        [HttpGet]
        [Route("Chat/Chat/{userId}")]
        public async Task<IActionResult> Chat(int userId)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(loggedInUserId);
            if (user.Role != "Admin")
            {
                var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == loggedInUserId && s.IsActive && s.EndDate > DateTime.Now);

                if (subscription == null || subscription.PlanType != PlanType.Platinum)
                {
                    return RedirectToAction("SubscriptionDetails", "Payment");
                }
            }
            var otherUser = await _context.Users.FindAsync(userId);
            if (otherUser == null)
                return NotFound();

            var chatRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(r => (r.User1Id == loggedInUserId && r.User2Id == userId) ||
                                          (r.User1Id == userId && r.User2Id == loggedInUserId));

            var messages = chatRoom != null
                ? await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoom.ChatRoomId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync()
                : new List<ChatMessage>();

            await MarkMessagesAsRead(userId);
            ViewBag.OtherUser = otherUser;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int receiverId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var chatRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(r => (r.User1Id == senderId && r.User2Id == receiverId) ||
                                          (r.User1Id == receiverId && r.User2Id == senderId));

            if (chatRoom == null)
            {
                chatRoom = new ChatRoom { User1Id = senderId, User2Id = receiverId };
                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();
            }
            var chatMessage = new ChatMessage
            {
                ChatRoomId = chatRoom.ChatRoomId,
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageText = message,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(receiverId.ToString())
                .SendAsync("ReceiveMessage", senderId, message, chatMessage.MessageId);

            await _hubContext.Clients.User(receiverId.ToString())
                .SendAsync("UpdateUnreadMessages", senderId,
                _context.ChatMessages.Count(m => m.ReceiverId == receiverId && m.SenderId == senderId && !m.IsRead),
                message);

            return Ok(new { success = true, messageId = chatMessage.MessageId });
        }

        [HttpPost]
        [Route("Chat/MarkMessagesAsRead/{otherUserId}")]
        public async Task<IActionResult> MarkMessagesAsRead(int otherUserId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var chatRoom = await _context.ChatRooms
                    .FirstOrDefaultAsync(r => (r.User1Id == userId && r.User2Id == otherUserId) ||
                                              (r.User1Id == otherUserId && r.User2Id == userId));

                if (chatRoom == null)
                    return NotFound(new { success = false, message = "Chat room not found." });

                var unreadMessages = await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoom.ChatRoomId &&
                                m.ReceiverId == userId &&
                                !m.IsRead)
                    .ToListAsync();

                if (!unreadMessages.Any())
                    return Ok(new { success = false, message = "No unread messages." });

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                var messageIds = unreadMessages.Select(m => m.MessageId).ToList();
                await _hubContext.Clients.User(otherUserId.ToString())
                    .SendAsync("MessagesSeen", messageIds);

                return Ok(new { success = true, messageIds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }
    }
}