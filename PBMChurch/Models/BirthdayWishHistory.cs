using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBMChurch.Models
{
    public class BirthdayWishHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Wish Sent Date")]
        public DateTime WishSentDate { get; set; }

        [Display(Name = "Sent Date Time")]
        public DateTime SentDateTime { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Sent";

        [StringLength(500)]
        [Display(Name = "Error Message")]
        public string? ErrorMessage { get; set; }

        [StringLength(500)]
        [Display(Name = "Message Sent")]
        public string? MessageSent { get; set; }

        // Navigation property
        [ForeignKey("MemberId")]
        public virtual Member? Member { get; set; }
    }
}
