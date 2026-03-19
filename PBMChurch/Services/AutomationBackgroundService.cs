using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;

namespace PBMChurch.Services
{
    public class AutomationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutomationBackgroundService> _logger;

        public AutomationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AutomationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Automation Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var whatsAppService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();
                    
                    var now = DateTime.Now;

                    // Get settings from database
                    var settings = await context.AutomationSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

                    // Check if it's time to send birthday wishes
                    var birthdayEnabled = settings.ContainsKey("Birthday_Wish_Enabled") && settings["Birthday_Wish_Enabled"] == "true";
                    if (birthdayEnabled)
                    {
                        var birthdayTime = settings.ContainsKey("Birthday_Wish_Time") ? settings["Birthday_Wish_Time"] : "09:00";
                        var timeParts = birthdayTime.Split(':');
                        if (timeParts.Length == 2 && 
                            int.TryParse(timeParts[0], out int hour) && 
                            int.TryParse(timeParts[1], out int minute))
                        {
                            if (now.Hour == hour && now.Minute == minute)
                            {
                                _logger.LogInformation($"Sending birthday wishes at {now}");
                                await whatsAppService.SendBirthdayWishesAsync();
                            }
                        }
                    }

                    // Check if it's time to send daily verse
                    var verseEnabled = settings.ContainsKey("Daily_Verse_Enabled") && settings["Daily_Verse_Enabled"] == "true";
                    if (verseEnabled)
                    {
                        var verseTime = settings.ContainsKey("Daily_Verse_Time") ? settings["Daily_Verse_Time"] : "07:00";
                        var timeParts = verseTime.Split(':');
                        if (timeParts.Length == 2 && 
                            int.TryParse(timeParts[0], out int hour) && 
                            int.TryParse(timeParts[1], out int minute))
                        {
                            if (now.Hour == hour && now.Minute == minute)
                            {
                                _logger.LogInformation($"Sending daily verse at {now}");
                                await whatsAppService.SendDailyVerseAsync();
                            }
                        }
                    }

                    // Check if it's time to send anniversary wishes
                    var anniversaryEnabled = settings.ContainsKey("Anniversary_Wish_Enabled") && settings["Anniversary_Wish_Enabled"] == "true";
                    if (anniversaryEnabled)
                    {
                        var anniversaryTime = settings.ContainsKey("Anniversary_Wish_Time") ? settings["Anniversary_Wish_Time"] : "14:42";
                        _logger.LogInformation($"Anniversary check: Enabled={anniversaryEnabled}, Time={anniversaryTime}, Now={now:HH:mm}");
                        var timeParts = anniversaryTime.Split(':');
                        if (timeParts.Length == 2 && 
                            int.TryParse(timeParts[0], out int hour) && 
                            int.TryParse(timeParts[1], out int minute))
                        {
                            _logger.LogInformation($"Anniversary time parsed: {hour:D2}:{minute:D2}, Current: {now.Hour:D2}:{now.Minute:D2}");
                            if (now.Hour == hour && now.Minute == minute)
                            {
                                _logger.LogInformation($"Sending anniversary wishes at {now}");
                                await whatsAppService.SendAnniversaryWishesAsync();
                            }
                        }
                    }

                    // Check if it's time to send attendance reminders
                    var reminderEnabled = settings.ContainsKey("Attendance_Reminders_Enabled") && settings["Attendance_Reminders_Enabled"] == "true";
                    if (reminderEnabled)
                    {
                        var reminderTime = settings.ContainsKey("Attendance_Reminder_Time") ? settings["Attendance_Reminder_Time"] : "18:00";
                        var timeParts = reminderTime.Split(':');
                        if (timeParts.Length == 2 && 
                            int.TryParse(timeParts[0], out int hour) && 
                            int.TryParse(timeParts[1], out int minute))
                        {
                            if (now.Hour == hour && now.Minute == minute)
                            {
                                _logger.LogInformation($"Sending attendance reminders at {now}");
                                await whatsAppService.SendAttendanceRemindersAsync();
                            }
                        }
                    }

                    // Wait for 1 minute before checking again
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Automation Background Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Automation Background Service stopped");
        }
    }
}
