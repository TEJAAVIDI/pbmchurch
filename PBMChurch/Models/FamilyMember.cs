using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class FamilyMember
    {
        [Key]
        public int FamilyMemberId { get; set; } // Auto-incremented

        [Required]
        public int MemberId { get; set; } // Foreign key to Member

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Relation { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AnniversaryDate { get; set; }

    public int? RelatedMemberId { get; set; } // Link to MemberId of family member

        // Navigation property
        [ForeignKey("MemberId")]
        public Member Member { get; set; } = null!;
    }
}
