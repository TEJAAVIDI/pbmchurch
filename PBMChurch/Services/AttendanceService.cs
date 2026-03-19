using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;

namespace PBMChurch.Services
{
    public interface IAttendanceService
    {
        Task<Member?> GetMemberByUserIdAsync(string userId);
        Task<bool> MarkAttendanceAsync(int churchId, string userId, DateTime date, int markedBy);
        Task<int> MarkAutoAbsentAsync(int churchId, DateTime date, int markedBy);
        Task<List<Attendance>> GetTodayAttendanceAsync(int churchId, DateTime date);
        List<DateTime> GetMeetingDatesForWeek(Church church, DateTime startDate);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Member?> GetMemberByUserIdAsync(string userId)
        {
            return await _context.Members
                .Include(m => m.Church)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.Status == "Active");
        }

        public async Task<bool> MarkAttendanceAsync(int churchId, string userId, DateTime date, int markedBy)
        {
            var member = await GetMemberByUserIdAsync(userId);
            if (member == null || member.ChurchId != churchId)
                return false;

            var weekNumber = GetWeekNumber(date);

            // Check if attendance already exists
            var existing = await _context.Attendance
                .FirstOrDefaultAsync(a => a.MemberId == member.MemberId && a.AttendanceDate == date);

            if (existing != null)
            {
                // Update existing
                existing.IsPresent = true;
                existing.MarkedBy = markedBy;
                existing.MarkedDate = DateTime.Now;
            }
            else
            {
                // Create new
                var attendance = new Attendance
                {
                    ChurchId = churchId,
                    MemberId = member.MemberId,
                    AttendanceDate = date,
                    WeekNumber = weekNumber,
                    IsPresent = true,
                    MarkedBy = markedBy,
                    MarkedDate = DateTime.Now
                };
                _context.Attendance.Add(attendance);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAutoAbsentAsync(int churchId, DateTime date, int markedBy)
        {
            var weekNumber = GetWeekNumber(date);

            // Get all active members of the church
            var members = await _context.Members
                .Where(m => m.ChurchId == churchId && m.Status == "Active")
                .ToListAsync();

            int count = 0;
            foreach (var member in members)
            {
                // Check if attendance already marked
                var exists = await _context.Attendance
                    .AnyAsync(a => a.MemberId == member.MemberId && a.AttendanceDate == date);

                if (!exists)
                {
                    _context.Attendance.Add(new Attendance
                    {
                        ChurchId = churchId,
                        MemberId = member.MemberId,
                        AttendanceDate = date,
                        WeekNumber = weekNumber,
                        IsPresent = false,
                        MarkedBy = markedBy,
                        MarkedDate = DateTime.Now
                    });
                    count++;
                }
            }

            await _context.SaveChangesAsync();
            return count;
        }

        public async Task<List<Attendance>> GetTodayAttendanceAsync(int churchId, DateTime date)
        {
            return await _context.Attendance
                .Include(a => a.Member)
                .Where(a => a.ChurchId == churchId && a.AttendanceDate == date)
                .OrderBy(a => a.Member!.Name)
                .ToListAsync();
        }

        public List<DateTime> GetMeetingDatesForWeek(Church church, DateTime startDate)
        {
            var dates = new List<DateTime>();
            var endDate = startDate.AddDays(6);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayName = date.DayOfWeek.ToString();
                if (dayName.Equals(church.MeetingDay1, StringComparison.OrdinalIgnoreCase) ||
                    dayName.Equals(church.MeetingDay2, StringComparison.OrdinalIgnoreCase))
                {
                    dates.Add(date);
                }
            }

            return dates;
        }

        private int GetWeekNumber(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, 
                System.Globalization.CalendarWeekRule.FirstDay, 
                DayOfWeek.Sunday);
        }
    }
}
