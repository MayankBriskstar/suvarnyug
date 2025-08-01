using Microsoft.Extensions.Hosting;
using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("forums")]
    public class Forum
    {
        public int ForumId { get; set; }

        [Required(ErrorMessage = "Please Enter Title.")]
        [StringLength(100, ErrorMessage = "Title Cannot Exceed 100 Characters")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please Enter Description.")]
        [StringLength(300, ErrorMessage = "Description Cannot Exceed 300 Characters")]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Reply> Replies { get; set; } = new List<Reply>();
        public virtual ICollection<ForumLike> ForumLikes { get; set; } = new List<ForumLike>();
    }
}
