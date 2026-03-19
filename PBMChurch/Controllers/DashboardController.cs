using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Attributes;
using PBMChurch.Data;
using PBMChurch.Services;
using System.Security.Claims;

namespace PBMChurch.Controllers
{
    [RoleAuthorize("Admin")]
    public class DashboardController : Controller
    {
        private readonly IReportService _reportService;
        private readonly AppDbContext _context;

        public DashboardController(IReportService reportService, AppDbContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = await _reportService.GetDashboardDataAsync();
            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var adminId = GetCurrentAdminId();
                var today = DateTime.Today;
                var currentMonth = DateTime.Now.Month;
                var currentDay = DateTime.Now.Day;
                
                // Get member birthdays
                var todayBirthdays = await _context.Members
                    .FromSqlRaw(@"SELECT * FROM Members 
                                 WHERE Status = 'Active' 
                                 AND DateOfBirth IS NOT NULL
                                 AND MONTH(DateOfBirth) = {1} 
                                 AND DAY(DateOfBirth) = {2}
                                 AND NOT EXISTS (SELECT 1 FROM NotificationReads 
                                                WHERE MemberId = Members.MemberId 
                                                AND NotificationType = 'Birthday' 
                                                AND ReadDate = {3} 
                                                AND AdminId = {0})", adminId, currentMonth, currentDay, today)
                    .Select(m => new { 
                        Type = "Birthday", 
                        Name = m.Name ?? "Unknown", 
                        Message = "Today is " + (m.Name ?? "Unknown") + "'s birthday!",
                        Id = m.MemberId
                    })
                    .ToListAsync();

                // Get family member birthdays (exclude those who are already Members)
                var familyBirthdays = await _context.FamilyMembers
                    .FromSqlRaw(@"SELECT * FROM FamilyMembers 
                                 WHERE DateOfBirth IS NOT NULL
                                 AND MONTH(DateOfBirth) = {1} 
                                 AND DAY(DateOfBirth) = {2}
                                 AND RelatedMemberId IS NOT NULL
                                 AND NOT EXISTS (SELECT 1 FROM Members WHERE Members.MemberId = FamilyMembers.RelatedMemberId)
                                 AND NOT EXISTS (SELECT 1 FROM NotificationReads 
                                                WHERE MemberId = FamilyMembers.FamilyMemberId 
                                                AND NotificationType = 'FamilyBirthday' 
                                                AND ReadDate = {3} 
                                                AND AdminId = {0})", adminId, currentMonth, currentDay, today)
                    .Select(fm => new { 
                        Type = "FamilyBirthday", 
                        Name = fm.Name ?? "Unknown", 
                        Message = "Today is " + (fm.Name ?? "Unknown") + "'s birthday! (" + (fm.Relation ?? "Family") + ")",
                        Id = fm.FamilyMemberId
                    })
                    .ToListAsync();

                // Get member anniversaries
                var todayAnniversaries = await _context.Members
                    .FromSqlRaw(@"SELECT * FROM Members 
                                 WHERE Status = 'Active' 
                                 AND AnniversaryDate IS NOT NULL
                                 AND MONTH(AnniversaryDate) = {1} 
                                 AND DAY(AnniversaryDate) = {2}
                                 AND NOT EXISTS (SELECT 1 FROM NotificationReads 
                                                WHERE MemberId = Members.MemberId 
                                                AND NotificationType = 'Anniversary' 
                                                AND ReadDate = {3} 
                                                AND AdminId = {0})", adminId, currentMonth, currentDay, today)
                    .Select(m => new { 
                        Type = "Anniversary", 
                        Name = m.Name ?? "Unknown", 
                        Message = "Today is " + (m.Name ?? "Unknown") + "'s anniversary!",
                        Id = m.MemberId
                    })
                    .ToListAsync();

                // Get family member anniversaries (exclude those who are already Members)
                var familyAnniversaries = await _context.FamilyMembers
                    .FromSqlRaw(@"SELECT * FROM FamilyMembers 
                                 WHERE AnniversaryDate IS NOT NULL
                                 AND MONTH(AnniversaryDate) = {1} 
                                 AND DAY(AnniversaryDate) = {2}
                                 AND RelatedMemberId IS NOT NULL
                                 AND NOT EXISTS (SELECT 1 FROM Members WHERE Members.MemberId = FamilyMembers.RelatedMemberId)
                                 AND NOT EXISTS (SELECT 1 FROM NotificationReads 
                                                WHERE MemberId = FamilyMembers.FamilyMemberId 
                                                AND NotificationType = 'FamilyAnniversary' 
                                                AND ReadDate = {3} 
                                                AND AdminId = {0})", adminId, currentMonth, currentDay, today)
                    .Select(fm => new { 
                        Type = "FamilyAnniversary", 
                        Name = fm.Name ?? "Unknown", 
                        Message = "Today is " + (fm.Name ?? "Unknown") + "'s anniversary! (" + (fm.Relation ?? "Family") + ")",
                        Id = fm.FamilyMemberId
                    })
                    .ToListAsync();

                // Get prayer request notifications
                var todayPrayerRequests = await _context.PrayerRequests
                    .FromSqlRaw(@"SELECT * FROM PrayerRequests 
                                 WHERE CAST(CreatedDate AS DATE) = {1}
                                 AND NOT EXISTS (SELECT 1 FROM NotificationReads 
                                                WHERE RequestId = PrayerRequests.RequestId 
                                                AND NotificationType = 'PrayerRequest' 
                                                AND ReadDate = {1} 
                                                AND AdminId = {0})", adminId, today)
                    .Select(pr => new { 
                        Type = "PrayerRequest", 
                        Name = pr.Name ?? "Anonymous", 
                        Message = "New prayer request from " + (pr.Name ?? "Anonymous"),
                        Id = pr.RequestId
                    })
                    .ToListAsync();

                var notifications = todayBirthdays
                    .Concat(familyBirthdays)
                    .Concat(todayAnniversaries)
                    .Concat(familyAnniversaries)
                    .Concat(todayPrayerRequests)
                    .ToList();
                    
                return Json(new { count = notifications.Count, items = notifications });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0, items = new List<object>(), error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int memberId, string type)
        {
            try
            {
                var adminId = GetCurrentAdminId();
                
                // For PrayerRequest, use NULL for MemberId since RequestId is not a Member
                if (type == "PrayerRequest")
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO NotificationReads (MemberId, NotificationType, ReadDate, AdminId, RequestId) " +
                        "VALUES (NULL, {0}, {1}, {2}, {3})",
                        type, DateTime.Today, adminId, memberId);
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO NotificationReads (MemberId, NotificationType, ReadDate, AdminId) " +
                        "VALUES ({0}, {1}, {2}, {3})",
                        memberId, type, DateTime.Today, adminId);
                }
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveNotification(int memberId, string type)
        {
            try
            {
                var adminId = GetCurrentAdminId();
                
                // For PrayerRequest, use NULL for MemberId since RequestId is not a Member
                if (type == "PrayerRequest")
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO NotificationReads (MemberId, NotificationType, ReadDate, AdminId, RequestId) " +
                        "VALUES (NULL, {0}, {1}, {2}, {3})",
                        type, DateTime.Today, adminId, memberId);
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO NotificationReads (MemberId, NotificationType, ReadDate, AdminId) " +
                        "VALUES ({0}, {1}, {2}, {3})",
                        memberId, type, DateTime.Today, adminId);
                }
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}
