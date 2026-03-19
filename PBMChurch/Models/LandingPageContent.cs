using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class LandingPageContent
    {
        [Key]
        public int ContentId { get; set; }

        [Required]
        [StringLength(200)]
        public string SectionName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImagePath { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }
    }

    public class GalleryImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ChurchId { get; set; }

        public string? Category { get; set; } // "Service", "Event", "Community", "Worship"

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Church? Church { get; set; }
    }
}
