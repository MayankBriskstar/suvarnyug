using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("userverification")]
    public class UserVerification
    {
        [Key]
        public int VerificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "Aadhaar number must be exactly 12 digits.")]
        public string AadhaarNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public bool IsVerified { get; set; } = false;

        public User User { get; set; }
    }
}
