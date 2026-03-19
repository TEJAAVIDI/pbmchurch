using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class Verse
    {
        [Key]
        public int VerseId { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Image File Name")]
        public string ImageFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Storage URL")]
        public string StorageURL { get; set; } = string.Empty;

        [Display(Name = "Uploaded Date")]
        public DateTime UploadedDate { get; set; } = DateTime.Now;

        public int? UploadedBy { get; set; }

        [Display(Name = "Last Posted Date")]
        public DateTime? LastPostedDate { get; set; }

        [Display(Name = "Post Count")]
        public int PostCount { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
