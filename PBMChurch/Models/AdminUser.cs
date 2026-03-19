using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models
{
    public class AdminUser
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Password { get; set; } = string.Empty; // Plain text password

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Admin";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiry { get; set; }

        public string? ResetOtp { get; set; }
        
        public DateTime? OtpExpiry { get; set; }

        [StringLength(20)]
        public string? AttendanceMode { get; set; } = "individual";
    }
}

