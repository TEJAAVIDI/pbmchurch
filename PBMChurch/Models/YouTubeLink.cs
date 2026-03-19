using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class YouTubeLink
    {
        [Key]
        public int LinkId { get; set; }

        [Required]
        [Display(Name = "Church")]
        public int ChurchId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(300)]
        [Display(Name = "Video Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "YouTube URL is required")]
        [StringLength(500)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "YouTube URL")]
        public string YouTubeURL { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Posted Date")]
        public DateTime PostedDate { get; set; } = DateTime.Now;

        public int? PostedBy { get; set; }

        [Display(Name = "Sent to WhatsApp")]
        public bool IsSentToWhatsApp { get; set; } = false;

        [Display(Name = "Sent Date")]
        public DateTime? SentDate { get; set; }

        // Navigation property
        [ForeignKey("ChurchId")]
        public virtual Church? Church { get; set; }
    }
}
