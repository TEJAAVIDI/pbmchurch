using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Services;

namespace PBMChurch.Controllers
{
    public class TestAnniversaryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWhatsAppService _whatsAppService;

        public TestAnniversaryController(AppDbContext context, IWhatsAppService whatsAppService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
        }

        public async Task<IActionResult> TestNow()
        {
            try
            {
                var today = DateTime.Today;
                var settings = await _context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
                
                var anniversaryEnabled = settings.ContainsKey("Anniversary_Wish_Enabled") && settings["Anniversary_Wish_Enabled"] == "true";
                var anniversaryTime = settings.ContainsKey("Anniversary_Wish_Time") ? settings["Anniversary_Wish_Time"] : "14:42";
                
                var members = await _context.Members
                    .Where(m => m.Status == "Active" 
                        && m.AnniversaryDate != null
                        && m.AnniversaryDate.Value.Month == today.Month
                        && m.AnniversaryDate.Value.Day == today.Day)
                    .ToListAsync();

                var result = new
                {
                    Today = today.ToString("yyyy-MM-dd"),
                    AnniversaryEnabled = anniversaryEnabled,
                    AnniversaryTime = anniversaryTime,
                    MembersWithAnniversaryToday = members.Count,
                    Members = members.Select(m => new {
                        m.Name,
                        m.Phone,
                        AnniversaryDate = m.AnniversaryDate?.ToString("yyyy-MM-dd"),
                        m.ChurchId
                    }).ToList()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> SendTestAnniversary()
        {
            try
            {
                await _whatsAppService.SendAnniversaryWishesAsync();
                return Json(new { success = true, message = "Anniversary wishes sent!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}