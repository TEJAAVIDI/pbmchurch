namespace PBMChurch.Models.ViewModels
{
    public class AttendanceEntryViewModel
    {
        public int ChurchId { get; set; }
        public string ChurchName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; } = DateTime.Today;
        public int WeekNumber { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        // Auto-fetched member details
        public int? MemberId { get; set; }
        public string? MemberName { get; set; }
        public string? Phone { get; set; }
        public string? Family { get; set; }
        
        public List<AttendanceRecord> TodayAttendance { get; set; } = new();
        public List<DateTime> AvailableDates { get; set; } = new();
    }

    public class AttendanceRecord
    {
        public int AttendanceId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public DateTime MarkedDate { get; set; }
    }
}
