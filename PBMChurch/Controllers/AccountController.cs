using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;
using PBMChurch.Models.ViewModels;
using PBMChurch.Services;

namespace PBMChurch.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IJwtTokenService _jwtService;
        private readonly IOtpService _otpService;

        public AccountController(
            AppDbContext context, 
            ILogger<AccountController> logger,
            IJwtTokenService jwtService,
            IOtpService otpService)
        {
            _context = context;
            _logger = logger;
            _jwtService = jwtService;
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Check if already logged in via JWT token in cookie
            if (Request.Cookies.ContainsKey("jwt_token"))
            {
                var token = Request.Cookies["jwt_token"];
                var principal = _jwtService.ValidateToken(token!);
                
                // If token is valid, redirect to dashboard
                if (principal != null)
                    return RedirectToAction("Index", "Dashboard");
                
                // Token invalid/expired, clear cookies
                Response.Cookies.Delete("jwt_token");
                Response.Cookies.Delete("refresh_token");
                HttpContext.Session.Clear();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(a => a.Username == model.Username && a.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Plain text password comparison
            if (user.Password != model.Password)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Update last login
            user.LastLoginDate = DateTime.Now;

            // Generate JWT token
            var token = _jwtService.GenerateToken(user, model.RememberMe);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            // Set JWT token in cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            if (model.RememberMe)
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
            }
            // If RememberMe is false, don't set Expires - makes it a session cookie

            Response.Cookies.Append("jwt_token", token, cookieOptions);
            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);

            // Also store in session for easy access
            HttpContext.Session.SetString("UserName", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            _logger.LogInformation($"User {model.Username} logged in successfully");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect based on user role
            if (user.Role == "Member")
                return RedirectToAction("Index", "Attendance");
            else
                return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string usernameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                ModelState.AddModelError("", "Please enter your username or email");
                return View();
            }

            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(a => 
                    (a.Username == usernameOrEmail || a.Email == usernameOrEmail) && a.IsActive);

            if (user != null)
            {
                // Generate OTP
                var otp = _otpService.GenerateOtp();
                user.ResetOtp = otp;
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes

                await _context.SaveChangesAsync();

                // Send OTP via email or SMS
                var isEmail = usernameOrEmail.Contains("@");
                var sent = await _otpService.SendOtpAsync(
                    isEmail ? user.Email! : user.Phone!, 
                    otp, 
                    isEmail);

                if (sent)
                {
                    TempData["UserId"] = user.UserId;
                    TempData["Message"] = $"OTP sent to your {(isEmail ? "email" : "phone")}. Check console logs for demo.";
                    return RedirectToAction("ResetPassword");
                }
            }

            // Don't reveal if user exists or not
            TempData["Message"] = "If the account exists, you will receive an OTP shortly.";
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string otp, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("", "All fields are required");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View();
            }

            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(a => a.ResetOtp == otp && a.IsActive);

            if (user == null || user.OtpExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired OTP");
                return View();
            }

            // Update password
            user.Password = newPassword;
            user.ResetOtp = null;
            user.OtpExpiry = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password reset successfully. Please login with your new password.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            // Clear cookies
            Response.Cookies.Delete("jwt_token");
            Response.Cookies.Delete("refresh_token");

            // Clear session
            HttpContext.Session.Clear();

            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

