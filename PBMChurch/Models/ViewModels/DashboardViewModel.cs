    public class TodayChurchAttendance
    {
        public int ChurchId { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

namespace PBMChurch.Models.ViewModels
{
    public class DashboardViewModel
    {
    public int TotalMembers { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int PresentTodayForSelectedChurch { get; set; }
    public int AbsentTodayForSelectedChurch { get; set; }
    public string SelectedChurchName { get; set; } = string.Empty;
    public List<TodayChurchAttendance> TodayChurchAttendances { get; set; } = new();
        public int BirthdaysToday { get; set; }
        public decimal MonthIncome { get; set; }
        public decimal MonthExpense { get; set; }
        public decimal MonthBalance { get; set; }
        
        public List<BirthdayMember> TodayBirthdays { get; set; } = new();
        public List<BirthdayMember> UpcomingBirthdays { get; set; } = new();
        public List<ChurchAttendanceStats> ChurchWiseAttendance { get; set; } = new();
        public List<MeetingDay> ThisWeekMeetings { get; set; } = new();
        public BibleReadingInfo TodayBibleReading { get; set; } = new();
    }

    public class BirthdayMember
    {
        public int MemberId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string ChurchName { get; set; } = string.Empty;
        public int DaysUntilBirthday { get; set; }
        public int Age { get; set; }
    }

    public class ChurchAttendanceStats
    {
        public int ChurchId { get; set; }
        public string ChurchName { get; set; } = string.Empty;
        public int TotalMembers { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public double AttendancePercentage => TotalMembers > 0 ? (double)PresentCount / TotalMembers * 100 : 0;
    }

    public class MeetingDay
    {
        public string ChurchName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class BibleReadingInfo
    {
        public int DayOfYear { get; set; }
        public int Year { get; set; }
        public decimal ChaptersPerDay { get; set; }
        public string TodaysReading { get; set; } = string.Empty;
        public decimal Progress { get; set; }
    }
}
