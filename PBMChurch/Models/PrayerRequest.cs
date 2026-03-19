using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class PrayerRequest
    {
        [Key]
        public int RequestId { get; set; }
        
        public int? MemberId { get; set; }
        public Member? Member { get; set; }
        
        [StringLength(200)]
        public string? Title { get; set; }
        
        [Required]
        public string Request { get; set; }
        
        public DateTime RequestDate { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Additional properties for non-member requests
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(15)]
        public string? Phone { get; set; }
        
        public int? ChurchId { get; set; }
        public Church? Church { get; set; }
        
        public DateTime? AnsweredDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}