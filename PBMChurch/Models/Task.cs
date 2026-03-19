using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class CalendarTask
    {
        public int TaskId { get; set; }

        [Required]
        [StringLength(200)]
        public string Summary { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public TimeSpan? EndTime { get; set; }

        public bool IsAllDay { get; set; }

        [Required]
        [StringLength(50)]
        public string TaskType { get; set; } = "Custom";

        public int? RelatedId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual AdminUser? Creator { get; set; }
        public virtual Member? RelatedMember { get; set; }
        public virtual Church? RelatedChurch { get; set; }
    }
}