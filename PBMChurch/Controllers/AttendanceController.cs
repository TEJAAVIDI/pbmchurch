using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Attributes;
using PBMChurch.Data;
using PBMChurch.Models.ViewModels;
using PBMChurch.Services;
using System.Globalization;
using System.Security.Claims;

namespace PBMChurch.Controllers
{
    [RoleAuthorize("Admin", "Member")]
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(AppDbContext context, IAttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            
            var adminId = GetCurrentAdminId();
            var admin = await _context.AdminUsers.FindAsync(adminId);
            ViewBag.AttendanceMode = admin?.AttendanceMode ?? "individual";
            
            return View(new AttendanceEntryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoadWeek(int churchId, DateTime startDate)
        {
            var church = await _context.Churches.FindAsync(churchId);
            if (church == null)
                return Json(new { success = false, message = "Church not found" });

            var dates = _attendanceService.GetMeetingDatesForWeek(church, startDate);
            return Json(new { success = true, dates = dates.Select(d => new { date = d.ToString("yyyy-MM-dd"), display = d.ToString("dddd, MMM dd") }) });
        }

        [HttpPost]
        public async Task<IActionResult> GetMemberDetails(string userId)
        {
            var member = await _attendanceService.GetMemberByUserIdAsync(userId);
            if (member == null)
                return Json(new { success = false, message = "Member not found or inactive" });

            return Json(new
            {
                success = true,
                memberId = member.MemberId,
                name = member.Name,
                phone = member.Phone,
                family = member.Family,
                churchId = member.ChurchId,
                profileImage = member.ProfileImage
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPresent(int churchId, string userId, DateTime attendanceDate)
        {
            var adminId = GetCurrentAdminId();
            var success = await _attendanceService.MarkAttendanceAsync(churchId, userId, attendanceDate, adminId);

            if (success)
            {
                TempData["Success"] = "Attendance marked successfully!";
                return Json(new { success = true, message = "Attendance marked successfully" });
            }

            return Json(new { success = false, message = "Failed to mark attendance" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFamilyPresent(int churchId, string userId, DateTime attendanceDate)
        {
            var adminId = GetCurrentAdminId();
            
            // Get the member
            var member = await _attendanceService.GetMemberByUserIdAsync(userId);
            if (member == null)
                return Json(new { success = false, message = "Member not found" });

            // Mark attendance for the main member
            var success = await _attendanceService.MarkAttendanceAsync(churchId, userId, attendanceDate, adminId);
            if (!success)
                return Json(new { success = false, message = "Failed to mark attendance" });

            int familyCount = 0;

            // Get family members linked to this member
            var familyMembers = await _context.FamilyMembers
                .Where(fm => fm.MemberId == member.MemberId && fm.RelatedMemberId.HasValue)
                .ToListAsync();

            // Mark attendance for each family member
            foreach (var familyMember in familyMembers)
            {
                var relatedMember = await _context.Members.FindAsync(familyMember.RelatedMemberId.Value);
                if (relatedMember != null && relatedMember.Status == "Active")
                {
                    await _attendanceService.MarkAttendanceAsync(churchId, relatedMember.UserId, attendanceDate, adminId);
                    familyCount++;
                }
            }

            // Check if this member is a family member of someone else
            var parentFamily = await _context.FamilyMembers
                .Where(fm => fm.RelatedMemberId == member.MemberId)
                .FirstOrDefaultAsync();

            if (parentFamily != null)
            {
                // Mark parent as present
                var parentMember = await _context.Members.FindAsync(parentFamily.MemberId);
                if (parentMember != null && parentMember.Status == "Active")
                {
                    await _attendanceService.MarkAttendanceAsync(churchId, parentMember.UserId, attendanceDate, adminId);
                    familyCount++;
                }

                // Mark siblings as present
                var siblings = await _context.FamilyMembers
                    .Where(fm => fm.MemberId == parentFamily.MemberId && fm.RelatedMemberId != member.MemberId && fm.RelatedMemberId.HasValue)
                    .ToListAsync();

                foreach (var sibling in siblings)
                {
                    var siblingMember = await _context.Members.FindAsync(sibling.RelatedMemberId.Value);
                    if (siblingMember != null && siblingMember.Status == "Active")
                    {
                        await _attendanceService.MarkAttendanceAsync(churchId, siblingMember.UserId, attendanceDate, adminId);
                        familyCount++;
                    }
                }
            }

            TempData["Success"] = $"Attendance marked for member and {familyCount} family member(s)!";
            return Json(new { success = true, message = "Family attendance marked successfully", familyCount = familyCount });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendanceMode(string mode)
        {
            var adminId = GetCurrentAdminId();
            var admin = await _context.AdminUsers.FindAsync(adminId);
            if (admin != null)
            {
                admin.AttendanceMode = mode;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAutoAbsent(int churchId, DateTime attendanceDate)
        {
            var adminId = GetCurrentAdminId();
            var count = await _attendanceService.MarkAutoAbsentAsync(churchId, attendanceDate, adminId);
            return Json(new { success = true, count = count, message = $"{count} members marked as absent" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTodayAttendance(int churchId, DateTime attendanceDate)
        {
            // Get all members from the church
            var allMembers = await _context.Members
                .Where(m => m.ChurchId == churchId && m.Status == "Active")
                .ToListAsync();

            // Get today's attendance records
            var attendanceRecords = await _context.Attendance
                .Where(a => a.ChurchId == churchId && a.AttendanceDate.Date == attendanceDate.Date)
                .Include(a => a.Member)
                .ToListAsync();

            // Create a list combining all members with their attendance status
            var records = allMembers.Select(member =>
            {
                var attendance = attendanceRecords.FirstOrDefault(a => a.MemberId == member.MemberId);
                return new
                {
                    attendanceId = attendance?.AttendanceId ?? 0,
                    userId = member.UserId,
                    memberName = member.Name,
                    phone = member.Phone,
                    isPresent = attendance?.IsPresent ?? false,
                    markedDate = attendance != null ? attendance.MarkedDate.ToString("hh:mm tt") : "-"
                };
            }).OrderByDescending(r => r.isPresent).ThenBy(r => r.memberName).ToList();

            return Json(new { success = true, records = records });
        }

        [HttpGet]
        public async Task<IActionResult> GetChurchInfo(int churchId)
        {
            var church = await _context.Churches.FindAsync(churchId);
            if (church == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                meetingDay1 = church.MeetingDay1,
                meetingDay2 = church.MeetingDay2
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPresentMembers(int churchId, DateTime attendanceDate)
        {
            try
            {
                // Get church meeting days
                var church = await _context.Churches.FindAsync(churchId);
                if (church == null)
                    return Json(new { success = false, message = "Church not found" });

                // Get today's present attendance records with member details
                var presentMembers = await _context.Attendance
                    .Where(a => a.ChurchId == churchId 
                           && a.AttendanceDate.Date == attendanceDate.Date 
                           && a.IsPresent)
                    .Include(a => a.Member)
                    .Select(a => a.Member)
                    .ToListAsync();

                if (presentMembers.Count == 0)
                {
                    return Json(new { success = true, members = new List<object>() });
                }

                // Get last 21 meeting days (3 weeks)
                var meetingDays = new[] { church.MeetingDay1, church.MeetingDay2 };
                var lastMeetingDates = GetLastMeetingDates(attendanceDate, meetingDays, 21);

                // Get attendance records for these dates
                var lastWeekAttendance = await _context.Attendance
                    .Where(a => a.ChurchId == churchId 
                           && lastMeetingDates.Contains(a.AttendanceDate.Date))
                    .OrderByDescending(a => a.AttendanceDate)
                    .ToListAsync();

                var result = presentMembers.Select(member => new
                {
                    userId = member.UserId,
                    name = member.Name,
                    phone = member.Phone,
                    meetingDayDates = lastMeetingDates.OrderByDescending(d => d).Select(d => new
                    {
                        date = d.ToString("yyyy-MM-dd"),
                        displayDate = d.ToString("MM/dd"),
                        dayName = d.ToString("dddd"),
                        dayShort = d.ToString("ddd")
                    }).ToList(),
                    lastWeekAttendance = lastWeekAttendance
                        .Where(a => a.MemberId == member.MemberId)
                        .Select(a => new
                        {
                            date = a.AttendanceDate.ToString("yyyy-MM-dd"),
                            isPresent = a.IsPresent
                        })
                        .ToList()
                }).ToList();

                return Json(new { success = true, members = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading present members: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAbsentMembers(int churchId, DateTime attendanceDate)
        {
            try
            {
                // Get church meeting days
                var church = await _context.Churches.FindAsync(churchId);
                if (church == null)
                    return Json(new { success = false, message = "Church not found" });

                // Get all active members from the church
                var allMembers = await _context.Members
                    .Where(m => m.ChurchId == churchId && m.Status == "Active")
                    .ToListAsync();

                // Get today's present attendance records
                var presentToday = await _context.Attendance
                    .Where(a => a.ChurchId == churchId 
                           && a.AttendanceDate.Date == attendanceDate.Date 
                           && a.IsPresent)
                    .Select(a => a.MemberId)
                    .ToListAsync();

                // Get absent members (those not marked present today)
                var absentMembers = allMembers.Where(m => !presentToday.Contains(m.MemberId)).ToList();

                // Get last 21 meeting days (3 weeks)
                var meetingDays = new[] { church.MeetingDay1, church.MeetingDay2 };
                var lastMeetingDates = GetLastMeetingDates(attendanceDate, meetingDays, 21);

                // Get attendance records for these dates
                var lastWeekAttendance = await _context.Attendance
                    .Where(a => a.ChurchId == churchId 
                           && lastMeetingDates.Contains(a.AttendanceDate.Date))
                    .OrderByDescending(a => a.AttendanceDate)
                    .ToListAsync();

                var result = absentMembers.Select(member => new
                {
                    userId = member.UserId,
                    name = member.Name,
                    phone = member.Phone,
                    meetingDayDates = lastMeetingDates.OrderByDescending(d => d).Select(d => new
                    {
                        date = d.ToString("yyyy-MM-dd"),
                        displayDate = d.ToString("MM/dd"),
                        dayName = d.ToString("dddd"),
                        dayShort = d.ToString("ddd")
                    }).ToList(),
                    lastWeekAttendance = lastWeekAttendance
                        .Where(a => a.MemberId == member.MemberId)
                        .Select(a => new
                        {
                            date = a.AttendanceDate.ToString("yyyy-MM-dd"),
                            isPresent = a.IsPresent
                        })
                        .ToList()
                }).ToList();

                return Json(new { success = true, members = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading absent members: " + ex.Message });
            }
        }

        private List<DateTime> GetLastMeetingDates(DateTime fromDate, string[] meetingDays, int count)
        {
            var dates = new List<DateTime>();
            var currentDate = fromDate.AddDays(-1); // Start from yesterday
            
            while (dates.Count < count && currentDate >= fromDate.AddDays(-180)) // Look back up to 180 days
            {
                var dayName = currentDate.ToString("dddd");
                if (meetingDays.Contains(dayName))
                {
                    dates.Add(currentDate.Date);
                }
                currentDate = currentDate.AddDays(-1);
            }
            
            return dates;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                var attendance = await _context.Attendance.FindAsync(id);
                if (attendance == null)
                    return Json(new { success = false, message = "Attendance record not found" });

                _context.Attendance.Remove(attendance);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Attendance deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting attendance: " + ex.Message });
            }
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}
