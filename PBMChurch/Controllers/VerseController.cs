using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;
using System.Security.Claims;

namespace PBMChurch.Controllers
{
    [Authorize]
    public class VerseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public VerseController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var verses = await _context.Verses
                .OrderByDescending(v => v.UploadedDate)
                .ToListAsync();

            // Get wish templates for messaging
            var bibleTemplates = await _context.WishTemplates
                .Where(w => w.Type == "Bible" && w.IsActive)
                .ToListAsync();
            
            var eventTemplates = await _context.WishTemplates
                .Where(w => w.Type == "Event" && w.IsActive)
                .ToListAsync();
            
            var generalTemplates = await _context.WishTemplates
                .Where(w => w.Type == "General" && w.IsActive)
                .ToListAsync();

            ViewBag.BibleTemplates = bibleTemplates;
            ViewBag.EventTemplates = eventTemplates;
            ViewBag.GeneralTemplates = generalTemplates;

            // Calculate today's Bible reading
            var dayOfYear = DateTime.Now.DayOfYear;
            var chaptersPerDay = 1189.0 / 365.0; // Total chapters / days in year
            var startChapter = (int)Math.Ceiling((dayOfYear - 1) * chaptersPerDay) + 1;
            var endChapter = (int)Math.Ceiling(dayOfYear * chaptersPerDay);
            var readingRange = BibleStructure.GetReadingRange(startChapter, endChapter);
            ViewBag.BibleReading = readingRange;
            ViewBag.DayOfYear = dayOfYear;
            ViewBag.Progress = Math.Round((dayOfYear / 365.0) * 100, 1);

            // Format Bible message from template
            var bibleTemplate = bibleTemplates.FirstOrDefault();
            if (bibleTemplate != null)
            {
                var formattedMessage = bibleTemplate.MessageText?.Replace("{StartChapter}", readingRange.Split(" to ")[0])
                    .Replace("{EndChapter}", readingRange.Split(" to ").Length > 1 ? readingRange.Split(" to ")[1] : readingRange);
                ViewBag.FormattedBibleMessage = formattedMessage;
            }

            // For modal dropdown (show only WhatsApp-enabled active churches)
            var churches = await _context.Churches
                .Where(c => c.Status == "Active" && c.WhatsAppEnabled)
                .OrderBy(c => c.ChurchName)
                .ToListAsync();
            ViewBag.Churches = churches;

            // Skip sent messages for now
            ViewBag.SentMessages = new List<object>();

            return View(verses);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTextToGroups(string verseText, string youtubeLink, List<int> selectedChurchIds)
        {
            if (string.IsNullOrWhiteSpace(verseText) || selectedChurchIds == null || !selectedChurchIds.Any())
            {
                TempData["Error"] = "Please enter verse text and select at least one church.";
                return RedirectToAction(nameof(Index));
            }

            var churches = await _context.Churches
                .Where(c => selectedChurchIds.Contains(c.ChurchId) && c.Status == "Active" && c.WhatsAppEnabled)
                .ToListAsync();

            int successCount = 0;
            int failCount = 0;
            var errors = new List<string>();
            var successMessages = new List<string>();

            // Compose message with YouTube link if provided
            string messageToSend = verseText;
            if (!string.IsNullOrWhiteSpace(youtubeLink))
            {
                messageToSend += $"\nYouTube: {youtubeLink}";
            }

            // Skip message tracking for now

            foreach (var church in churches)
            {
                if (string.IsNullOrEmpty(church.WhatsAppInstanceId) || string.IsNullOrEmpty(church.WhatsAppApiToken))
                {
                    errors.Add($"{church.ChurchName}: Missing WhatsApp credentials");
                    continue;
                }
                if (string.IsNullOrEmpty(church.GeneralGroupId))
                {
                    errors.Add($"{church.ChurchName}: No General Group configured");
                    continue;
                }
                try
                {
                    var (success, errorMsg) = await SendTextToWhatsAppGroup(
                        church.WhatsAppInstanceId,
                        church.WhatsAppApiToken,
                        church.WhatsAppApiUrl ?? "https://api.green-api.com",
                        church.GeneralGroupId,
                        messageToSend
                    );
                    if (success)
                    {
                        successCount++;
                        successMessages.Add($"✓ {church.ChurchName}");
                    }
                    else
                    {
                        failCount++;
                        errors.Add($"✗ {church.ChurchName}: {errorMsg}");
                    }
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    failCount++;
                    errors.Add($"✗ {church.ChurchName}: {ex.Message}");
                }
            }

            var resultMessage = $"Sent to General Group of selected churches. Total: {successCount + failCount} churches. ";
            resultMessage += $"Success: {successCount}, Failed: {failCount}";
            if (successCount > 0)
                resultMessage += $"\n\nSuccessful Churches: {string.Join(", ", successMessages)}";
            if (errors.Any())
                resultMessage += $"\n\nFailed: {string.Join(", ", errors)}";
            if (failCount == 0)
                TempData["Success"] = resultMessage;
            else if (successCount > 0)
                TempData["Success"] = resultMessage;
            else
                TempData["Error"] = resultMessage;

            return RedirectToAction(nameof(Index));
        }

        private async Task<(bool success, string errorMessage)> SendTextToWhatsAppGroup(string instanceId, string apiToken, string apiUrl, string groupId, string text)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(60);
                var url = $"{apiUrl}/waInstance{instanceId}/sendMessage/{apiToken}";
                var payload = new
                {
                    chatId = groupId,
                    message = text
                };
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"API Error {response.StatusCode}: {responseContent}");
                }
                if (responseContent.Contains("\"error\"") || responseContent.Contains("false"))
                {
                    return (false, $"API returned error: {responseContent}");
                }
                return (true, "Success");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select an image file");
                return View();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Only image files (.jpg, .jpeg, .png, .gif) are allowed");
                return View();
            }

            try
            {
                // Create verses folder if it doesn't exist
                var versesFolder = Path.Combine(_environment.WebRootPath, "uploads", "verses");
                if (!Directory.Exists(versesFolder))
                    Directory.CreateDirectory(versesFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(versesFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save to database
                var verse = new Verse
                {
                    ImageFileName = file.FileName,
                    StorageURL = $"/uploads/verses/{uniqueFileName}",
                    UploadedDate = DateTime.Now,
                    UploadedBy = GetCurrentAdminId(),
                    IsActive = true
                };

                _context.Verses.Add(verse);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Verse image uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error uploading file: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToGroups(int id)
        {
            try
            {
                var verse = await _context.Verses.FindAsync(id);
                if (verse == null)
                {
                    TempData["Error"] = "Verse not found!";
                    return RedirectToAction(nameof(Index));
                }

                // Get all active churches with WhatsApp enabled
                var churches = await _context.Churches
                    .Where(c => c.Status == "Active" && c.WhatsAppEnabled)
                    .ToListAsync();

                if (!churches.Any())
                {
                    TempData["Error"] = "No churches with WhatsApp enabled found!";
                    return RedirectToAction(nameof(Index));
                }

                int successCount = 0;
                int failCount = 0;
                var errors = new List<string>();
                var successMessages = new List<string>();

                // Get the full image URL - must be publicly accessible
                var imageUrl = $"{Request.Scheme}://{Request.Host}{verse.StorageURL}";

                // Send to GENERAL GROUP ONLY for each church
                foreach (var church in churches)
                {
                    if (string.IsNullOrEmpty(church.WhatsAppInstanceId) || string.IsNullOrEmpty(church.WhatsAppApiToken))
                    {
                        errors.Add($"{church.ChurchName}: Missing WhatsApp credentials");
                        continue;
                    }

                    // Check if General Group ID exists
                    if (string.IsNullOrEmpty(church.GeneralGroupId))
                    {
                        errors.Add($"{church.ChurchName}: No General Group configured");
                        continue;
                    }

                    try
                    {
                        var (success, errorMsg) = await SendImageToWhatsAppGroup(
                            church.WhatsAppInstanceId,
                            church.WhatsAppApiToken,
                            church.WhatsAppApiUrl ?? "https://api.green-api.com",
                            church.GeneralGroupId,
                            imageUrl,
                            verse.ImageFileName
                        );

                        if (success)
                        {
                            successCount++;
                            successMessages.Add($"✓ {church.ChurchName}");
                        }
                        else
                        {
                            failCount++;
                            errors.Add($"✗ {church.ChurchName}: {errorMsg}");
                        }

                        // Small delay between messages to avoid rate limiting
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errors.Add($"✗ {church.ChurchName}: {ex.Message}");
                    }
                }

                // Update verse posting statistics
                verse.LastPostedDate = DateTime.Now;
                verse.PostCount++;
                await _context.SaveChangesAsync();

                // Build detailed message
                var resultMessage = $"Sent to General Group of all churches. Total: {successCount + failCount} churches. ";
                resultMessage += $"Success: {successCount}, Failed: {failCount}";

                if (successCount > 0)
                {
                    resultMessage += $"\n\nSuccessful Churches: {string.Join(", ", successMessages)}";
                }

                if (errors.Any())
                {
                    resultMessage += $"\n\nFailed: {string.Join(", ", errors)}";
                }

                if (failCount == 0)
                    TempData["Success"] = resultMessage;
                else if (successCount > 0)
                    TempData["Success"] = resultMessage;
                else
                    TempData["Error"] = resultMessage;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending verse: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<(bool success, string errorMessage)> SendImageToWhatsAppGroup(string instanceId, string apiToken, string apiUrl, 
            string groupId, string imageUrl, string caption)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                var url = $"{apiUrl}/waInstance{instanceId}/sendFileByUrl/{apiToken}";

                var payload = new
                {
                    chatId = groupId,
                    urlFile = imageUrl,
                    fileName = caption,
                    caption = $"📖 Verse of the Day 🙏"
                };

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"API Error {response.StatusCode}: {responseContent}");
                }

                // Check if response contains error
                if (responseContent.Contains("\"error\"") || responseContent.Contains("false"))
                {
                    return (false, $"API returned error: {responseContent}");
                }

                return (true, "Success");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var verse = await _context.Verses.FindAsync(id);
            if (verse != null)
            {
                verse.IsActive = !verse.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Verse {(verse.IsActive ? "activated" : "deactivated")} successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var verse = await _context.Verses.FindAsync(id);
            if (verse != null)
            {
                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath, verse.StorageURL.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Verses.Remove(verse);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Verse deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}
