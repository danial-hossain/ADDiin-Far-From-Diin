using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdDiin.Models.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }

        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string SenderType { get; set; } = "user"; // user, admin

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
