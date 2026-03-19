using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;

namespace PBMChurch.Controllers
{
    public class TestConnectionRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
    }

    [Authorize]
    public class AutomationController : Controller
    {
        private readonly AppDbContext _context;

        public AutomationController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Settings()
        {
            // Load all settings
            var settings = await _context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

            // WhatsApp settings
            ViewBag.InstanceId = settings.ContainsKey("WhatsApp_Instance_ID") ? settings["WhatsApp_Instance_ID"] : "";
            ViewBag.ApiKey = settings.ContainsKey("WhatsApp_API_Key") ? settings["WhatsApp_API_Key"] : "";
            ViewBag.ApiUrl = settings.ContainsKey("WhatsApp_API_URL") ? settings["WhatsApp_API_URL"] : "https://api.green-api.com";
            ViewBag.PhoneNumber = settings.ContainsKey("WhatsApp_Phone") ? settings["WhatsApp_Phone"] : "";
            ViewBag.WhatsAppEnabled = settings.ContainsKey("WhatsApp_Enabled") ? settings["WhatsApp_Enabled"] == "true" : false;

            // Group settings
            ViewBag.BirthdayGroupId = settings.ContainsKey("Birthday_Group_ID") ? settings["Birthday_Group_ID"] : "";
            ViewBag.GeneralGroupId = settings.ContainsKey("General_Group_ID") ? settings["General_Group_ID"] : "";
            ViewBag.PrayerGroupId = settings.ContainsKey("Prayer_Group_ID") ? settings["Prayer_Group_ID"] : "";

            // Birthday settings
            ViewBag.EnableBirthdayWishes = settings.ContainsKey("Birthday_Wish_Enabled") ? settings["Birthday_Wish_Enabled"] == "true" : false;
            ViewBag.BirthdayWishTime = settings.ContainsKey("Birthday_Wish_Time") ? settings["Birthday_Wish_Time"] : "09:00";
            ViewBag.BirthdayMessage = settings.ContainsKey("Birthday_Wish_Message") ? settings["Birthday_Wish_Message"] : "🎂 Happy Birthday {Name}! 🎉\n\nMay God bless you abundantly on your special day!";

            // Anniversary settings
            ViewBag.EnableAnniversaryWishes = settings.ContainsKey("Anniversary_Wish_Enabled") ? settings["Anniversary_Wish_Enabled"] == "true" : false;
            ViewBag.AnniversaryWishTime = settings.ContainsKey("Anniversary_Wish_Time") ? settings["Anniversary_Wish_Time"] : "14:42";
            ViewBag.AnniversaryMessage = settings.ContainsKey("Anniversary_Wish_Message") ? settings["Anniversary_Wish_Message"] : "💒 Happy Anniversary {Name}! 🎉\n\nMay God bless your marriage with many more years of love, joy, and happiness!";

            // Attendance reminder settings
            ViewBag.EnableAttendanceReminders = settings.ContainsKey("Attendance_Reminders_Enabled") ? settings["Attendance_Reminders_Enabled"] == "true" : false;
            ViewBag.ReminderTime = settings.ContainsKey("Attendance_Reminder_Time") ? settings["Attendance_Reminder_Time"] : "18:00";
            ViewBag.ReminderMessage = settings.ContainsKey("Attendance_Reminder_Message") ? settings["Attendance_Reminder_Message"] : "📅 Reminder: Church service tomorrow!";
            ViewBag.ReminderDaysBefore = settings.ContainsKey("Attendance_Reminder_Days_Before") ? settings["Attendance_Reminder_Days_Before"] : "1";

            // Verse settings
            ViewBag.EnableDailyVerse = settings.ContainsKey("Daily_Verse_Enabled") ? settings["Daily_Verse_Enabled"] == "true" : false;
            ViewBag.VerseShareTime = settings.ContainsKey("Daily_Verse_Time") ? settings["Daily_Verse_Time"] : "07:00";
            ViewBag.ShareToMembers = settings.ContainsKey("Verse_Share_To_Members") ? settings["Verse_Share_To_Members"] == "true" : false;
            ViewBag.ShareToGroups = settings.ContainsKey("Verse_Share_To_Groups") ? settings["Verse_Share_To_Groups"] == "true" : false;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWhatsAppSettings(string WhatsApp_Instance_ID, string WhatsApp_API_Key, 
            string WhatsApp_API_URL, string WhatsApp_Phone, bool WhatsApp_Enabled = false,
            string Birthday_Group_ID = "", string General_Group_ID = "", string Prayer_Group_ID = "")
        {
            await UpdateOrCreateSettingAsync("WhatsApp_Instance_ID", WhatsApp_Instance_ID);
            await UpdateOrCreateSettingAsync("WhatsApp_API_Key", WhatsApp_API_Key);
            await UpdateOrCreateSettingAsync("WhatsApp_API_URL", WhatsApp_API_URL);
            await UpdateOrCreateSettingAsync("WhatsApp_Phone", WhatsApp_Phone);
            await UpdateOrCreateSettingAsync("WhatsApp_Enabled", WhatsApp_Enabled.ToString().ToLower());
            
            // Save group IDs
            if (!string.IsNullOrWhiteSpace(Birthday_Group_ID))
                await UpdateOrCreateSettingAsync("Birthday_Group_ID", Birthday_Group_ID);
            if (!string.IsNullOrWhiteSpace(General_Group_ID))
                await UpdateOrCreateSettingAsync("General_Group_ID", General_Group_ID);
            if (!string.IsNullOrWhiteSpace(Prayer_Group_ID))
                await UpdateOrCreateSettingAsync("Prayer_Group_ID", Prayer_Group_ID);

            TempData["Success"] = "WhatsApp settings updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBirthdaySettings(bool EnableBirthdayWishes = false, 
            string BirthdayWishTime = "09:00", string BirthdayMessage = "")
        {
            await UpdateOrCreateSettingAsync("Birthday_Wish_Enabled", EnableBirthdayWishes.ToString().ToLower());
            await UpdateOrCreateSettingAsync("Birthday_Wish_Time", BirthdayWishTime);
            await UpdateOrCreateSettingAsync("Birthday_Wish_Message", BirthdayMessage);

            TempData["Success"] = "Birthday settings updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAnniversarySettings(bool EnableAnniversaryWishes = false, 
            string AnniversaryWishTime = "14:42", string AnniversaryMessage = "")
        {
            await UpdateOrCreateSettingAsync("Anniversary_Wish_Enabled", EnableAnniversaryWishes.ToString().ToLower());
            await UpdateOrCreateSettingAsync("Anniversary_Wish_Time", AnniversaryWishTime);
            await UpdateOrCreateSettingAsync("Anniversary_Wish_Message", AnniversaryMessage);

            TempData["Success"] = "Anniversary settings updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAttendanceSettings(bool EnableAttendanceReminders = false,
            string ReminderTime = "18:00", string ReminderMessage = "", string ReminderDaysBefore = "1")
        {
            await UpdateOrCreateSettingAsync("Attendance_Reminders_Enabled", EnableAttendanceReminders.ToString().ToLower());
            await UpdateOrCreateSettingAsync("Attendance_Reminder_Time", ReminderTime);
            await UpdateOrCreateSettingAsync("Attendance_Reminder_Message", ReminderMessage);
            await UpdateOrCreateSettingAsync("Attendance_Reminder_Days_Before", ReminderDaysBefore);

            TempData["Success"] = "Attendance reminder settings updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVerseSettings(bool EnableDailyVerse = false,
            string VerseShareTime = "07:00", bool ShareToMembers = false, bool ShareToGroups = false)
        {
            await UpdateOrCreateSettingAsync("Daily_Verse_Enabled", EnableDailyVerse.ToString().ToLower());
            await UpdateOrCreateSettingAsync("Daily_Verse_Time", VerseShareTime);
            await UpdateOrCreateSettingAsync("Verse_Share_To_Members", ShareToMembers.ToString().ToLower());
            await UpdateOrCreateSettingAsync("Verse_Share_To_Groups", ShareToGroups.ToString().ToLower());

            TempData["Success"] = "Verse settings updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        public async Task<IActionResult> TestWhatsAppConnection([FromBody] TestConnectionRequest request)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.ApiKey}");

                // Test API by getting instance state
                var response = await httpClient.GetAsync($"{request.ApiUrl}/waInstance{request.InstanceId}/getStateInstance/{request.ApiKey}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Connection successful!", data = content });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Connection failed: {response.StatusCode}", error = errorContent });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestAnniversaryAutomation()
        {
            try
            {
                var today = DateTime.Today;
                var debugInfo = new List<string>();
                
                // Check if today has any anniversaries
                var anniversaryMembers = await _context.Members
                    .Include(m => m.Church)
                    .Where(m => m.Status == "Active" 
                        && m.AnniversaryDate != null
                        && m.AnniversaryDate.Value.Month == today.Month
                        && m.AnniversaryDate.Value.Day == today.Day
                        && m.Phone != null)
                    .ToListAsync();
                
                debugInfo.Add($"Today: {today:yyyy-MM-dd}");
                debugInfo.Add($"Members with anniversary today: {anniversaryMembers.Count}");
                
                foreach (var member in anniversaryMembers)
                {
                    debugInfo.Add($"- {member.Name} (Church: {member.Church?.ChurchName}, Phone: {member.Phone}, Anniversary: {member.AnniversaryDate:yyyy-MM-dd})");
                }
                
                // Check anniversary automation settings
                var settings = await _context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
                var anniversaryEnabled = settings.ContainsKey("Anniversary_Wish_Enabled") && settings["Anniversary_Wish_Enabled"] == "true";
                debugInfo.Add($"Anniversary automation enabled: {anniversaryEnabled}");
                
                // Test direct WhatsApp API call for anniversary
                if (anniversaryMembers.Count > 0)
                {
                    var member = anniversaryMembers.First();
                    var church = member.Church;
                    
                    if (church != null && church.WhatsAppEnabled && !string.IsNullOrEmpty(church.WhatsAppInstanceId))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var testMessage = $"💒 Test Anniversary Message for {member.Name}! 🎉";
                            var recipient = !string.IsNullOrEmpty(church.BirthdayGroupId) ? church.BirthdayGroupId : $"91{member.Phone}@c.us";
                            
                            var requestData = new
                            {
                                chatId = recipient,
                                message = testMessage
                            };
                            
                            var json = System.Text.Json.JsonSerializer.Serialize(requestData);
                            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            
                            var apiUrl = church.WhatsAppApiUrl ?? "https://api.green-api.com";
                            var url = $"{apiUrl}/waInstance{church.WhatsAppInstanceId}/sendMessage/{church.WhatsAppApiToken}";
                            
                            var response = await httpClient.PostAsync(url, content);
                            var responseContent = await response.Content.ReadAsStringAsync();
                            
                            debugInfo.Add($"Anniversary API Response: {response.StatusCode}");
                            debugInfo.Add($"Response Content: {responseContent}");
                        }
                        catch (Exception apiEx)
                        {
                            debugInfo.Add($"Anniversary API Error: {apiEx.Message}");
                        }
                    }
                }
                else
                {
                    debugInfo.Add("No members with anniversary today. To test:");
                    debugInfo.Add("1. Go to Members -> Edit a member");
                    debugInfo.Add($"2. Set Anniversary Date to today ({today:yyyy-MM-dd})");
                    debugInfo.Add("3. Run this test again");
                }
                
                // Also trigger the anniversary service
                using var scope = HttpContext.RequestServices.CreateScope();
                var whatsAppService = scope.ServiceProvider.GetRequiredService<Services.IWhatsAppService>();
                await whatsAppService.SendAnniversaryWishesAsync();
                
                var message = "Anniversary wishes test completed!\n\nDebug Info:\n" + string.Join("\n", debugInfo);
                
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForceRunAutomation()
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var whatsAppService = scope.ServiceProvider.GetRequiredService<Services.IWhatsAppService>();
                
                var debugInfo = new List<string>();
                debugInfo.Add("Force running automation services...");
                
                // Force run birthday wishes
                await whatsAppService.SendBirthdayWishesAsync();
                debugInfo.Add("✅ Birthday wishes service executed");
                
                // Force run anniversary wishes  
                await whatsAppService.SendAnniversaryWishesAsync();
                debugInfo.Add("✅ Anniversary wishes service executed");
                
                var message = string.Join("\n", debugInfo);
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CheckBackgroundService()
        {
            try
            {
                var now = DateTime.Now;
                var debugInfo = new List<string>();
                
                debugInfo.Add($"Current Time: {now:yyyy-MM-dd HH:mm:ss}");
                
                // Check all automation settings
                var settings = await _context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
                
                debugInfo.Add("\nAutomation Settings:");
                debugInfo.Add($"Birthday_Wish_Enabled: {(settings.ContainsKey("Birthday_Wish_Enabled") ? settings["Birthday_Wish_Enabled"] : "NOT SET")}");
                debugInfo.Add($"Birthday_Wish_Time: {(settings.ContainsKey("Birthday_Wish_Time") ? settings["Birthday_Wish_Time"] : "NOT SET")}");
                debugInfo.Add($"Anniversary_Wish_Enabled: {(settings.ContainsKey("Anniversary_Wish_Enabled") ? settings["Anniversary_Wish_Enabled"] : "NOT SET")}");
                debugInfo.Add($"Anniversary_Wish_Time: {(settings.ContainsKey("Anniversary_Wish_Time") ? settings["Anniversary_Wish_Time"] : "NOT SET")}");
                
                // Check if times match current time
                var birthdayTime = settings.ContainsKey("Birthday_Wish_Time") ? settings["Birthday_Wish_Time"] : "";
                var anniversaryTime = settings.ContainsKey("Anniversary_Wish_Time") ? settings["Anniversary_Wish_Time"] : "";
                
                debugInfo.Add($"\nTime Matching:");
                debugInfo.Add($"Current: {now.Hour:D2}:{now.Minute:D2}");
                debugInfo.Add($"Birthday Time: {birthdayTime} - Match: {CheckTimeMatch(now, birthdayTime)}");
                debugInfo.Add($"Anniversary Time: {anniversaryTime} - Match: {CheckTimeMatch(now, anniversaryTime)}");
                
                var message = "Background Service Status Check:\n\n" + string.Join("\n", debugInfo);
                
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        private bool CheckTimeMatch(DateTime now, string timeString)
        {
            if (string.IsNullOrEmpty(timeString)) return false;
            
            var timeParts = timeString.Split(':');
            if (timeParts.Length == 2 && 
                int.TryParse(timeParts[0], out int hour) && 
                int.TryParse(timeParts[1], out int minute))
            {
                return now.Hour == hour && now.Minute == minute;
            }
            return false;
        }

        [HttpPost]
        public async Task<IActionResult> ClearBirthdayHistory()
        {
            try
            {
                var today = DateTime.Today;
                
                // Clear today's birthday history for testing
                var birthdayHistory = await _context.BirthdayWishHistory
                    .Where(b => b.WishSentDate.Date == today)
                    .ToListAsync();
                
                _context.BirthdayWishHistory.RemoveRange(birthdayHistory);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = $"Cleared {birthdayHistory.Count} birthday history records for today. You can now test birthday wishes again." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestAutomation()
        {
            try
            {
                var today = DateTime.Today;
                var debugInfo = new List<string>();
                
                // Check if today has any birthdays
                var birthdayMembers = await _context.Members
                    .Include(m => m.Church)
                    .Where(m => m.Status == "Active" 
                        && m.DateOfBirth != null
                        && m.DateOfBirth.Value.Month == today.Month
                        && m.DateOfBirth.Value.Day == today.Day
                        && m.Phone != null)
                    .ToListAsync();
                
                debugInfo.Add($"Today: {today:yyyy-MM-dd}");
                debugInfo.Add($"Members with birthday today: {birthdayMembers.Count}");
                
                foreach (var member in birthdayMembers)
                {
                    debugInfo.Add($"- {member.Name} (Church: {member.Church?.ChurchName}, Phone: {member.Phone}, WhatsApp Enabled: {member.Church?.WhatsAppEnabled})");
                    debugInfo.Add($"  Church API: {member.Church?.WhatsAppInstanceId}, Group: {member.Church?.BirthdayGroupId}");
                }
                
                // Check birthday automation settings
                var settings = await _context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
                var birthdayEnabled = settings.ContainsKey("Birthday_Wish_Enabled") && settings["Birthday_Wish_Enabled"] == "true";
                debugInfo.Add($"Birthday automation enabled: {birthdayEnabled}");
                
                // Test direct WhatsApp API call
                if (birthdayMembers.Count > 0)
                {
                    var member = birthdayMembers.First();
                    var church = member.Church;
                    
                    if (church != null && church.WhatsAppEnabled && !string.IsNullOrEmpty(church.WhatsAppInstanceId))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var testMessage = $"🎂 Test Birthday Message for {member.Name}! 🎉";
                            var recipient = !string.IsNullOrEmpty(church.BirthdayGroupId) ? church.BirthdayGroupId : $"91{member.Phone}@c.us";
                            
                            var requestData = new
                            {
                                chatId = recipient,
                                message = testMessage
                            };
                            
                            var json = System.Text.Json.JsonSerializer.Serialize(requestData);
                            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            
                            var apiUrl = church.WhatsAppApiUrl ?? "https://api.green-api.com";
                            var url = $"{apiUrl}/waInstance{church.WhatsAppInstanceId}/sendMessage/{church.WhatsAppApiToken}";
                            
                            debugInfo.Add($"Sending to: {recipient}");
                            debugInfo.Add($"API URL: {url}");
                            
                            var response = await httpClient.PostAsync(url, content);
                            var responseContent = await response.Content.ReadAsStringAsync();
                            
                            debugInfo.Add($"API Response: {response.StatusCode}");
                            debugInfo.Add($"Response Content: {responseContent}");
                        }
                        catch (Exception apiEx)
                        {
                            debugInfo.Add($"API Error: {apiEx.Message}");
                        }
                    }
                }
                
                // Also trigger the service
                using var scope = HttpContext.RequestServices.CreateScope();
                var whatsAppService = scope.ServiceProvider.GetRequiredService<Services.IWhatsAppService>();
                await whatsAppService.SendBirthdayWishesAsync();
                
                var message = "Birthday wishes test completed!\n\nDebug Info:\n" + string.Join("\n", debugInfo);
                
                TempData["Success"] = "Birthday wishes test triggered successfully!";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetWhatsAppGroups([FromBody] TestConnectionRequest request)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                // Green API doesn't use Bearer token, just append to URL
                var url = $"{request.ApiUrl}/waInstance{request.InstanceId}/getChats/{request.ApiKey}";
                
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Groups retrieved successfully!", data = content });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Failed to get groups: {response.StatusCode}", error = errorContent });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> ShowGroups()
        {
            try
            {
                var settings = await _context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
                
                var instanceId = settings.ContainsKey("WhatsApp_Instance_ID") ? settings["WhatsApp_Instance_ID"] : "";
                var apiKey = settings.ContainsKey("WhatsApp_API_Key") ? settings["WhatsApp_API_Key"] : "";
                var apiUrl = settings.ContainsKey("WhatsApp_API_URL") ? settings["WhatsApp_API_URL"] : "https://api.green-api.com";
                
                using var httpClient = new HttpClient();
                var url = $"{apiUrl}/waInstance{instanceId}/getChats/{apiKey}";
                
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                ViewBag.Response = content;
                ViewBag.StatusCode = response.StatusCode;
                ViewBag.Url = url;
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        private async Task UpdateOrCreateSettingAsync(string key, string value)
        {
            var setting = await _context.AutomationSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            
            if (setting == null)
            {
                // Create new setting
                setting = new AutomationSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    IsActive = true,
                    ModifiedDate = DateTime.Now
                };
                _context.AutomationSettings.Add(setting);
            }
            else
            {
                // Update existing setting
                setting.SettingValue = value;
                setting.ModifiedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSetting(int id, string value)
        {
            var setting = await _context.AutomationSettings.FindAsync(id);
            if (setting != null)
            {
                setting.SettingValue = value;
                setting.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Setting updated successfully!";
            }
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSetting(int id)
        {
            var setting = await _context.AutomationSettings.FindAsync(id);
            if (setting != null)
            {
                setting.IsActive = !setting.IsActive;
                setting.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Setting {(setting.IsActive ? "enabled" : "disabled")} successfully!";
            }
            return RedirectToAction(nameof(Settings));
        }


    }
}
