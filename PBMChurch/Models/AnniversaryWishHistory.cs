using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class AnniversaryWishHistory
    {
        [Key]
        public int Id { get; set; }
        
        public int MemberId { get; set; }
        
        public DateTime WishDate { get; set; }
        
        public DateTime SentAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Member? Member { get; set; }
    }
}