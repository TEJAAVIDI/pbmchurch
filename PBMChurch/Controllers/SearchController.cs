using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;

namespace PBMChurch.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { success = false, message = "Please enter at least 2 characters" });
            }

            var searchTerm = query.ToLower();

            // Search Members
            var members = await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Name.ToLower().Contains(searchTerm) || 
                           m.UserId.ToLower().Contains(searchTerm) ||
                           m.Phone.ToLower().Contains(searchTerm))
                .Take(5)
                .Select(m => new
                {
                    id = m.MemberId,
                    name = m.Name,
                    userId = m.UserId,
                    church = m.Church.ChurchName,
                    phone = m.Phone,
                    type = "Member"
                })
                .ToListAsync();

            // Search Churches
            var churches = await _context.Churches
                .Where(c => c.ChurchName.ToLower().Contains(searchTerm) || 
                           c.Location.ToLower().Contains(searchTerm))
                .Take(5)
                .Select(c => new
                {
                    id = c.ChurchId,
                    name = c.ChurchName,
                    location = c.Location,
                    status = c.Status,
                    type = "Church"
                })
                .ToListAsync();

            // Search Prayer Requests
            var prayers = await _context.PrayerRequests
                .Include(p => p.Member)
                .Where(p => p.Title.ToLower().Contains(searchTerm) || 
                           p.Request.ToLower().Contains(searchTerm) ||
                           p.Member.Name.ToLower().Contains(searchTerm))
                .Take(5)
                .Select(p => new
                {
                    id = p.RequestId,
                    title = p.Title,
                    memberName = p.Member.Name,
                    status = p.Status,
                    type = "Prayer"
                })
                .ToListAsync();

            // Search Navigation/Modules
            var modules = new List<object>();
            var menuItems = new[]
            {
                new { name = "Dashboard", url = "/Dashboard/Index", icon = "fa-tachometer-alt", keywords = "dashboard home overview stats" },
                new { name = "Churches", url = "/Church/Index", icon = "fa-church", keywords = "churches church locations" },
                new { name = "Members", url = "/Member/Index", icon = "fa-users", keywords = "members people contacts" },
                new { name = "Attendance", url = "/Attendance/Index", icon = "fa-clipboard-check", keywords = "attendance present absent" },
                new { name = "Prayer Requests", url = "/PrayerRequest/Index", icon = "fa-praying-hands", keywords = "prayer requests prayers" },
                new { name = "User Management", url = "/UserManagement/Index", icon = "fa-user-shield", keywords = "users admin management" },
                new { name = "Income", url = "/Finance/Income", icon = "fa-arrow-up", keywords = "income money finance" },
                new { name = "Expense", url = "/Finance/Expense", icon = "fa-arrow-down", keywords = "expense money finance" },
                new { name = "Birthdays", url = "/Birthday/Index", icon = "fa-birthday-cake", keywords = "birthdays celebrations" },
                new { name = "Task Calendar", url = "/TaskCalendar/Index", icon = "fa-calendar-alt", keywords = "tasks calendar events" },
                new { name = "Messages", url = "/Verse/Index", icon = "fa-comments", keywords = "messages verses daily" },
                new { name = "Wishes Management", url = "/Wishes/Index", icon = "fa-heart", keywords = "wishes greetings" },
                new { name = "Gallery", url = "/Gallery/Index", icon = "fa-images", keywords = "gallery photos images" },
                new { name = "Church Activities", url = "/ChurchActivity/Index", icon = "fa-calendar-alt", keywords = "activities events" },
                new { name = "Automation Settings", url = "/Automation/Settings", icon = "fa-sliders-h", keywords = "settings automation config" },
                new { name = "WhatsApp Groups", url = "/Automation/ShowGroups", icon = "fa-whatsapp", keywords = "whatsapp groups messaging" }
            };

            modules = menuItems
                .Where(m => m.name.ToLower().Contains(searchTerm) || m.keywords.ToLower().Contains(searchTerm))
                .Take(5)
                .Select(m => new { name = m.name, url = m.url, icon = m.icon, type = "Module" } as object)
                .ToList();

            var results = new
            {
                success = true,
                members = members,
                churches = churches,
                prayers = prayers,
                modules = modules,
                totalCount = members.Count + churches.Count + prayers.Count + modules.Count
            };

            return Json(results);
        }
    }
}
