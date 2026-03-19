using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class Church
    {
        [Key]
        public int ChurchId { get; set; }

        [Required(ErrorMessage = "Church name is required")]
        [StringLength(200)]
        [Display(Name = "Church Name")]
        public string ChurchName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Location { get; set; }

        [Required(ErrorMessage = "First meeting day is required")]
        [StringLength(20)]
        [Display(Name = "First Meeting Day")]
        public string MeetingDay1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Second meeting day is required")]
        [StringLength(20)]
        [Display(Name = "Second Meeting Day")]
        public string MeetingDay2 { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // WhatsApp API Configuration per Church
        [StringLength(100)]
        [Display(Name = "WhatsApp Instance ID")]
        public string? WhatsAppInstanceId { get; set; }

        [StringLength(200)]
        [Display(Name = "WhatsApp API Token")]
        public string? WhatsAppApiToken { get; set; }

        [StringLength(200)]
        [Display(Name = "WhatsApp API URL")]
        public string? WhatsAppApiUrl { get; set; }

        [StringLength(20)]
        [Display(Name = "WhatsApp Phone Number")]
        public string? WhatsAppPhone { get; set; }

        [StringLength(200)]
        [Display(Name = "Birthday Group ID")]
        public string? BirthdayGroupId { get; set; }

        [StringLength(200)]
        [Display(Name = "General Group ID")]
        public string? GeneralGroupId { get; set; }

        [StringLength(200)]
        [Display(Name = "Prayer Group ID")]
        public string? PrayerGroupId { get; set; }

        [Display(Name = "Enable WhatsApp")]
        public bool WhatsAppEnabled { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Member>? Members { get; set; }
        public virtual ICollection<Attendance>? Attendances { get; set; }
        public virtual ICollection<Income>? Incomes { get; set; }
        public virtual ICollection<Expense>? Expenses { get; set; }
    }
}
