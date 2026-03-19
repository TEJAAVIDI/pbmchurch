using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class DailyBibleReading
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime ReadingDate { get; set; }
        
        public int DayOfYear { get; set; }
        
        [StringLength(100)]
        public string ReadingRange { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string StartBook { get; set; } = string.Empty;
        
        public int StartChapter { get; set; }
        
        [StringLength(50)]
        public string EndBook { get; set; } = string.Empty;
        
        public int EndChapter { get; set; }
    }
}