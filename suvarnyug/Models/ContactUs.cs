using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("contactus")]
    public class ContactUs
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^\d{10,14}$", ErrorMessage = "Invalid Phone Number. Only digits allowed, length between 10 to 14.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Your Message is required.")]
        public string Message { get; set; }
    }
}
