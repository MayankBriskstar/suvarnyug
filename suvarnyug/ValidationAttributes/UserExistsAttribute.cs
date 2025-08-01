using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Suvarnyug.Data;

namespace Suvarnyug.ValidationAttributes
{
    public class UserExistsAttribute : ValidationAttribute
    {
        private readonly string _propertyName;

        public UserExistsAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var context = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
            var propertyValue = value as string;

            var userIdProperty = validationContext.ObjectType.GetProperty("UserId");
            var userId = (int)userIdProperty.GetValue(validationContext.ObjectInstance);

            var existingUser = context.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);

            if (existingUser != null)
            {
                if (_propertyName == "Email" && context.Users.Any(u => u.Email == propertyValue && u.UserId != userId))
                {
                    return new ValidationResult("Email already exists.");
                }
                else if (_propertyName == "MobileNo" && context.Users.Any(u => u.MobileNo == propertyValue && u.UserId != userId))
                {
                    return new ValidationResult("Mobile number already exists.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
