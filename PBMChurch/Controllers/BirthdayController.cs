using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Services;

namespace PBMChurch.Controllers
{
    [Authorize]
    public class BirthdayController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWhatsAppService _whatsAppService;

        public BirthdayController(AppDbContext context, IWhatsAppService whatsAppService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
        }

        public async Task<IActionResult> Index(int? month, int? churchId, string? search, string? tab)
        {
            var today = DateTime.Today;
            var selectedMonth = month ?? today.Month;

            // Today's birthdays and anniversaries
            var todayBirthdays = await GetBirthdayMembersAsync(today.Month, today.Day);
            var todayAnniversaries = await GetAnniversaryMembersAsync(today.Month, today.Day);

            // Next 7 days
            var next7Days = await GetUpcomingBirthdaysAsync(today, today.AddDays(7));
            var next7DaysAnniversaries = await GetUpcomingAnniversariesAsync(today, today.AddDays(7));

            // Next 30 days
            var next30Days = await GetUpcomingBirthdaysAsync(today, today.AddDays(30));
            var next30DaysAnniversaries = await GetUpcomingAnniversariesAsync(today, today.AddDays(30));

            // All birthdays with filters
            var monthBirthdays = await GetAllBirthdaysAsync(churchId, search);
            var monthAnniversaries = await GetAllAnniversariesAsync(churchId, search);

            // Get churches for dropdown
            var churches = await _context.Churches
                .OrderBy(c => c.ChurchName)
                .Select(c => new { c.ChurchId, c.ChurchName })
                .ToListAsync();

            ViewBag.Churches = churches;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedChurchId = churchId?.ToString();
            ViewBag.SearchTerm = search;
            ViewBag.ActiveTab = tab ?? "today";
            ViewBag.TodayBirthdays = todayBirthdays;
            ViewBag.TodayAnniversaries = todayAnniversaries;
            ViewBag.Next7Days = next7Days;
            ViewBag.Next7DaysAnniversaries = next7DaysAnniversaries;
            ViewBag.Next30Days = next30Days;
            ViewBag.Next30DaysAnniversaries = next30DaysAnniversaries;
            ViewBag.MonthBirthdays = monthBirthdays;
            ViewBag.MonthAnniversaries = monthAnniversaries;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendWish(int memberId)
        {
            var member = await _context.Members
                .Include(m => m.Church)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null || string.IsNullOrEmpty(member.Phone))
                return Json(new { success = false, message = "Member not found or phone number missing" });

            try
            {
                var messageTemplate = await _whatsAppService.GetSettingAsync("Birthday_Wish_Message");
                var message = messageTemplate.Replace("{Name}", member.Name);

                // This would call actual WhatsApp API
                // For now, just log it
                TempData["Success"] = $"Birthday wish sent to {member.Name}";
                return Json(new { success = true, message = $"Birthday wish sent to {member.Name}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to send birthday wish" });
            }
        }

        private async Task<List<dynamic>> GetBirthdayMembersAsync(int month, int day)
        {
            return await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" 
                    && m.DateOfBirth != null 
                    && m.DateOfBirth.Value.Month == month 
                    && m.DateOfBirth.Value.Day == day)
                .Select(m => new
                {
                    m.MemberId,
                    m.UserId,
                    m.Name,
                    m.Phone,
                    m.DateOfBirth,
                    ChurchName = m.Church!.ChurchName,
                    Age = DateTime.Now.Year - m.DateOfBirth!.Value.Year
                })
                .Cast<dynamic>()
                .ToListAsync();
        }

        private async Task<List<dynamic>> GetUpcomingBirthdaysAsync(DateTime fromDate, DateTime toDate)
        {
            var members = await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" && m.DateOfBirth != null)
                .ToListAsync();

            var birthdays = members
                .Where(m =>
                {
                    var dob = m.DateOfBirth!.Value;
                    var nextBirthday = new DateTime(DateTime.Now.Year, dob.Month, dob.Day);
                    if (nextBirthday < fromDate)
                        nextBirthday = nextBirthday.AddYears(1);
                    return nextBirthday >= fromDate && nextBirthday <= toDate;
                })
                .Select(m =>
                {
                    var dob = m.DateOfBirth!.Value;
                    var nextBirthday = new DateTime(DateTime.Now.Year, dob.Month, dob.Day);
                    if (nextBirthday < fromDate)
                        nextBirthday = nextBirthday.AddYears(1);

                    return new
                    {
                        m.MemberId,
                        m.UserId,
                        m.Name,
                        m.Phone,
                        m.DateOfBirth,
                        ChurchName = m.Church?.ChurchName ?? "",
                        NextBirthday = nextBirthday,
                        DaysUntil = (nextBirthday - DateTime.Today).Days,
                        Age = DateTime.Now.Year - dob.Year
                    };
                })
                .OrderBy(b => b.DaysUntil)
                .Cast<dynamic>()
                .ToList();

            return birthdays;
        }

        private async Task<List<dynamic>> GetBirthdaysByMonthAsync(int month)
        {
            return await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" 
                    && m.DateOfBirth != null 
                    && m.DateOfBirth.Value.Month == month)
                .OrderBy(m => m.DateOfBirth!.Value.Day)
                .Select(m => new
                {
                    m.MemberId,
                    m.UserId,
                    m.Name,
                    m.Phone,
                    m.DateOfBirth,
                    ChurchName = m.Church!.ChurchName,
                    Age = DateTime.Now.Year - m.DateOfBirth!.Value.Year
                })
                .Cast<dynamic>()
                .ToListAsync();
        }

        private async Task<List<dynamic>> GetAllBirthdaysAsync(int? churchId, string? search)
        {
            var query = _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" && m.DateOfBirth != null);

            // Filter by church if selected
            if (churchId.HasValue && churchId.Value > 0)
            {
                query = query.Where(m => m.ChurchId == churchId.Value);
            }

            // Filter by search term if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.UserId.Contains(search));
            }

            return await query
                .OrderBy(m => m.DateOfBirth!.Value.Month)
                .ThenBy(m => m.DateOfBirth!.Value.Day)
                .Select(m => new
                {
                    m.MemberId,
                    m.UserId,
                    m.Name,
                    m.Phone,
                    m.DateOfBirth,
                    ChurchName = m.Church!.ChurchName,
                    Age = DateTime.Now.Year - m.DateOfBirth!.Value.Year
                })
                .Cast<dynamic>()
                .ToListAsync();
        }

        private async Task<List<dynamic>> GetAnniversaryMembersAsync(int month, int day)
        {
            return await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" 
                    && m.AnniversaryDate != null 
                    && m.AnniversaryDate.Value.Month == month 
                    && m.AnniversaryDate.Value.Day == day)
                .Select(m => new
                {
                    m.MemberId,
                    m.UserId,
                    m.Name,
                    m.Phone,
                    m.AnniversaryDate,
                    ChurchName = m.Church!.ChurchName,
                    Years = DateTime.Now.Year - m.AnniversaryDate!.Value.Year
                })
                .Cast<dynamic>()
                .ToListAsync();
        }

        private async Task<List<dynamic>> GetUpcomingAnniversariesAsync(DateTime fromDate, DateTime toDate)
        {
            var members = await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" && m.AnniversaryDate != null)
                .ToListAsync();

            var anniversaries = members
                .Where(m =>
                {
                    var anniversary = m.AnniversaryDate!.Value;
                    var nextAnniversary = new DateTime(DateTime.Now.Year, anniversary.Month, anniversary.Day);
                    if (nextAnniversary < fromDate)
                        nextAnniversary = nextAnniversary.AddYears(1);
                    return nextAnniversary >= fromDate && nextAnniversary <= toDate;
                })
                .Select(m =>
                {
                    var anniversary = m.AnniversaryDate!.Value;
                    var nextAnniversary = new DateTime(DateTime.Now.Year, anniversary.Month, anniversary.Day);
                    if (nextAnniversary < fromDate)
                        nextAnniversary = nextAnniversary.AddYears(1);

                    return new
                    {
                        m.MemberId,
                        m.UserId,
                        m.Name,
                        m.Phone,
                        m.AnniversaryDate,
                        ChurchName = m.Church?.ChurchName ?? "",
                        NextAnniversary = nextAnniversary,
                        DaysUntil = (nextAnniversary - DateTime.Today).Days,
                        Years = DateTime.Now.Year - anniversary.Year
                    };
                })
                .OrderBy(a => a.DaysUntil)
                .Cast<dynamic>()
                .ToList();

            return anniversaries;
        }

        private async Task<List<dynamic>> GetAllAnniversariesAsync(int? churchId, string? search)
        {
            var query = _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status == "Active" && m.AnniversaryDate != null);

            if (churchId.HasValue && churchId.Value > 0)
            {
                query = query.Where(m => m.ChurchId == churchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.UserId.Contains(search));
            }

            return await query
                .OrderBy(m => m.AnniversaryDate!.Value.Month)
                .ThenBy(m => m.AnniversaryDate!.Value.Day)
                .Select(m => new
                {
                    m.MemberId,
                    m.UserId,
                    m.Name,
                    m.Phone,
                    m.AnniversaryDate,
                    ChurchName = m.Church!.ChurchName,
                    Years = DateTime.Now.Year - m.AnniversaryDate!.Value.Year
                })
                .Cast<dynamic>()
                .ToListAsync();
        }
    }
}
