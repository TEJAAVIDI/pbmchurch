using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class Income
    {
        [Key]
        public int IncomeId { get; set; }

        [Required]
        [Display(Name = "Church")]
        public int ChurchId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Source is required")]
        [StringLength(200)]
        [Display(Name = "Income Source")]
        public string Source { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Income Date")]
        public DateTime IncomeDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        public int AddedBy { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // Navigation property
        [ForeignKey("ChurchId")]
        public virtual Church? Church { get; set; }
    }
}
