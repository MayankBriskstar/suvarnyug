using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("profileviewhistory")]
    public class ProfileViewHistory
    {
        public int ViewId { get; set; }
        public int UserId { get; set; }
        public int BiodataId { get; set; }
        public DateTime ViewedAt { get; set; }
        public Biodata Biodata { get; set; }
        public User User { get; set; }
    }
}
