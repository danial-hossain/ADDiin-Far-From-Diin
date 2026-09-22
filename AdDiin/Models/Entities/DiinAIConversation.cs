using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    /// <summary>
    /// A saved AI conversation owned by one authenticated user.
    /// </summary>
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

        // Messages are ordered by CreatedAt when history is returned to the client.
        public virtual ICollection<DiinAIMessage> Messages { get; set; } = new List<DiinAIMessage>();
    }
}
