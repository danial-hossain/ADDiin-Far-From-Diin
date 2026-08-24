using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(255)]
        public string? ImagePublicId { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; } // education, charity, youth, community

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
