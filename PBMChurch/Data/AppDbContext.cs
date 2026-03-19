using Microsoft.EntityFrameworkCore;
using PBMChurch.Models;

namespace PBMChurch.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Church> Churches { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<Income> Income { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Verse> Verses { get; set; }
        public DbSet<YouTubeLink> YouTubeLinks { get; set; }
        public DbSet<AutomationSetting> AutomationSettings { get; set; }
        public DbSet<BirthdayWishHistory> BirthdayWishHistory { get; set; }
        public DbSet<AnniversaryWishHistory> AnniversaryWishHistory { get; set; }
        public DbSet<MemberNote> MemberNotes { get; set; }
        public DbSet<PrayerRequest> PrayerRequests { get; set; }
        public DbSet<MemberOffering> MemberOfferings { get; set; }
        public DbSet<LandingPageContent> LandingPageContents { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }
    public DbSet<SentVerseMessage> SentVerseMessages { get; set; }
    public DbSet<WishTemplate> WishTemplates { get; set; }
    public DbSet<BibleReading> BibleReadings { get; set; }
    public DbSet<DailyBibleReading> DailyBibleReadings { get; set; }
    public DbSet<ChurchActivity> ChurchActivities { get; set; }
    public DbSet<CalendarTask> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints
            modelBuilder.Entity<AdminUser>()
                .HasIndex(a => a.Username)
                .IsUnique();

            modelBuilder.Entity<Member>()
                .HasIndex(m => m.UserId)
                .IsUnique();

            modelBuilder.Entity<AutomationSetting>()
                .HasIndex(a => a.SettingKey)
                .IsUnique();

            // Configure unique constraint for attendance
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.MemberId, a.AttendanceDate })
                .IsUnique();

            // Configure unique constraint for birthday wish history
            modelBuilder.Entity<BirthdayWishHistory>()
                .HasIndex(b => new { b.MemberId, b.WishSentDate })
                .IsUnique();

            // Configure unique constraint for anniversary wish history
            modelBuilder.Entity<AnniversaryWishHistory>()
                .HasIndex(a => new { a.MemberId, a.WishDate })
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<Income>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            // FamilyMember relationship
            modelBuilder.Entity<FamilyMember>()
                .HasOne(fm => fm.Member)
                .WithMany(m => m.FamilyMembers)
                .HasForeignKey(fm => fm.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            // Configure PrayerRequest relationships
            modelBuilder.Entity<PrayerRequest>()
                .HasKey(p => p.RequestId);

            modelBuilder.Entity<PrayerRequest>()
                .HasOne(p => p.Church)
                .WithMany()
                .HasForeignKey(p => p.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrayerRequest>()
                .HasOne(p => p.Member)
                .WithMany()
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Church)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Church)
                .WithMany(c => c.Attendances)
                .HasForeignKey(a => a.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Attendances)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Income>()
                .HasOne(i => i.Church)
                .WithMany(c => c.Incomes)
                .HasForeignKey(i => i.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Church)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YouTubeLink>()
                .HasOne(y => y.Church)
                .WithMany()
                .HasForeignKey(y => y.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BirthdayWishHistory>()
                .HasOne(b => b.Member)
                .WithMany()
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure check constraints (these are already in SQL, but good for documentation)
            modelBuilder.Entity<Church>()
                .Property(c => c.Status)
                .HasDefaultValue("Active");

            modelBuilder.Entity<Member>()
                .Property(m => m.Status)
                .HasDefaultValue("Active");

            // Configure Task relationships
            modelBuilder.Entity<CalendarTask>()
                .HasKey(t => t.TaskId);

            modelBuilder.Entity<CalendarTask>()
                .HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalendarTask>()
                .HasOne(t => t.RelatedMember)
                .WithMany()
                .HasForeignKey(t => t.RelatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<CalendarTask>()
                .HasOne(t => t.RelatedChurch)
                .WithMany()
                .HasForeignKey(t => t.RelatedId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
