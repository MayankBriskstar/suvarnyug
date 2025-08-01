using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace suvarnyug.Models
{
    [Table("userlogins")]
    public class UserLogin
    {
        [Key] 
        public int LoginId { get; set; }
        public int UserId { get; set; }

        public DateTime LoginTime { get; set; }

        public virtual User User { get; set; } // Navigation property
    }
}
