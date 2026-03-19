using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models.ViewModels;
using PBMChurch.Services;

namespace PBMChurch.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IReportService _reportService;

        public ReportController(AppDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<IActionResult> Attendance()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            
            var filter = new ReportFilterViewModel
            {
                FromDate = DateTime.Today.AddDays(-30),
                ToDate = DateTime.Today
            };

            var model = await _reportService.GetAttendanceReportAsync(filter);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Attendance(ReportFilterViewModel filter)
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", filter.ChurchId);
            var model = await _reportService.GetAttendanceReportAsync(filter);
            return View(model);
        }

        public async Task<IActionResult> Financial(int? churchId, int? year, int? month)
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", churchId);
            
            var currentYear = year ?? DateTime.Now.Year;
            var currentMonth = month ?? DateTime.Now.Month;

            var model = await _reportService.GetFinancialReportAsync(churchId, currentYear, currentMonth);
            return View(model);
        }
    }
}
