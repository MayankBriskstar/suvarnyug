using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("directpayments")]
    public class DirectPayment
    {
        [Key]
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Description { get; set; }
        public string PaymentScreenshot { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentStatus { get; set; }
    }
}
