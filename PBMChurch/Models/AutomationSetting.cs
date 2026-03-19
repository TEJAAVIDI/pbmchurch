using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class AutomationSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Setting Key")]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Setting Value")]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; }
    }
}
