using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;

namespace PBMChurch.Services
{
    public class UserIdMigrationService
    {
        private readonly AppDbContext _context;

        public UserIdMigrationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task MigrateUserIdsAsync()
        {
            var churches = await _context.Churches.ToListAsync();
            var churchGroups = churches.GroupBy(c => c.ChurchName.Substring(0, 1).ToUpper());

            foreach (var group in churchGroups.Where(g => g.Count() > 1))
            {
                foreach (var church in group)
                {
                    if (string.IsNullOrEmpty(church.ChurchName) || church.ChurchName.Length < 2)
                        continue;

                    var newPrefix = church.ChurchName.Substring(0, 2).ToUpper();
                    var oldPrefix = church.ChurchName.Substring(0, 1).ToUpper();

                    var membersToUpdate = await _context.Members
                        .Where(m => m.ChurchId == church.ChurchId && m.UserId.StartsWith(oldPrefix) && m.UserId.Length == 3)
                        .ToListAsync();

                    foreach (var member in membersToUpdate)
                    {
                        var numberPart = member.UserId.Substring(1);
                        var newUserId = $"{newPrefix}{numberPart}";

                        if (!await _context.Members.AnyAsync(m => m.UserId == newUserId))
                        {
                            member.UserId = newUserId;
                        }
                    }

                    if (membersToUpdate.Any())
                    {
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
