using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using suvarnyug.Models;
using Suvarnyug.ValidationAttributes;

namespace Suvarnyug.Models
{
    [Table("users")]
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [UserExists("Email", ErrorMessage = "Email already exists.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile Number must be exactly 10 digits.")]
        [UserExists("MobileNo", ErrorMessage = "Mobile number already exists.")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Please Select Country")]
        public int CountryId { get; set; }

        public Country Country { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[!@#$%^&*()_+[\]{};':""\\|,.<>\/?])(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "Password must contain at least one special character, one letter, one number,and 8 character long.")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("PasswordHash", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "User";
        public string? ProfilePhotoPath { get; set; } = "/images/default-profile.jpg";

        public bool IsActive { get; set; } = false;

        //public bool IsVerified { get; set; }
        //[ReadOnly]
        //[Required(ErrorMessage = "Rechapta is required")]
        //public string Rechapta { get; set; }
        public ICollection<ReplyLike> ReplyLikes { get; set; } = new List<ReplyLike>();
        public ICollection<ForumLike> ForumLikes { get; set; } = new List<ForumLike>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<ProfileViewHistory> ProfileViews { get; set; } = new List<ProfileViewHistory>();
        public ICollection<UserLogin> UserLogin { get; set; }  // Ensure this is a collection
    }
}
