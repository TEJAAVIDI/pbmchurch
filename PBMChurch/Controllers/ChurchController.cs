using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Attributes;
using PBMChurch.Data;
using PBMChurch.Models;
using System.Security.Claims;

namespace PBMChurch.Controllers
{
    [RoleAuthorize("Admin")]
    public class ChurchController : Controller
    {
        private readonly AppDbContext _context;

        public ChurchController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var churches = await _context.Churches
                .OrderBy(c => c.ChurchName)
                .ToListAsync();
            return View(churches);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Church church)
        {
            if (ModelState.IsValid)
            {
                church.CreatedBy = GetCurrentAdminId();
                church.CreatedDate = DateTime.Now;
                _context.Churches.Add(church);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Church created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(church);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var church = await _context.Churches.FindAsync(id);
            if (church == null)
                return NotFound();
            return View(church);
        }

        public async Task<IActionResult> Details(int id)
        {
            var church = await _context.Churches.FindAsync(id);
            if (church == null)
                return NotFound();

            var viewModel = new
            {
                Church = church,
                TotalMembers = await _context.Members.CountAsync(m => m.ChurchId == id && m.Status == "Active"),
                TotalFamilies = await _context.Members.Where(m => m.ChurchId == id && m.Status == "Active").Select(m => m.Family).Distinct().CountAsync(),
                TotalIncome = await _context.Income.Where(i => i.ChurchId == id).SumAsync(i => i.Amount),
                TotalExpense = await _context.Expenses.Where(e => e.ChurchId == id).SumAsync(e => e.Amount),
                MonthlyIncome = await _context.Income.Where(i => i.ChurchId == id && i.IncomeDate.Month == DateTime.Now.Month && i.IncomeDate.Year == DateTime.Now.Year).SumAsync(i => i.Amount),
                MonthlyExpense = await _context.Expenses.Where(e => e.ChurchId == id && e.ExpenseDate.Month == DateTime.Now.Month && e.ExpenseDate.Year == DateTime.Now.Year).SumAsync(e => e.Amount),
                TodayBirthdays = await _context.Members.Where(m => m.ChurchId == id && m.Status == "Active" && m.DateOfBirth.HasValue && m.DateOfBirth.Value.Month == DateTime.Now.Month && m.DateOfBirth.Value.Day == DateTime.Now.Day).CountAsync(),
                UpcomingBirthdays = await _context.Members.Where(m => m.ChurchId == id && m.Status == "Active" && m.DateOfBirth.HasValue && m.DateOfBirth.Value.Month == DateTime.Now.Month && m.DateOfBirth.Value.Day > DateTime.Now.Day).CountAsync(),
                TodayAnniversaries = await _context.Members.Where(m => m.ChurchId == id && m.Status == "Active" && m.AnniversaryDate.HasValue && m.AnniversaryDate.Value.Month == DateTime.Now.Month && m.AnniversaryDate.Value.Day == DateTime.Now.Day).CountAsync(),
                RecentAttendance = await _context.Attendance.Where(a => a.Member.ChurchId == id && a.AttendanceDate >= DateTime.Now.AddDays(-30)).GroupBy(a => a.AttendanceDate).Select(g => new { Date = g.Key, Present = g.Count(a => a.IsPresent), Total = g.Count() }).OrderByDescending(x => x.Date).Take(10).ToListAsync(),
                FamilyStats = await _context.Members.Where(m => m.ChurchId == id && m.Status == "Active" && !string.IsNullOrEmpty(m.Family)).GroupBy(m => m.Family).Select(g => new { FamilyName = g.Key, MemberCount = g.Count() }).OrderByDescending(f => f.MemberCount).Take(10).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Church church)
        {
            if (id != church.ChurchId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    church.ModifiedBy = GetCurrentAdminId();
                    church.ModifiedDate = DateTime.Now;
                    _context.Update(church);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Church updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChurchExists(church.ChurchId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(church);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var church = await _context.Churches.FindAsync(id);
            if (church != null)
            {
                // Soft delete - set status to inactive
                church.Status = "Inactive";
                church.ModifiedBy = GetCurrentAdminId();
                church.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Church deactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var church = await _context.Churches.FindAsync(id);
            if (church != null)
            {
                church.Status = "Active";
                church.ModifiedBy = GetCurrentAdminId();
                church.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Church reactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ChurchExists(int id)
        {
            return _context.Churches.Any(e => e.ChurchId == id);
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}
