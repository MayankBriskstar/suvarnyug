using Microsoft.AspNetCore.SignalR;
using suvarnyug.Models;
using Suvarnyug.Data;
namespace suvarnyug.Services
{
    public interface INotificationService
    {
        Task AddNotification(Notification notification);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task AddNotification(Notification notification)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }
}
