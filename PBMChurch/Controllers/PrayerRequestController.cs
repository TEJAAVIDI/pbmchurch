using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Attributes;
using PBMChurch.Data;
using PBMChurch.Models;
using System.Security.Claims;

namespace PBMChurch.Controllers
{
    [RoleAuthorize("Admin", "Member")]
    public class PrayerRequestController : Controller
    {
        private readonly AppDbContext _context;

        public PrayerRequestController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? churchFilter)
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            
            var query = _context.PrayerRequests
                .Include(p => p.Church)
                .Include(p => p.Member)
                .ThenInclude(m => m.Church)
                .Where(p => p.Status != "Closed");

            // Apply church filter
            if (churchFilter.HasValue)
            {
                if (churchFilter.Value == -1) // No Church filter
                {
                    query = query.Where(p => p.ChurchId == null);
                }
                else
                {
                    query = query.Where(p => p.ChurchId == churchFilter.Value);
                }
            }

            var prayerRequests = await query
                .OrderByDescending(p => p.RequestDate)
                .ToListAsync();

            ViewBag.SelectedChurchFilter = churchFilter;
            return View(prayerRequests);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrayerRequest prayerRequest)
        {
            if (ModelState.IsValid)
            {
                prayerRequest.CreatedBy = GetCurrentAdminId();
                prayerRequest.CreatedDate = DateTime.Now;
                prayerRequest.Status = "Active";
                
                _context.PrayerRequests.Add(prayerRequest);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Prayer request added successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", prayerRequest.ChurchId);
            return View(prayerRequest);
        }

        [HttpGet]
        public async Task<IActionResult> SearchMember(string name, int? churchId)
        {
            if (string.IsNullOrEmpty(name))
                return Json(new { found = false });

            var query = _context.Members.Where(m => m.Status == "Active");
            
            if (churchId.HasValue)
                query = query.Where(m => m.ChurchId == churchId);
                
            var member = await query
                .Where(m => m.Name.Contains(name))
                .Include(m => m.Church)
                .FirstOrDefaultAsync();

            if (member != null)
            {
                return Json(new
                {
                    found = true,
                    memberId = member.MemberId,
                    name = member.Name,
                    phone = member.Phone,
                    churchId = member.ChurchId,
                    churchName = member.Church?.ChurchName
                });
            }

            return Json(new { found = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                var prayerRequest = await _context.PrayerRequests.FindAsync(request.Id);
                if (prayerRequest != null)
                {
                    prayerRequest.Status = request.Status;
                    prayerRequest.ModifiedBy = GetCurrentAdminId();
                    prayerRequest.ModifiedDate = DateTime.Now;
                    if (request.Status == "Answered")
                        prayerRequest.AnsweredDate = DateTime.Now;
                        
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Prayer request not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateStatusRequest
        {
            public int Id { get; set; }
            public string Status { get; set; }
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}