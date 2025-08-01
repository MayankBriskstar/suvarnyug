using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("replies")]
    public class Reply
    {
        public int ReplyId { get; set; }

        public int ForumId { get; set; }
        public Forum Forum { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int? ParentReplyId { get; set; }
        public Reply ParentReply { get; set; }
        public ICollection<Reply> Replies { get; set; }
        public virtual ICollection<ReplyLike> Likes { get; set; } = new List<ReplyLike>();
    }
}
