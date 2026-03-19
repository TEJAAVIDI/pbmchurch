using System.Security.Cryptography;

namespace PBMChurch.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<bool> SendOtpAsync(string emailOrPhone, string otp, bool isEmail);
    }

    public class OtpService : IOtpService
    {
        private readonly ILogger<OtpService> _logger;

        public OtpService(ILogger<OtpService> logger)
        {
            _logger = logger;
        }

        public string GenerateOtp()
        {
            // Generate 6-digit OTP
            var random = RandomNumberGenerator.GetInt32(100000, 999999);
            return random.ToString();
        }

        public async Task<bool> SendOtpAsync(string emailOrPhone, string otp, bool isEmail)
        {
            try
            {
                if (isEmail)
                {
                    // TODO: Implement email sending using SMTP or email service
                    // For now, just log the OTP
                    _logger.LogInformation($"OTP sent to email {emailOrPhone}: {otp}");
                    
                    // Simulated email sending
                    await Task.Delay(100);
                    return true;
                }
                else
                {
                    // TODO: Implement SMS sending using Twilio or similar service
                    // For now, just log the OTP
                    _logger.LogInformation($"OTP sent to phone {emailOrPhone}: {otp}");
                    
                    // Simulated SMS sending
                    await Task.Delay(100);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP to {emailOrPhone}");
                return false;
            }
        }
    }
}
