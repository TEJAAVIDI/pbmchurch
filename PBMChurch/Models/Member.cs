using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
        public class Member
        {
            public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
            [Key]
            public int MemberId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [StringLength(50)]
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        [Display(Name = "Sur Name")]
        public string? Family { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Anniversary Date")]
        public DateTime? AnniversaryDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Joined Date")]
        public DateTime JoinedDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Church")]
        public int ChurchId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Display(Name = "Profile Image")]
        public string? ProfileImage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // Navigation properties
        [ForeignKey("ChurchId")]
        public virtual Church? Church { get; set; }

            public virtual ICollection<Attendance>? Attendances { get; set; }
            [StringLength(200)]
            [EmailAddress]
            public string? Email { get; set; }
        }
    }
// End of namespace
