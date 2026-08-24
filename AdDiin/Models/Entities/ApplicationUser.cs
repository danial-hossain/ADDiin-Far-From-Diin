using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; } // Male, Female, Other

        [MaxLength(255)]
        public string? ProfilePicture { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation collections
        public virtual ICollection<MiladRequest> MiladRequests { get; set; } = new List<MiladRequest>();
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public virtual ICollection<Conversation> UserConversations { get; set; } = new List<Conversation>();
        public virtual ICollection<Conversation> AdminConversations { get; set; } = new List<Conversation>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
