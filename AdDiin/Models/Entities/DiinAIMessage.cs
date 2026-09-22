using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    /// <summary>
    /// Stores one user or assistant message within a saved AI conversation.
    /// </summary>
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

        // Citations are serialized with assistant messages so historical
        // responses can render the same source information later.
        public string? SourcesJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
