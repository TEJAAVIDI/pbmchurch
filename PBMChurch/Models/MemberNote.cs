using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class MemberNote
    {
        [Key]
        public int NoteId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [Display(Name = "Note")]
        public string NoteText { get; set; } = string.Empty;

        [Display(Name = "Created By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        // Navigation property
        [ForeignKey("MemberId")]
        public virtual Member? Member { get; set; }
    }
}
