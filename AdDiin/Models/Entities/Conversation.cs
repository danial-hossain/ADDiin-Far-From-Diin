using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdDiin.Models.Entities
{
    public class Conversation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public int? AdminId { get; set; }

        [ForeignKey("AdminId")]
        public virtual ApplicationUser? Admin { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; } = "Support Request";

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "active"; // active, closed, pending

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
