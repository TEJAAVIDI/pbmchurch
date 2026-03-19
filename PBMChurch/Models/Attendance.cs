using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int ChurchId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; }

        [Required]
        [Display(Name = "Week Number")]
        public int WeekNumber { get; set; }

        [Display(Name = "Present")]
        public bool IsPresent { get; set; } = false;

        public int? MarkedBy { get; set; }

        public DateTime MarkedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("ChurchId")]
        public virtual Church? Church { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member? Member { get; set; }
    }
}
