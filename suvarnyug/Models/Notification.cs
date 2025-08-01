using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("notifications")]
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
