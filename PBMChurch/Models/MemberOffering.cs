using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class MemberOffering
    {
        [Key]
        public int OfferingId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Category")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Purpose")]
        [StringLength(500)]
        public string? Purpose { get; set; }

        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        // Navigation property
        [ForeignKey("MemberId")]
        public virtual Member? Member { get; set; }
    }
}
