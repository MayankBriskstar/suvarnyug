using System.ComponentModel.DataAnnotations;

namespace suvarnyug.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[!@#$%^&*()_+[\]{};':""\\|,.<>\/?])(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "New password must contain at least one special character, one letter, one number, and be at least 8 characters long.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
