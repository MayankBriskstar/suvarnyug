using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("subscriptions")]
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string PaymentStatus { get; set; }
        [Column("PlanType")]
        public PlanType PlanType { get; set; }
        public User User { get; set; }
    }
    public enum PlanType
    {
        Free,
        Premium
    }

}
