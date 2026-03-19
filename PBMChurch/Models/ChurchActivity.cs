using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class ChurchActivity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime FromDateTime { get; set; }

        [Required]
        public DateTime ToDateTime { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Location { get; set; } = string.Empty;

        public byte[]? ImageData { get; set; }
        public string? ImageContentType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}