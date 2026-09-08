using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class DiinAIMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public virtual DiinAIConversation? Conversation { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "user";

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? SourcesJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
