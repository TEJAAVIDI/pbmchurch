using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;

namespace PBMChurch.Services
{
    public interface IWhatsAppService
    {
        Task SendBirthdayWishesAsync();
        Task SendAnniversaryWishesAsync();
        Task SendDailyVerseAsync();
        Task SendAttendanceRemindersAsync();
        Task SendYouTubeLinkAsync(int linkId);
        Task<string> GetSettingAsync(string key);
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(AppDbContext context, ILogger<WhatsAppService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendBirthdayWishesAsync()
        {
            try
            {
                _logger.LogInformation("Starting birthday wishes check...");

                var today = DateTime.Today;
                
                // Get all active churches with WhatsApp enabled
                var churches = await _context.Churches
                    .Where(c => c.Status == "Active" && c.WhatsAppEnabled)
                    .ToListAsync();

                _logger.LogInformation($"Found {churches.Count} churches with WhatsApp enabled");

                foreach (var church in churches)
                {
                    // Skip if WhatsApp not properly configured for this church
                    if (string.IsNullOrEmpty(church.WhatsAppInstanceId) || 
                        string.IsNullOrEmpty(church.WhatsAppApiToken))
                    {
                        _logger.LogWarning($"WhatsApp not configured properly for {church.ChurchName}, skipping...");
                        continue;
                    }

                    // Get members with birthdays today in this church
                    var members = await _context.Members
                        .Where(m => m.ChurchId == church.ChurchId
                            && m.Status == "Active" 
                            && m.DateOfBirth != null
                            && m.DateOfBirth.Value.Month == today.Month
                            && m.DateOfBirth.Value.Day == today.Day
                            && m.Phone != null)
                        .ToListAsync();

                    _logger.LogInformation($"Found {members.Count} members with birthday today in {church.ChurchName}");

                    if (members.Count == 0) continue;

                    // Get birthday message template
                    var messageTemplate = await GetSettingAsync("Birthday_Wish_Message");
                    if (string.IsNullOrEmpty(messageTemplate))
                    {
                        messageTemplate = "🎂 Happy Birthday {Name}! 🎉\n\nMay God bless you abundantly on your special day!";
                    }

                    foreach (var member in members)
                    {
                        // Check if wish already sent today
                        var alreadySent = await _context.BirthdayWishHistory
                            .AnyAsync(b => b.MemberId == member.MemberId && b.WishSentDate.Date == today);
                        
                        if (alreadySent) continue;

                        var message = messageTemplate.Replace("{Name}", member.Name);
                        
                        // Send to church's birthday group if configured, otherwise to individual
                        if (!string.IsNullOrEmpty(church.BirthdayGroupId))
                        {
                            var success = await SendWhatsAppMessageAsync(
                                church.BirthdayGroupId, 
                                message, 
                                church.WhatsAppInstanceId!, 
                                church.WhatsAppApiToken!, 
                                church.WhatsAppApiUrl ?? "https://api.green-api.com"
                            );
                            
                            if (success)
                            {
                                // Record the wish
                                _context.BirthdayWishHistory.Add(new BirthdayWishHistory
                                {
                                    MemberId = member.MemberId,
                                    WishSentDate = today,
                                    MessageSent = message
                                });
                                await _context.SaveChangesAsync();
                            }
                            
                            _logger.LogInformation($"Birthday wish for {member.Name} sent to {church.ChurchName} group: {success}");
                        }
                        else
                        {
                            var success = await SendWhatsAppMessageAsync(
                                member.Phone!, 
                                message, 
                                church.WhatsAppInstanceId!, 
                                church.WhatsAppApiToken!, 
                                church.WhatsAppApiUrl ?? "https://api.green-api.com"
                            );
                            
                            if (success)
                            {
                                // Record the wish
                                _context.BirthdayWishHistory.Add(new BirthdayWishHistory
                                {
                                    MemberId = member.MemberId,
                                    WishSentDate = today,
                                    MessageSent = message
                                });
                                await _context.SaveChangesAsync();
                            }
                            
                            _logger.LogInformation($"Birthday wish sent to {member.Name} ({member.Phone}) in {church.ChurchName}: {success}");
                        }
                    }
                }

                _logger.LogInformation("Birthday wishes processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending birthday wishes");
                // Don't throw - just log the error so the service continues running
            }
        }

        public async Task SendAnniversaryWishesAsync()
        {
            try
            {
                _logger.LogInformation("Starting anniversary wishes check...");

                var today = DateTime.Today;
                
                // Get all active churches with WhatsApp enabled
                var churches = await _context.Churches
                    .Where(c => c.Status == "Active" && c.WhatsAppEnabled)
                    .ToListAsync();

                _logger.LogInformation($"Found {churches.Count} churches with WhatsApp enabled");

                foreach (var church in churches)
                {
                    // Skip if WhatsApp not properly configured for this church
                    if (string.IsNullOrEmpty(church.WhatsAppInstanceId) || 
                        string.IsNullOrEmpty(church.WhatsAppApiToken))
                    {
                        _logger.LogWarning($"WhatsApp not configured properly for {church.ChurchName}, skipping...");
                        continue;
                    }

                    // Get members with anniversaries today in this church
                    var members = await _context.Members
                        .Where(m => m.ChurchId == church.ChurchId
                            && m.Status == "Active" 
                            && m.AnniversaryDate != null
                            && m.AnniversaryDate.Value.Month == today.Month
                            && m.AnniversaryDate.Value.Day == today.Day
                            && m.Phone != null)
                        .ToListAsync();

                    _logger.LogInformation($"Found {members.Count} members with anniversary today in {church.ChurchName}");

                    if (members.Count == 0) continue;

                    // Get anniversary message template
                    var messageTemplate = await GetSettingAsync("Anniversary_Wish_Message");
                    if (string.IsNullOrEmpty(messageTemplate))
                    {
                        messageTemplate = "💒 Happy Anniversary {Name}! 🎉\n\nMay God bless your marriage with many more years of love, joy, and happiness. Wishing you both a wonderful anniversary celebration!";
                    }

                    foreach (var member in members)
                    {
                        // Check if wish already sent today
                        var alreadySent = await _context.AnniversaryWishHistory
                            .AnyAsync(a => a.MemberId == member.MemberId && a.WishDate == today);
                        
                        if (alreadySent) continue;

                        var message = messageTemplate.Replace("{Name}", member.Name);
                        
                        // Send to church's birthday group if configured, otherwise to individual
                        if (!string.IsNullOrEmpty(church.BirthdayGroupId))
                        {
                            var success = await SendWhatsAppMessageAsync(
                                church.BirthdayGroupId, 
                                message, 
                                church.WhatsAppInstanceId!, 
                                church.WhatsAppApiToken!, 
                                church.WhatsAppApiUrl ?? "https://api.green-api.com"
                            );
                            
                            if (success)
                            {
                                // Record the wish
                                _context.AnniversaryWishHistory.Add(new AnniversaryWishHistory
                                {
                                    MemberId = member.MemberId,
                                    WishDate = today
                                });
                                await _context.SaveChangesAsync();
                            }
                            
                            _logger.LogInformation($"Anniversary wish for {member.Name} sent to {church.ChurchName} group: {success}");
                        }
                        else
                        {
                            var success = await SendWhatsAppMessageAsync(
                                member.Phone!, 
                                message, 
                                church.WhatsAppInstanceId!, 
                                church.WhatsAppApiToken!, 
                                church.WhatsAppApiUrl ?? "https://api.green-api.com"
                            );
                            
                            if (success)
                            {
                                // Record the wish
                                _context.AnniversaryWishHistory.Add(new AnniversaryWishHistory
                                {
                                    MemberId = member.MemberId,
                                    WishDate = today
                                });
                                await _context.SaveChangesAsync();
                            }
                            
                            _logger.LogInformation($"Anniversary wish sent to {member.Name} ({member.Phone}) in {church.ChurchName}: {success}");
                        }
                    }
                }

                _logger.LogInformation("Anniversary wishes processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending anniversary wishes");
            }
        }

        public async Task SendDailyVerseAsync()
        {
            try
            {
                _logger.LogInformation("Starting daily verse sharing...");

                // Get all active churches with WhatsApp enabled
                var churches = await _context.Churches
                    .Where(c => c.Status == "Active" && c.WhatsAppEnabled)
                    .ToListAsync();

                _logger.LogInformation($"Found {churches.Count} churches with WhatsApp enabled");

                // Get a verse that hasn't been posted or posted least recently
                var verse = await _context.Verses
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.PostCount)
                    .ThenBy(v => v.LastPostedDate)
                    .FirstOrDefaultAsync();

                if (verse == null)
                {
                    _logger.LogWarning("No active verses found");
                    return;
                }

                foreach (var church in churches)
                {
                    // Skip if WhatsApp not properly configured for this church
                    if (string.IsNullOrEmpty(church.WhatsAppInstanceId) || 
                        string.IsNullOrEmpty(church.WhatsAppApiToken))
                    {
                        _logger.LogWarning($"WhatsApp not configured properly for {church.ChurchName}, skipping...");
                        continue;
                    }

                    // Send to church's general group if configured
                    if (!string.IsNullOrEmpty(church.GeneralGroupId))
                    {
                        var success = await SendWhatsAppImageAsync(
                            church.GeneralGroupId, 
                            verse.StorageURL, 
                            "📖 Daily Bible Verse",
                            church.WhatsAppInstanceId!, 
                            church.WhatsAppApiToken!, 
                            church.WhatsAppApiUrl ?? "https://api.green-api.com"
                        );
                        
                        _logger.LogInformation($"Daily verse sent to {church.ChurchName} group: {success}");
                    }
                    else
                    {
                        _logger.LogWarning($"General Group ID not configured for {church.ChurchName}");
                    }
                }

                // Update verse post count and date
                verse.LastPostedDate = DateTime.Now;
                verse.PostCount++;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Daily verse sharing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending daily verse");
            }
        }

        public async Task SendAttendanceRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Starting attendance reminders...");

                // Get all active churches with WhatsApp enabled
                var churches = await _context.Churches
                    .Where(c => c.Status == "Active" && c.WhatsAppEnabled)
                    .ToListAsync();

                _logger.LogInformation($"Found {churches.Count} churches with WhatsApp enabled");

                // Get tomorrow's day name
                var tomorrow = DateTime.Today.AddDays(1);
                var tomorrowDayName = tomorrow.ToString("dddd"); // e.g., "Monday", "Tuesday", etc.

                _logger.LogInformation($"Tomorrow is {tomorrowDayName} ({tomorrow:yyyy-MM-dd})");

                // Get reminder message template
                var messageTemplate = await GetSettingAsync("Attendance_Reminder_Message");
                if (string.IsNullOrEmpty(messageTemplate))
                {
                    messageTemplate = "📅 Reminder: Church service tomorrow! 🙏\n\nLooking forward to seeing you at the service.\n\nGod bless you!";
                }

                int remindersSent = 0;

                foreach (var church in churches)
                {
                    // Check if this church has a meeting tomorrow
                    bool hasMeetingTomorrow = tomorrowDayName.Equals(church.MeetingDay1, StringComparison.OrdinalIgnoreCase) ||
                                             tomorrowDayName.Equals(church.MeetingDay2, StringComparison.OrdinalIgnoreCase);

                    if (!hasMeetingTomorrow)
                    {
                        _logger.LogInformation($"{church.ChurchName} has no meeting tomorrow ({tomorrowDayName}), skipping...");
                        continue;
                    }

                    // Skip if WhatsApp not properly configured for this church
                    if (string.IsNullOrEmpty(church.WhatsAppInstanceId) || 
                        string.IsNullOrEmpty(church.WhatsAppApiToken))
                    {
                        _logger.LogWarning($"WhatsApp not configured properly for {church.ChurchName}, skipping...");
                        continue;
                    }

                    // Send to church's general group if configured
                    if (!string.IsNullOrEmpty(church.GeneralGroupId))
                    {
                        var success = await SendWhatsAppMessageAsync(
                            church.GeneralGroupId, 
                            messageTemplate,
                            church.WhatsAppInstanceId!, 
                            church.WhatsAppApiToken!, 
                            church.WhatsAppApiUrl ?? "https://api.green-api.com"
                        );
                        
                        if (success)
                        {
                            remindersSent++;
                            _logger.LogInformation($"✅ Attendance reminder sent to {church.ChurchName} (Meeting tomorrow: {tomorrowDayName})");
                        }
                        else
                        {
                            _logger.LogError($"❌ Failed to send reminder to {church.ChurchName}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"General Group ID not configured for {church.ChurchName}");
                    }
                }

                _logger.LogInformation($"Attendance reminders completed. Sent {remindersSent} reminder(s) for tomorrow's meetings.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending attendance reminders");
            }
        }

        public async Task SendYouTubeLinkAsync(int linkId)
        {
            try
            {
                var isEnabled = await GetSettingAsync("WhatsApp_Enabled");
                var autoPost = await GetSettingAsync("YouTube_Auto_Post");
                
                if (isEnabled != "true" || autoPost != "true") return;

                var link = await _context.YouTubeLinks.FindAsync(linkId);
                if (link != null && !link.IsSentToWhatsApp)
                {
                    var groupId = await GetSettingAsync("WhatsApp_Group_ID");
                    if (!string.IsNullOrEmpty(groupId))
                    {
                        var message = $"🎬 New Video: {link.Title}\n\n{link.Description}\n\n🔗 Watch here: {link.YouTubeURL}";
                        var success = await SendWhatsAppMessageAsync(groupId, message);

                        if (success)
                        {
                            link.IsSentToWhatsApp = true;
                            link.SentDate = DateTime.Now;
                            await _context.SaveChangesAsync();

                            _logger.LogInformation($"YouTube link posted successfully: {link.Title}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending YouTube link");
            }
        }

        public async Task<string> GetSettingAsync(string key)
        {
            var setting = await _context.AutomationSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key && s.IsActive);
            return setting?.SettingValue ?? "";
        }

        private async Task<bool> SendWhatsAppMessageAsync(string recipient, string message, string instanceId, string apiKey, string apiUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(instanceId))
                {
                    _logger.LogWarning("WhatsApp API not configured properly");
                    return false;
                }

                // Clean phone number if it's not a group ID
                string chatId;
                if (recipient.EndsWith("@g.us"))
                {
                    // It's a group ID, use as is
                    chatId = recipient;
                }
                else
                {
                    // It's a phone number, clean and format it
                    var phone = new string(recipient.Where(char.IsDigit).ToArray());
                    
                    // Ensure phone number has country code
                    if (!phone.StartsWith("91") && phone.Length == 10)
                    {
                        phone = "91" + phone; // Add India country code
                    }
                    chatId = $"{phone}@c.us";
                }

                using var httpClient = new HttpClient();

                // Prepare request for Green API
                var requestData = new
                {
                    chatId = chatId,
                    message = message
                };

                var json = System.Text.Json.JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send message using Green API
                var response = await httpClient.PostAsync($"{apiUrl}/waInstance{instanceId}/sendMessage/{apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"WhatsApp message sent successfully to {chatId}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send WhatsApp message to {chatId}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending WhatsApp message to {recipient}");
                return false;
            }
        }

        // Overload for backward compatibility with global settings
        private async Task<bool> SendWhatsAppMessageAsync(string phone, string message)
        {
            // Get global API configuration from settings
            var apiKey = await GetSettingAsync("WhatsApp_API_Key");
            var apiUrl = await GetSettingAsync("WhatsApp_API_URL");
            var instanceId = await GetSettingAsync("WhatsApp_Instance_ID");

            return await SendWhatsAppMessageAsync(phone, message, instanceId, apiKey, apiUrl);
        }

        private async Task<bool> SendWhatsAppImageAsync(string recipient, string imageUrl, string caption, string instanceId, string apiKey, string apiUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(instanceId))
                {
                    _logger.LogWarning("WhatsApp API not configured properly");
                    return false;
                }

                using var httpClient = new HttpClient();

                // Prepare request for sending image
                var requestData = new
                {
                    chatId = recipient,
                    urlFile = imageUrl,
                    caption = caption,
                    fileName = "verse.jpg"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send image using Green API
                var response = await httpClient.PostAsync($"{apiUrl}/waInstance{instanceId}/sendFileByUrl/{apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"WhatsApp image sent successfully to {recipient}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send WhatsApp image. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending WhatsApp image to {recipient}");
                return false;
            }
        }

        // Overload for backward compatibility with global settings
        private async Task<bool> SendWhatsAppImageAsync(string recipient, string imageUrl, string caption)
        {
            // Get global API configuration from settings
            var apiKey = await GetSettingAsync("WhatsApp_API_Key");
            var apiUrl = await GetSettingAsync("WhatsApp_API_URL");
            var instanceId = await GetSettingAsync("WhatsApp_Instance_ID");

            return await SendWhatsAppImageAsync(recipient, imageUrl, caption, instanceId, apiKey, apiUrl);
        }
    }
}
