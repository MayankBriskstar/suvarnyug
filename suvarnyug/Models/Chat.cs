using Suvarnyug.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("chatrooms")]
    public class ChatRoom
    {
        [Key]
        public int ChatRoomId { get; set; }

        [Required]
        public int User1Id { get; set; }

        [Required]
        public int User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("chatmessages")]
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ChatRoomId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        public string MessageText { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.Now;

        [ForeignKey("ChatRoomId")]
        public ChatRoom ChatRoom { get; set; }

    }
}
