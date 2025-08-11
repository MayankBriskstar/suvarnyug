//using Microsoft.AspNetCore.SignalR;
//using System.Threading.Tasks;
//using Suvarnyug.Data;
//using Suvarnyug.Models;
//using System.Linq;
//using System;
//using suvarnyug.Models;
//using Microsoft.EntityFrameworkCore;

//namespace suvarnyug.Hubs
//{
//    public class ChatHub : Hub
//    {
//        private readonly ApplicationDbContext _context;

//        public ChatHub(ApplicationDbContext context)
//        {
//            _context = context;
//        }
//        public override async Task OnConnectedAsync()
//        {
//            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrEmpty(userId))
//            {
//                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
//            }
//            await base.OnConnectedAsync();
//        }

//        //public async Task SendMessage(int senderId, int receiverId, string message, int messageId)
//        //{
//        //    await Clients.Group(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message, messageId);
//        //}

//        //public async Task MarkMessageAsRead(int messageId)
//        //{
//        //    await Clients.All.SendAsync("MessageSeen", messageId);
//        //}
//        //public async Task NotifyMessageSeen(int messageId, int senderId)
//        //{
//        //    await Clients.User(senderId.ToString()).SendAsync("MessageSeen", messageId);
//        //}
//        //public async Task UpdateUnreadMessages(int receiverId, int senderId)
//        //{
//        //    var unreadCount = _context.ChatMessages
//        //        .Count(m => m.ReceiverId == receiverId && m.SenderId == senderId && !m.IsRead);

//        //    var latestMessage = _context.ChatMessages
//        //        .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
//        //                    (m.SenderId == receiverId && m.ReceiverId == senderId))
//        //        .OrderByDescending(m => m.SentAt)
//        //        .Select(m => m.MessageText)
//        //        .FirstOrDefault();

//        //    await Clients.User(receiverId.ToString()).SendAsync("UpdateUnreadMessages", senderId, unreadCount, latestMessage);
//        //}

//    }
//}
