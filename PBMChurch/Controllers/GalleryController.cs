using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;

namespace PBMChurch.Controllers
{
    public class GalleryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<GalleryController> _logger;

        public GalleryController(AppDbContext context, IWebHostEnvironment environment, ILogger<GalleryController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: Gallery
        public async Task<IActionResult> Index()
        {
            var galleries = await _context.GalleryImages
                .Include(g => g.Church)
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();

            ViewBag.Churches = await _context.Churches
                .Where(c => c.Status == "Active")
                .Select(c => new { c.ChurchId, c.ChurchName })
                .ToListAsync();

            return View(galleries);
        }

        // GET: Gallery/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Churches = await _context.Churches
                .Where(c => c.Status == "Active")
                .Select(c => new { c.ChurchId, c.ChurchName })
                .ToListAsync();

            return View();
        }

        // POST: Gallery/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GalleryImage gallery, IFormFile imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Only image files (JPG, PNG, GIF) are allowed.";
                        ViewBag.Churches = await _context.Churches
                            .Where(c => c.Status == "Active")
                            .Select(c => new { c.ChurchId, c.ChurchName })
                            .ToListAsync();
                        return View(gallery);
                    }

                    // Validate file size (5MB max)
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File size must be less than 5MB.";
                        ViewBag.Churches = await _context.Churches
                            .Where(c => c.Status == "Active")
                            .Select(c => new { c.ChurchId, c.ChurchName })
                            .ToListAsync();
                        return View(gallery);
                    }

                    // Create gallery folder if it doesn't exist
                    var galleryFolder = Path.Combine(_environment.WebRootPath, "images", "gallery");
                    if (!Directory.Exists(galleryFolder))
                    {
                        Directory.CreateDirectory(galleryFolder);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(galleryFolder, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Set the image path
                    gallery.ImagePath = $"/images/gallery/{fileName}";
                }
                else
                {
                    TempData["Error"] = "Please select an image file.";
                    ViewBag.Churches = await _context.Churches
                        .Where(c => c.Status == "Active")
                        .Select(c => new { c.ChurchId, c.ChurchName })
                        .ToListAsync();
                    return View(gallery);
                }

                gallery.UploadedDate = DateTime.Now;
                gallery.IsActive = true;

                _context.GalleryImages.Add(gallery);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gallery image uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading gallery image");
                TempData["Error"] = "Error uploading image. Please try again.";
                ViewBag.Churches = await _context.Churches
                    .Where(c => c.Status == "Active")
                    .Select(c => new { c.ChurchId, c.ChurchName })
                    .ToListAsync();
                return View(gallery);
            }
        }

        // GET: Gallery/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gallery = await _context.GalleryImages.FindAsync(id);
            if (gallery == null)
            {
                return NotFound();
            }

            ViewBag.Churches = await _context.Churches
                .Where(c => c.Status == "Active")
                .Select(c => new { c.ChurchId, c.ChurchName })
                .ToListAsync();

            return View(gallery);
        }

        // POST: Gallery/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GalleryImage gallery, IFormFile? imageFile)
        {
            if (id != gallery.ImageId)
            {
                return NotFound();
            }

            try
            {
                var existingGallery = await _context.GalleryImages.FindAsync(id);
                if (existingGallery == null)
                {
                    return NotFound();
                }

                // Update image if new file uploaded
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Only image files (JPG, PNG, GIF) are allowed.";
                        ViewBag.Churches = await _context.Churches
                            .Where(c => c.Status == "Active")
                            .Select(c => new { c.ChurchId, c.ChurchName })
                            .ToListAsync();
                        return View(gallery);
                    }

                    // Delete old image
                    if (!string.IsNullOrEmpty(existingGallery.ImagePath))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, existingGallery.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Create gallery folder if it doesn't exist
                    var galleryFolder = Path.Combine(_environment.WebRootPath, "images", "gallery");
                    if (!Directory.Exists(galleryFolder))
                    {
                        Directory.CreateDirectory(galleryFolder);
                    }

                    // Save new image
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(galleryFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    existingGallery.ImagePath = $"/images/gallery/{fileName}";
                }

                // Update other properties
                existingGallery.Title = gallery.Title;
                existingGallery.Description = gallery.Description;
                existingGallery.ChurchId = gallery.ChurchId;
                existingGallery.Category = gallery.Category;
                existingGallery.DisplayOrder = gallery.DisplayOrder;
                existingGallery.IsActive = gallery.IsActive;

                _context.Update(existingGallery);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gallery image updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating gallery image");
                TempData["Error"] = "Error updating image. Please try again.";
                ViewBag.Churches = await _context.Churches
                    .Where(c => c.Status == "Active")
                    .Select(c => new { c.ChurchId, c.ChurchName })
                    .ToListAsync();
                return View(gallery);
            }
        }

        // POST: Gallery/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var gallery = await _context.GalleryImages.FindAsync(id);
                if (gallery == null)
                {
                    TempData["Error"] = "Gallery image not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete image file
                if (!string.IsNullOrEmpty(gallery.ImagePath))
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, gallery.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.GalleryImages.Remove(gallery);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gallery image deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting gallery image");
                TempData["Error"] = "Error deleting image. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
