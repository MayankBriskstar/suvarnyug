using System.ComponentModel.DataAnnotations;

namespace suvarnyug.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email Is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}
