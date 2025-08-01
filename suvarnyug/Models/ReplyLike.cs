using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("replylikes")]
    public class ReplyLike
    {
        public int Id { get; set; }
        public int ReplyId { get; set; }
        public int UserId { get; set; }
        public virtual Reply Reply { get; set; }
        public virtual User User { get; set; }
    }
}
