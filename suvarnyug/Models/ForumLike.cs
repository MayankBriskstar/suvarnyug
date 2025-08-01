using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("forumlikes")]
    public class ForumLike
    {
        public int Id { get; set; }

        public int ForumId { get; set; }
        public int UserId { get; set; }

        public virtual Forum Forum { get; set; }
        public virtual User User { get; set; }
    }
}
