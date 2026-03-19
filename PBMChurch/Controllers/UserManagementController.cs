using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;
using System.Threading.Tasks;

namespace PBMChurch.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;

        public UserManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.AdminUsers.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUser user)
        {
            //if (!ModelState.IsValid)
            //    return View(user);

            // Set Username to Email
            user.Username = user.Email;

            // Generate password: first 4 uppercase letters of email + @ + last 4 digits of phone
            if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.Phone) && user.Email.Length >= 4 && user.Phone.Length >= 4)
            {
                var emailPart = user.Email.Substring(0, 4).ToUpper();
                var phonePart = user.Phone.Substring(user.Phone.Length - 4);
                user.Password = emailPart + "@" + phonePart;
            }
            else
            {
                ModelState.AddModelError("", "Email and Phone must be at least 4 characters long.");
                return View(user);
            }

            user.IsActive = true;
            user.LastLoginDate = null;
            // FullName is now handled from the form
            await _context.AdminUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User created successfully!";
            return RedirectToAction("Index");
        }
    }
}
