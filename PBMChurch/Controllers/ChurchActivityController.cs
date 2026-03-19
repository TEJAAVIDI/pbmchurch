using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;

namespace PBMChurch.Controllers
{
    public class ChurchActivityController : Controller
    {
        private readonly AppDbContext _context;

        public ChurchActivityController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var activities = await _context.ChurchActivities
                .Where(a => a.IsActive)
                .OrderBy(a => a.FromDateTime)
                .ToListAsync();
            return View(activities);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChurchActivity activity, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        activity.ImageData = memoryStream.ToArray();
                        activity.ImageContentType = imageFile.ContentType;
                    }
                }
                _context.ChurchActivities.Add(activity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Church activity created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var activity = await _context.ChurchActivities.FindAsync(id);
            if (activity == null) return NotFound();

            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChurchActivity activity, IFormFile? imageFile)
        {
            if (id != activity.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            activity.ImageData = memoryStream.ToArray();
                            activity.ImageContentType = imageFile.ContentType;
                        }
                    }
                    _context.Update(activity);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Church activity updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityExists(activity.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _context.ChurchActivities.FindAsync(id);
            if (activity != null)
            {
                activity.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Church activity deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetActiveActivities()
        {
            var activities = await _context.ChurchActivities
                .Where(a => a.IsActive && a.FromDateTime >= DateTime.Today)
                .OrderBy(a => a.FromDateTime)
                .Select(a => new
                {
                    a.Title,
                    a.Description,
                    a.Location,
                    FromDateTime = a.FromDateTime.ToString("MMM dd, yyyy hh:mm tt"),
                    ToDateTime = a.ToDateTime.ToString("MMM dd, yyyy hh:mm tt")
                })
                .ToListAsync();

            return Json(activities);
        }

        public async Task<IActionResult> GetImage(int id)
        {
            var activity = await _context.ChurchActivities.FindAsync(id);
            if (activity?.ImageData != null)
            {
                return File(activity.ImageData, activity.ImageContentType ?? "image/jpeg");
            }
            return NotFound();
        }

        private bool ActivityExists(int id)
        {
            return _context.ChurchActivities.Any(e => e.Id == id);
        }
    }
}