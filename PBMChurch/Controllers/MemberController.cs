// ...existing code...
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Attributes;
using PBMChurch.Data;
using PBMChurch.Models;
using System.Security.Claims;

namespace PBMChurch.Controllers
{
    [RoleAuthorize("Admin", "Member")]
    public class MemberController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MemberController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");

            // Return all members - JavaScript will handle filtering and pagination
            var members = await _context.Members
                .Include(m => m.Church)
                .Include(m => m.FamilyMembers)
                .Where(m => m.Status != "Inactive")
                .OrderBy(m => m.Name)
                .ToListAsync();

            return View(members);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var members = await _context.Members
                .Include(m => m.Church)
                .Where(m => m.Status != "Inactive")
                .OrderBy(m => m.Name)
                .ToListAsync();

            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                writer.WriteLine("User ID,Name,Phone,Gender,Sur Name,Date of Birth,Anniversary Date,Joined Date,Church,Email,Status");
                foreach (var member in members)
                {
                    writer.WriteLine($"{member.UserId},{member.Name},{member.Phone},{member.Gender},{member.Family},{member.DateOfBirth?.ToString("yyyy-MM-dd")},{member.AnniversaryDate?.ToString("yyyy-MM-dd")},{member.JoinedDate:yyyy-MM-dd},{member.Church?.ChurchName},{member.Email},{member.Status}");
                }
            }
            stream.Position = 0;
            return File(stream, "text/csv", $"Members_{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> Details(int id)
        {
            var member = await _context.Members
                .Include(m => m.Church)
                .Include(m => m.FamilyMembers)
                .FirstOrDefaultAsync(m => m.MemberId == id);

            if (member == null)
                return NotFound();

            return View(member);
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilyInfo(int memberId)
        {
            var familyMembers = new List<object>();
            
            // Check if this member has their own family
            var ownFamily = await _context.FamilyMembers
                .Where(fm => fm.MemberId == memberId && fm.RelatedMemberId.HasValue)
                .GroupBy(fm => fm.RelatedMemberId)
                .Select(g => g.First())
                .ToListAsync();
            
            if (ownFamily.Any())
            {
                // This is a parent, show their children
                foreach (var fm in ownFamily)
                {
                    familyMembers.Add(new {
                        Name = fm.Name,
                        Relation = fm.Relation,
                        Gender = fm.Gender,
                        DateOfBirth = fm.DateOfBirth,
                        Phone = fm.Phone,
                        Email = fm.Email,
                        RelatedMemberId = fm.RelatedMemberId
                    });
                }
            }
            else
            {
                // Check if this member is a child of someone
                var parentLink = await _context.FamilyMembers
                    .Where(fm => fm.RelatedMemberId == memberId)
                    .FirstOrDefaultAsync();
                
                if (parentLink != null)
                {
                    // This is a child, show parent and siblings
                    var parent = await _context.Members.FindAsync(parentLink.MemberId);
                    if (parent != null)
                    {
                        familyMembers.Add(new {
                            Name = parent.Name,
                            Relation = "Parent",
                            Gender = parent.Gender,
                            DateOfBirth = parent.DateOfBirth,
                            Phone = parent.Phone,
                            Email = parent.Email,
                            RelatedMemberId = parent.MemberId
                        });
                    }
                    
                    // Get siblings
                    var siblings = await _context.FamilyMembers
                        .Where(fm => fm.MemberId == parentLink.MemberId && fm.RelatedMemberId != memberId && fm.RelatedMemberId.HasValue)
                        .GroupBy(fm => fm.RelatedMemberId)
                        .Select(g => g.First())
                        .ToListAsync();
                    
                    foreach (var sibling in siblings)
                    {
                        familyMembers.Add(new {
                            Name = sibling.Name,
                            Relation = sibling.Relation,
                            Gender = sibling.Gender,
                            DateOfBirth = sibling.DateOfBirth,
                            Phone = sibling.Phone,
                            Email = sibling.Email,
                            RelatedMemberId = sibling.RelatedMemberId
                        });
                    }
                }
            }

            return Json(familyMembers);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            ViewBag.GenderOptions = new SelectList(new[] { 
                new { Value = "Male", Text = "Male" },
                new { Value = "Female", Text = "Female" }
            }, "Value", "Text");
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateUserId(int churchId)
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

            var newUserId = $"{prefix}{nextNumber:D2}";
            return Json(new { success = true, userId = newUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member, IFormFile? profileImage)
        {
            // Clear the FamilyMembers collection to prevent validation issues
            member.FamilyMembers = new List<FamilyMember>();
            
            // Remove family member validation errors since we handle them manually
            var keysToRemove = ModelState.Keys.Where(k => k.StartsWith("FamilyMembers")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
            
            if (ModelState.IsValid)
            {
                if (await _context.Members.AnyAsync(m => m.UserId == member.UserId))
                {
                    ModelState.AddModelError("UserId", "This User ID already exists");
                    ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", member.ChurchId);
                    ViewBag.GenderOptions = new SelectList(new[] { 
                        new { Value = "Male", Text = "Male" },
                        new { Value = "Female", Text = "Female" }
                    }, "Value", "Text", member.Gender);
                    return View(member);
                }

                // Handle image upload
                if (profileImage != null && profileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "members");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = $"{member.UserId}_{Guid.NewGuid()}{Path.GetExtension(profileImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(fileStream);
                    }
                    member.ProfileImage = $"/uploads/members/{uniqueFileName}";
                }

                // Set default values
                member.JoinedDate = DateTime.Today;
                member.Status = "Active";
                member.CreatedBy = GetCurrentAdminId();
                member.CreatedDate = DateTime.Now;
                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                // Save FamilyMembers and create Member records for each family member
                var familyKeys = Request.Form.Keys.Where(k => k.StartsWith("FamilyMembers[")).ToList();
                var familyIndices = familyKeys
                    .Select(k => {
                        var match = System.Text.RegularExpressions.Regex.Match(k, @"FamilyMembers\[(\d+)\]");
                        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
                    })
                    .Where(i => i >= 0)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();
                
                var church = await _context.Churches.FindAsync(member.ChurchId);
                var firstLetter = church?.ChurchName.Substring(0, 1).ToUpper() ?? "O";
                var hasConflict = await _context.Churches
                    .AnyAsync(c => c.ChurchId != member.ChurchId && c.ChurchName.StartsWith(firstLetter));
                var prefix = hasConflict && church?.ChurchName != null && church.ChurchName.Length >= 2
                    ? church.ChurchName.Substring(0, 2).ToUpper()
                    : firstLetter;

                foreach (var index in familyIndices)
                {
                    var name = Request.Form[$"FamilyMembers[{index}].Name"].FirstOrDefault();
                    var relation = Request.Form[$"FamilyMembers[{index}].Relation"].FirstOrDefault();
                    
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(relation))
                    {
                        var gender = Request.Form[$"FamilyMembers[{index}].Gender"].FirstOrDefault();
                        var dobStr = Request.Form[$"FamilyMembers[{index}].DateOfBirth"].FirstOrDefault();
                        var anniversaryStr = Request.Form[$"FamilyMembers[{index}].AnniversaryDate"].FirstOrDefault();
                        var phone = Request.Form[$"FamilyMembers[{index}].Phone"].FirstOrDefault();
                        var email = Request.Form[$"FamilyMembers[{index}].Email"].FirstOrDefault();
                        
                        var existingFamilyMembers = await _context.Members
                            .FromSqlRaw($"SELECT * FROM Members WITH (UPDLOCK, HOLDLOCK) WHERE ChurchId = {member.ChurchId} AND UserId LIKE '{prefix}%'")
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

                        int nextNumber = 1;
                        foreach (var num in existingFamilyNumbers)
                        {
                            if (num == nextNumber)
                                nextNumber++;
                            else if (num > nextNumber)
                                break;
                        }
                        var familyUserId = $"{prefix}{nextNumber:D2}";

                        // Create Member record for family member
                        var familyMemberEntity = new Member
                        {
                            UserId = familyUserId,
                            Name = name,
                            Status = "Active",
                            ChurchId = member.ChurchId,
                            JoinedDate = DateTime.Today,
                            CreatedBy = GetCurrentAdminId(),
                            CreatedDate = DateTime.Now,
                            Phone = phone,
                            Email = email,
                            Gender = !string.IsNullOrWhiteSpace(gender) ? gender : null,
                            DateOfBirth = !string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var dob) ? dob : null,
                            AnniversaryDate = !string.IsNullOrWhiteSpace(anniversaryStr) && DateTime.TryParse(anniversaryStr, out var anniversary) ? anniversary : null
                        };
                        _context.Members.Add(familyMemberEntity);
                        await _context.SaveChangesAsync();

                        // Add to FamilyMembers table, linking main member and family member
                        var fm = new FamilyMember
                        {
                            MemberId = member.MemberId,
                            Name = name,
                            Relation = relation,
                            Phone = phone,
                            Email = email,
                            Gender = !string.IsNullOrWhiteSpace(gender) ? gender : null,
                            DateOfBirth = !string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var fmDob) ? fmDob : null,
                            AnniversaryDate = !string.IsNullOrWhiteSpace(anniversaryStr) && DateTime.TryParse(anniversaryStr, out var fmAnniversary) ? fmAnniversary : null,
                            RelatedMemberId = familyMemberEntity.MemberId
                        };
                        _context.FamilyMembers.Add(fm);
                    }
                }
                    await _context.SaveChangesAsync();

                TempData["Success"] = "Member and family added successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", member.ChurchId);
            ViewBag.GenderOptions = new SelectList(new[] { 
                new { Value = "Male", Text = "Male" },
                new { Value = "Female", Text = "Female" }
            }, "Value", "Text", member.Gender);
            return View(member);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound();

            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", member.ChurchId);
            ViewBag.GenderOptions = new SelectList(new[] { 
                new { Value = "Male", Text = "Male" },
                new { Value = "Female", Text = "Female" }
            }, "Value", "Text", member.Gender);
            ViewBag.CurrentImage = member.ProfileImage;
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member, IFormFile? profileImage)
        {
            if (id != member.MemberId)
                return NotFound();

            // Remove family member validation errors since we handle them manually
            var keysToRemove = ModelState.Keys.Where(k => k.StartsWith("FamilyMembers[")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if UserId already exists for another member
                    if (await _context.Members.AnyAsync(m => m.UserId == member.UserId && m.MemberId != id))
                    {
                        ModelState.AddModelError("UserId", "This User ID already exists");
                        ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", member.ChurchId);
                        ViewBag.GenderOptions = new SelectList(new[] { 
                            new { Value = "Male", Text = "Male" },
                            new { Value = "Female", Text = "Female" }
                        }, "Value", "Text", member.Gender);
                        return View(member);
                    }

                    // Handle image upload
                    if (profileImage != null && profileImage.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(member.ProfileImage))
                        {
                            var oldImagePath = Path.Combine(_environment.WebRootPath, member.ProfileImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "members");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        var uniqueFileName = $"{member.UserId}_{Guid.NewGuid()}{Path.GetExtension(profileImage.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileImage.CopyToAsync(fileStream);
                        }
                        
                        member.ProfileImage = $"/uploads/members/{uniqueFileName}";
                    }

                    member.ModifiedBy = GetCurrentAdminId();
                    member.ModifiedDate = DateTime.Now;
                    _context.Update(member);
                    await _context.SaveChangesAsync();

                    // Handle family member updates
                    var existingFamily = await _context.FamilyMembers.Where(fm => fm.MemberId == id).ToListAsync();
                    var familyKeys = Request.Form.Keys.Where(k => k.StartsWith("FamilyMembers[")).ToList();
                    var familyIndices = familyKeys
                        .Select(k => {
                            var match = System.Text.RegularExpressions.Regex.Match(k, @"FamilyMembers\[(\d+)\]");
                            return match.Success ? int.Parse(match.Groups[1].Value) : -1;
                        })
                        .Where(i => i >= 0)
                        .Distinct()
                        .OrderBy(i => i)
                        .ToList();
                    
                    var processedRelatedMemberIds = new List<int>();
                    
                    foreach (var index in familyIndices)
                    {
                        var name = Request.Form[$"FamilyMembers[{index}].Name"].FirstOrDefault();
                        var relation = Request.Form[$"FamilyMembers[{index}].Relation"].FirstOrDefault();
                        var relatedMemberIdStr = Request.Form[$"FamilyMembers[{index}].RelatedMemberId"].FirstOrDefault();
                        
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(relation))
                        {
                            var gender = Request.Form[$"FamilyMembers[{index}].Gender"].FirstOrDefault();
                            var dobStr = Request.Form[$"FamilyMembers[{index}].DateOfBirth"].FirstOrDefault();
                            var anniversaryStr = Request.Form[$"FamilyMembers[{index}].AnniversaryDate"].FirstOrDefault();
                            var phone = Request.Form[$"FamilyMembers[{index}].Phone"].FirstOrDefault();
                            var email = Request.Form[$"FamilyMembers[{index}].Email"].FirstOrDefault();
                            
                            FamilyMember? existingFm = null;
                            if (!string.IsNullOrWhiteSpace(relatedMemberIdStr) && int.TryParse(relatedMemberIdStr, out var relatedMemberId) && relatedMemberId > 0)
                            {
                                existingFm = existingFamily.FirstOrDefault(f => f.RelatedMemberId == relatedMemberId);
                                if (existingFm != null)
                                {
                                    processedRelatedMemberIds.Add(relatedMemberId);
                                }
                            }
                            
                            if (existingFm != null)
                            {
                                // Update existing family member
                                existingFm.Name = name;
                                existingFm.Relation = relation;
                                existingFm.Gender = !string.IsNullOrWhiteSpace(gender) ? gender : null;
                                existingFm.DateOfBirth = !string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var fmDob) ? fmDob : null;
                                existingFm.AnniversaryDate = !string.IsNullOrWhiteSpace(anniversaryStr) && DateTime.TryParse(anniversaryStr, out var fmAnniversary) ? fmAnniversary : null;
                                existingFm.Phone = phone;
                                existingFm.Email = email;
                                
                                // Update related member record
                                if (existingFm.RelatedMemberId.HasValue)
                                {
                                    var relatedMember = await _context.Members.FindAsync(existingFm.RelatedMemberId.Value);
                                    if (relatedMember != null)
                                    {
                                        relatedMember.Name = name;
                                        relatedMember.Gender = !string.IsNullOrWhiteSpace(gender) ? gender : null;
                                        relatedMember.DateOfBirth = !string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var dob) ? dob : null;
                                        relatedMember.AnniversaryDate = !string.IsNullOrWhiteSpace(anniversaryStr) && DateTime.TryParse(anniversaryStr, out var anniversary) ? anniversary : null;
                                        relatedMember.Phone = phone;
                                        relatedMember.Email = email;
                                    }
                                }
                            }
                            else
                            {
                                // Add new family member
                                var church = await _context.Churches.FindAsync(member.ChurchId);
                                var firstLetter = church?.ChurchName.Substring(0, 1).ToUpper() ?? "O";
                                var hasConflict = await _context.Churches
                                    .AnyAsync(c => c.ChurchId != member.ChurchId && c.ChurchName.StartsWith(firstLetter));
                                var prefix = hasConflict && church?.ChurchName != null && church.ChurchName.Length >= 2
                                    ? church.ChurchName.Substring(0, 2).ToUpper()
                                    : firstLetter;
                                
                                var existingEditMembers = await _context.Members
                                    .FromSqlRaw($"SELECT * FROM Members WITH (UPDLOCK, HOLDLOCK) WHERE ChurchId = {member.ChurchId} AND UserId LIKE '{prefix}%'")
                                    .Select(m => m.UserId)
                                    .ToListAsync();

                                var existingEditNumbers = existingEditMembers
                                    .Select(uid => {
                                        if (uid.Length <= prefix.Length) return 0;
                                        var numPart = uid.Substring(prefix.Length);
                                        return int.TryParse(numPart, out int num) ? num : 0;
                                    })
                                    .Where(n => n > 0)
                                    .OrderBy(n => n)
                                    .ToList();

                                int nextNumber = 1;
                                foreach (var num in existingEditNumbers)
                                {
                                    if (num == nextNumber)
                                        nextNumber++;
                                    else if (num > nextNumber)
                                        break;
                                }
                                var familyUserId = $"{prefix}{nextNumber:D2}";

                                var familyMemberEntity = new Member
                                {
                                    UserId = familyUserId,
                                    Name = name,
                                    Status = "Active",
                                    ChurchId = member.ChurchId,
                                    JoinedDate = DateTime.Today,
                                    CreatedBy = GetCurrentAdminId(),
                                    CreatedDate = DateTime.Now,
                                    Phone = phone,
                                    Email = email,
                                    Gender = !string.IsNullOrWhiteSpace(gender) ? gender : null,
                                    DateOfBirth = !string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var familyDob) ? familyDob : null,
                                    AnniversaryDate = !string.IsNullOrWhiteSpace(anniversaryStr) && DateTime.TryParse(anniversaryStr, out var familyAnniversary) ? familyAnniversary : null
                                };
                                _context.Members.Add(familyMemberEntity);
                                await _context.SaveChangesAsync();
                                
                                var fm = new FamilyMember
                                {
                                    MemberId = member.MemberId,
                                    Name = name,
                                    Relation = relation,
                                    Gender = !string.IsNullOrWhiteSpace(gender) ? gender : null,
                                    DateOfBirth = !string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var fmDob) ? fmDob : null,
                                    AnniversaryDate = !string.IsNullOrWhiteSpace(anniversaryStr) && DateTime.TryParse(anniversaryStr, out var fmAnniversary) ? fmAnniversary : null,
                                    Phone = phone,
                                    Email = email,
                                    RelatedMemberId = familyMemberEntity.MemberId
                                };
                                _context.FamilyMembers.Add(fm);
                                processedRelatedMemberIds.Add(familyMemberEntity.MemberId);
                            }
                        }
                    }
                    
                    // Remove family members that were not in the form (deleted by user)
                    var familyToRemove = existingFamily.Where(f => f.RelatedMemberId.HasValue && !processedRelatedMemberIds.Contains(f.RelatedMemberId.Value)).ToList();
                    foreach (var fm in familyToRemove)
                    {
                        if (fm.RelatedMemberId.HasValue)
                        {
                            var relatedMember = await _context.Members.FindAsync(fm.RelatedMemberId.Value);
                            if (relatedMember != null)
                            {
                                _context.Members.Remove(relatedMember);
                            }
                        }
                        _context.FamilyMembers.Remove(fm);
                    }
                    
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Member updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", member.ChurchId);
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                // Delete all related records to avoid foreign key constraint violations
                var attendance = await _context.Attendance.Where(a => a.MemberId == id).ToListAsync();
                _context.Attendance.RemoveRange(attendance);
                
                var notes = await _context.MemberNotes.Where(n => n.MemberId == id).ToListAsync();
                _context.MemberNotes.RemoveRange(notes);
                
                var offerings = await _context.MemberOfferings.Where(o => o.MemberId == id).ToListAsync();
                _context.MemberOfferings.RemoveRange(offerings);
                
                var prayers = await _context.PrayerRequests.Where(p => p.MemberId == id).ToListAsync();
                _context.PrayerRequests.RemoveRange(prayers);
                
                // Handle family member records
                var familyMembers = await _context.FamilyMembers.Where(fm => fm.MemberId == id).ToListAsync();
                
                if (familyMembers.Any())
                {
                    // This member is a parent - reassign children to first child as new parent
                    var firstChild = familyMembers.FirstOrDefault();
                    if (firstChild != null && firstChild.RelatedMemberId.HasValue)
                    {
                        var newParentId = firstChild.RelatedMemberId.Value;
                        
                        // Reassign remaining children to the new parent
                        foreach (var fm in familyMembers.Skip(1))
                        {
                            if (fm.RelatedMemberId.HasValue)
                            {
                                _context.FamilyMembers.Add(new FamilyMember
                                {
                                    MemberId = newParentId,
                                    RelatedMemberId = fm.RelatedMemberId,
                                    Name = fm.Name,
                                    Relation = fm.Relation,
                                    Gender = fm.Gender,
                                    DateOfBirth = fm.DateOfBirth,
                                    AnniversaryDate = fm.AnniversaryDate,
                                    Phone = fm.Phone,
                                    Email = fm.Email
                                });
                            }
                        }
                    }
                    
                    // Delete old parent's family records
                    _context.FamilyMembers.RemoveRange(familyMembers);
                }
                
                // Delete family member records where this member is a child
                var childRecords = await _context.FamilyMembers.Where(fm => fm.RelatedMemberId == id).ToListAsync();
                _context.FamilyMembers.RemoveRange(childRecords);
                
                // Hard delete the member
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX Methods for Member Details Page
        [HttpGet]
        public async Task<IActionResult> GetNotes(int memberId)
        {
            var notes = await _context.MemberNotes
                .Where(n => n.MemberId == memberId)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new
                {
                    n.NoteId,
                    n.NoteText,
                    n.CreatedBy,
                    n.CreatedDate
                })
                .ToListAsync();
            
            return Json(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNote(int memberId, string noteText)
        {
            try
            {
                var note = new MemberNote
                {
                    MemberId = memberId,
                    NoteText = noteText,
                    CreatedBy = GetCurrentAdminId(),
                    CreatedDate = DateTime.Now
                };
                
                _context.MemberNotes.Add(note);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendance(int memberId)
        {
            var attendance = await _context.Attendance
                .Where(a => a.MemberId == memberId)
                .Include(a => a.Church)
                .OrderByDescending(a => a.AttendanceDate)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.AttendanceDate,
                    ChurchName = a.Church != null ? a.Church.ChurchName : "N/A",
                    a.IsPresent,
                    MarkedDate = a.MarkedDate
                })
                .ToListAsync();
            
            return Json(attendance);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrayerRequests(int memberId)
        {
            var prayers = await _context.PrayerRequests
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.RequestDate)
                .Select(p => new
                {
                    p.RequestId,
                    p.Title,
                    p.Request,
                    p.RequestDate,
                    p.Status
                })
                .ToListAsync();
            
            return Json(prayers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrayerStatus(int requestId, string status)
        {
            try
            {
                var prayerRequest = await _context.PrayerRequests.FindAsync(requestId);
                if (prayerRequest == null)
                    return Json(new { success = false, message = "Prayer request not found" });

                prayerRequest.Status = status;
                prayerRequest.ModifiedBy = GetCurrentAdminId();
                prayerRequest.ModifiedDate = DateTime.Now;
                
                if (status == "Answered")
                    prayerRequest.AnsweredDate = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePrayerRequest(int memberId, string title, string request)
        {
            try
            {
                var prayerRequest = new PrayerRequest
                {
                    MemberId = memberId,
                    Title = title,
                    Request = request,
                    RequestDate = DateTime.Now,
                    Status = "Pending",
                    CreatedBy = GetCurrentAdminId(),
                    CreatedDate = DateTime.Now
                };
                
                _context.PrayerRequests.Add(prayerRequest);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOfferings(int memberId)
        {
            var offerings = await _context.MemberOfferings
                .Where(o => o.MemberId == memberId)
                .OrderByDescending(o => o.Date)
                .Select(o => new
                {
                    o.OfferingId,
                    o.Date,
                    o.Category,
                    o.Amount,
                    o.Purpose
                })
                .ToListAsync();
            
            return Json(offerings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOffering(int memberId, DateTime date, string category, decimal amount, string? purpose)
        {
            try
            {
                var offering = new MemberOffering
                {
                    MemberId = memberId,
                    Date = date,
                    Category = category,
                    Amount = amount,
                    Purpose = purpose,
                    CreatedBy = GetCurrentAdminId(),
                    CreatedDate = DateTime.Now
                };
                
                _context.MemberOfferings.Add(offering);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(int memberId)
        {
            var history = new List<object>();

            // Add notes to history
            var notes = await _context.MemberNotes
                .Where(n => n.MemberId == memberId)
                .Select(n => new
                {
                    Type = "Note",
                    Action = "Note Added: " + (n.NoteText.Length > 50 ? n.NoteText.Substring(0, 50) + "..." : n.NoteText),
                    Date = n.CreatedDate
                })
                .ToListAsync();
            history.AddRange(notes);

            // Add attendance to history
            var attendance = await _context.Attendance
                .Where(a => a.MemberId == memberId)
                .Select(a => new
                {
                    Type = "Attendance",
                    Action = (a.IsPresent ? "Marked Present" : "Marked Absent") + " on " + a.AttendanceDate.ToString("MMM dd, yyyy"),
                    Date = a.MarkedDate
                })
                .ToListAsync();
            history.AddRange(attendance);

            // Add prayer requests to history
            var prayers = await _context.PrayerRequests
                .Where(p => p.MemberId == memberId)
                .Select(p => new
                {
                    Type = "Prayer",
                    Action = "Prayer Request: " + (p.Title ?? (p.Request.Length > 50 ? p.Request.Substring(0, 50) + "..." : p.Request)),
                    Date = p.CreatedDate
                })
                .ToListAsync();
            history.AddRange(prayers);

            // Add offerings to history
            var offerings = await _context.MemberOfferings
                .Where(o => o.MemberId == memberId)
                .Select(o => new
                {
                    Type = "Offering",
                    Action = $"Offering Recorded: ₹{o.Amount} - {o.Category}",
                    Date = o.CreatedDate
                })
                .ToListAsync();
            history.AddRange(offerings);

            // Sort by date descending
            var sortedHistory = history.OrderByDescending(h => ((dynamic)h).Date).ToList();
            
            return Json(sortedHistory);
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}
