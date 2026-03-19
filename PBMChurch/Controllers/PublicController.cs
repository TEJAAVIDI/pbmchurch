using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;

namespace PBMChurch.Controllers
{
    public class PublicController : Controller
    {
        private readonly AppDbContext _context;

        public PublicController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPrayerRequest(string name, string? phone, int? churchId, string title, string request, string? notes, string? location)
        {
            try
            {
                var prayerRequest = new PrayerRequest
                {
                    Name = name,
                    Phone = phone,
                    ChurchId = churchId,
                    Title = title,
                    Request = request,
                    Notes = !string.IsNullOrEmpty(location) ? $"{notes}\n\nLocation: {location}" : notes,
                    Status = "Pending",
                    RequestDate = DateTime.Now,
                    CreatedBy = 0, // Public submission
                    CreatedDate = DateTime.Now
                };

                _context.PrayerRequests.Add(prayerRequest);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Prayer request submitted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting prayer request: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChurches()
        {
            var churches = await _context.Churches
                .Where(c => c.Status == "Active")
                .Select(c => new { value = c.ChurchId, text = c.ChurchName })
                .ToListAsync();

            return Json(churches);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateUserId(int churchId)
        {
            try
            {
                var church = await _context.Churches.FindAsync(churchId);
                if (church == null)
                    return Json(new { success = false, message = "Church not found" });

                var firstLetter = church.ChurchName.Substring(0, 1).ToUpper();
                var hasConflict = await _context.Churches
                    .AnyAsync(c => c.ChurchId != churchId && c.ChurchName.StartsWith(firstLetter));

                var prefix = hasConflict && church.ChurchName.Length >= 2
                    ? church.ChurchName.Substring(0, 2).ToUpper()
                    : firstLetter;
                
                var existingMembers = await _context.Members
                    .FromSqlRaw($"SELECT * FROM Members WITH (UPDLOCK, HOLDLOCK) WHERE ChurchId = {churchId} AND UserId LIKE '{prefix}%'")
                    .Select(m => m.UserId)
                    .ToListAsync();

                var existingNumbers = existingMembers
                    .Select(uid => {
                        if (uid.Length <= prefix.Length) return 0;
                        var numPart = uid.Substring(prefix.Length);
                        return int.TryParse(numPart, out int num) ? num : 0;
                    })
                    .Where(n => n > 0)
                    .OrderBy(n => n)
                    .ToList();

                int nextNumber = 1;
                foreach (var num in existingNumbers)
                {
                    if (num == nextNumber)
                        nextNumber++;
                    else if (num > nextNumber)
                        break;
                }

                var userId = $"{prefix}{nextNumber:D2}";
                return Json(new { success = true, userId = userId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterMemberWithFamily(IFormFile? profileImage)
        {
            using var mainTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var churchId = int.Parse(Request.Form["churchId"]);
                var name = Request.Form["name"];
                var phone = Request.Form["phone"];
                var gender = Request.Form["gender"];
                var family = Request.Form["family"];
                var dateOfBirth = DateTime.Parse(Request.Form["dateOfBirth"]);
                var anniversaryDate = !string.IsNullOrEmpty(Request.Form["anniversaryDate"]) ? DateTime.Parse(Request.Form["anniversaryDate"]) : (DateTime?)null;
                var status = "Active";
                var joinedDate = !string.IsNullOrEmpty(Request.Form["joinedDate"]) ? DateTime.Parse(Request.Form["joinedDate"]) : DateTime.Today;
                var email = Request.Form["email"];

                // Generate User ID with lock - find first available gap
                var church = await _context.Churches.FindAsync(churchId);
                var firstLetter = church?.ChurchName.Substring(0, 1).ToUpper() ?? "O";
                var hasConflict = await _context.Churches
                    .AnyAsync(c => c.ChurchId != churchId && c.ChurchName.StartsWith(firstLetter));
                var prefix = hasConflict && church?.ChurchName != null && church.ChurchName.Length >= 2
                    ? church.ChurchName.Substring(0, 2).ToUpper()
                    : firstLetter;
                
                var existingMembers = await _context.Members
                    .FromSqlRaw($"SELECT * FROM Members WITH (UPDLOCK, HOLDLOCK) WHERE ChurchId = {churchId} AND UserId LIKE '{prefix}%'")
                    .Select(m => m.UserId)
                    .ToListAsync();

                var existingNumbers = existingMembers
                    .Select(uid => {
                        if (uid.Length <= prefix.Length) return 0;
                        var numPart = uid.Substring(prefix.Length);
                        return int.TryParse(numPart, out int num) ? num : 0;
                    })
                    .Where(n => n > 0)
                    .OrderBy(n => n)
                    .ToList();

                int nextNumber = 1;
                foreach (var num in existingNumbers)
                {
                    if (num == nextNumber)
                        nextNumber++;
                    else if (num > nextNumber)
                        break;
                }
                var userId = $"{prefix}{nextNumber:D2}";

                // Handle profile image
                string? imagePath = null;
                if (profileImage != null && profileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "members");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = $"{userId}_{Path.GetFileName(profileImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }
                    imagePath = $"/uploads/members/{uniqueFileName}";
                }

                var member = new Member
                {
                    UserId = userId,
                    Name = name,
                    Phone = phone,
                    Gender = gender,
                    Family = family,
                    DateOfBirth = dateOfBirth,
                    AnniversaryDate = anniversaryDate,
                    Email = email,
                    ChurchId = churchId,
                    JoinedDate = joinedDate,
                    Status = status,
                    CreatedDate = DateTime.Now,
                    CreatedBy = 0
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                // Add family members
                for (int i = 0; Request.Form.ContainsKey($"FamilyMembers[{i}].Name"); i++)
                {
                    var fmName = Request.Form[$"FamilyMembers[{i}].Name"].ToString();
                    var fmRelation = Request.Form[$"FamilyMembers[{i}].Relation"].ToString();
                    
                    if (!string.IsNullOrWhiteSpace(fmName) && !string.IsNullOrWhiteSpace(fmRelation))
                    {
                        var fmGender = Request.Form[$"FamilyMembers[{i}].Gender"].ToString();
                        var fmDobStr = Request.Form[$"FamilyMembers[{i}].DateOfBirth"].ToString();
                        var fmAnnivStr = Request.Form[$"FamilyMembers[{i}].AnniversaryDate"].ToString();
                        var fmPhone = Request.Form[$"FamilyMembers[{i}].Phone"].ToString();
                        var fmEmail = Request.Form[$"FamilyMembers[{i}].Email"].ToString();
                        
                        var existingFamilyMembers = await _context.Members
                            .FromSqlRaw($"SELECT * FROM Members WITH (UPDLOCK, HOLDLOCK) WHERE ChurchId = {churchId} AND UserId LIKE '{prefix}%'")
                            .Select(m => m.UserId)
                            .ToListAsync();

                        var existingFamilyNumbers = existingFamilyMembers
                            .Select(uid => {
                                if (uid.Length <= prefix.Length) return 0;
                                var numPart = uid.Substring(prefix.Length);
                                return int.TryParse(numPart, out int num) ? num : 0;
                            })
                            .Where(n => n > 0)
                            .OrderBy(n => n)
                            .ToList();

                        int nextFamilyNumber = 1;
                        foreach (var num in existingFamilyNumbers)
                        {
                            if (num == nextFamilyNumber)
                                nextFamilyNumber++;
                            else if (num > nextFamilyNumber)
                                break;
                        }
                        var familyUserId = $"{prefix}{nextFamilyNumber:D2}";

                        // Create Member record for family member
                        var familyMemberEntity = new Member
                        {
                            UserId = familyUserId,
                            Name = fmName,
                            Status = "Active",
                            ChurchId = churchId,
                            JoinedDate = joinedDate,
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            Phone = fmPhone,
                            Email = fmEmail,
                            Gender = !string.IsNullOrWhiteSpace(fmGender) ? fmGender : null,
                            DateOfBirth = !string.IsNullOrWhiteSpace(fmDobStr) && DateTime.TryParse(fmDobStr, out var dob) ? dob : null,
                            AnniversaryDate = !string.IsNullOrWhiteSpace(fmAnnivStr) && DateTime.TryParse(fmAnnivStr, out var anniv) ? anniv : null
                        };
                        _context.Members.Add(familyMemberEntity);
                        await _context.SaveChangesAsync();

                        // Add to FamilyMembers table
                        var fm = new FamilyMember
                        {
                            MemberId = member.MemberId,
                            Name = fmName,
                            Relation = fmRelation,
                            Gender = !string.IsNullOrWhiteSpace(fmGender) ? fmGender : null,
                            DateOfBirth = !string.IsNullOrWhiteSpace(fmDobStr) && DateTime.TryParse(fmDobStr, out var fmDob) ? fmDob : null,
                            AnniversaryDate = !string.IsNullOrWhiteSpace(fmAnnivStr) && DateTime.TryParse(fmAnnivStr, out var fmAnniv) ? fmAnniv : null,
                            Phone = fmPhone,
                            Email = fmEmail,
                            RelatedMemberId = familyMemberEntity.MemberId
                        };
                        _context.FamilyMembers.Add(fm);
                    }
                }
                await _context.SaveChangesAsync();

                // Collect all User IDs with relations
                var userIdsList = new List<object> { new { userId = userId, name = name.ToString(), relation = "You" } };
                
                var familyMembers = await _context.FamilyMembers
                    .Where(fm => fm.MemberId == member.MemberId)
                    .Join(_context.Members,
                        fm => fm.RelatedMemberId,
                        m => m.MemberId,
                        (fm, m) => new { userId = m.UserId, name = fm.Name, relation = fm.Relation })
                    .ToListAsync();
                
                userIdsList.AddRange(familyMembers);

                await mainTransaction.CommitAsync();
                return Json(new { success = true, userId = userId, userIdsList = userIdsList, message = "Member registered successfully!" });
            }
            catch (Exception ex)
            {
                await mainTransaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}