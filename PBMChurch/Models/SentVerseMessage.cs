using System;

namespace PBMChurch.Models
{
    public class SentVerseMessage
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public string? YouTubeLink { get; set; }
        public DateTime SentDate { get; set; }
        public int SentBy { get; set; }
        public string Churches { get; set; } // Comma-separated ChurchIds
    }
}
