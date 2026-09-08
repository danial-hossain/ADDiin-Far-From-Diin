using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class DiinAIConversation
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [MaxLength(255)]
        public string Title { get; set; } = "New Diin AI chat";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<DiinAIMessage> Messages { get; set; } = new List<DiinAIMessage>();
    }
}
