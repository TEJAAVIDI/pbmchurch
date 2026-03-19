using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;

namespace PBMChurch.Services
{
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationCleanupService> _logger;

        public NotificationCleanupService(IServiceProvider serviceProvider, ILogger<NotificationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldNotifications();
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification cleanup service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task CleanupOldNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var yesterday = DateTime.Today.AddDays(-1);
            var rowsDeleted = await context.Database.ExecuteSqlRawAsync(
                "DELETE FROM NotificationReads WHERE ReadDate < {0}", yesterday);
            
            if (rowsDeleted > 0)
            {
                _logger.LogInformation($"Cleaned up {rowsDeleted} old notification records");
            }
        }
    }
}
