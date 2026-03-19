using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class WishTemplate
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } // Birthday, Anniversary, Event, Bible, General
        
        public byte[]? ImageData { get; set; }
        
        public string? ImageFileName { get; set; }
        
        public string? ImageContentType { get; set; }
        
        public string? MessageText { get; set; }
        
        public string? YouTubeUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public string? CreatedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        
        public string? ModifiedBy { get; set; }
    }
    
    public class BibleReading
    {
        public int Id { get; set; }
        
        public int TotalChapters { get; set; } = 1189; // Total Bible chapters
        
        public int CurrentDay { get; set; }
        
        public decimal ChaptersPerDay { get; set; }
        
        public int StartChapter { get; set; }
        
        public int EndChapter { get; set; }
        
        public DateTime ReadingDate { get; set; }
        
        public int Year { get; set; }
        
        public bool IsCompleted { get; set; } = false;
    }
}