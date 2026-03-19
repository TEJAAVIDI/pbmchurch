using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBMChurch.Data;
using PBMChurch.Models;
using PBMChurch.Models.ViewModels;
using System.Globalization;

namespace PBMChurch.Controllers
{
    [Authorize]
    public class TaskCalendarController : Controller
    {
        private readonly AppDbContext _context;

        public TaskCalendarController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var tasks = await GetTasksForPeriod(startOfMonth, endOfMonth);
            var todayTasks = tasks.Where(t => t.StartDate.Date == today).ToList();

            var viewModel = new TaskCalendarViewModel
            {
                Events = tasks,
                CurrentDate = today,
                TodayTaskCount = todayTasks.Count,
                TodayTasks = todayTasks
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
        {
            var events = await GetTasksForPeriod(start, end);
            return Json(events.Select(e => new
            {
                id = e.TaskId,
                title = e.Summary,
                start = e.IsAllDay ? e.StartDate.ToString("yyyy-MM-dd") : 
                       e.StartDate.ToString("yyyy-MM-dd") + "T" + e.StartTime,
                end = e.IsAllDay ? e.EndDate.AddDays(1).ToString("yyyy-MM-dd") : 
                     e.EndDate.ToString("yyyy-MM-dd") + "T" + e.EndTime,
                allDay = e.IsAllDay,
                backgroundColor = e.TypeColor,
                borderColor = e.TypeColor,
                textColor = "#ffffff",
                extendedProps = new
                {
                    description = e.Description,
                    taskType = e.TaskType,
                    relatedName = e.RelatedName,
                    icon = e.TypeIcon
                }
            }));
        }

        [HttpGet]
        public async Task<IActionResult> GetDayTasks(DateTime date)
        {
            var tasks = await GetTasksForPeriod(date, date);
            return Json(tasks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateTaskViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            };
            return PartialView("_CreateTaskModal", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Always check for conflicts and show warning, but don't prevent creation
                if (!model.IsAllDay && !string.IsNullOrEmpty(model.StartTime) && !string.IsNullOrEmpty(model.EndTime))
                {
                    var conflicts = await CheckTimeConflicts(model.StartDate, model.StartTime, model.EndTime, 0);
                    if (conflicts.Any() && !model.IgnoreConflicts)
                    {
                        ViewBag.ConflictMessage = $"Time conflict detected with {conflicts.Count} existing task(s). You can still create this task.";
                        ViewBag.Conflicts = conflicts;
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return PartialView("_CreateTaskModal", model);
                        }
                    }
                }

                var adminId = GetCurrentAdminId();
                
                var task = new CalendarTask
                {
                    Summary = model.Summary,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsAllDay = model.IsAllDay,
                    CreatedBy = adminId
                };

                if (!model.IsAllDay)
                {
                    if (TimeSpan.TryParse(model.StartTime, out var startTime))
                        task.StartTime = startTime;
                    if (TimeSpan.TryParse(model.EndTime, out var endTime))
                        task.EndTime = endTime;
                }

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task created successfully!";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirect = Url.Action("Index") });
                }
                
                return RedirectToAction("Index");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateTaskModal", model);
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var model = new EditTaskViewModel
            {
                TaskId = task.TaskId,
                Summary = task.Summary,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsAllDay = task.IsAllDay,
                StartTime = task.StartTime?.ToString(@"hh\:mm"),
                EndTime = task.EndTime?.ToString(@"hh\:mm")
            };

            return PartialView("_EditTaskModal", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = await _context.Tasks.FindAsync(model.TaskId);
                if (task == null) return NotFound();

                task.Summary = model.Summary;
                task.Description = model.Description;
                task.StartDate = model.StartDate;
                task.EndDate = model.EndDate;
                task.IsAllDay = model.IsAllDay;
                task.ModifiedBy = GetCurrentAdminId();
                task.ModifiedDate = DateTime.Now;

                if (!model.IsAllDay)
                {
                    if (TimeSpan.TryParse(model.StartTime, out var startTime))
                        task.StartTime = startTime;
                    if (TimeSpan.TryParse(model.EndTime, out var endTime))
                        task.EndTime = endTime;
                }
                else
                {
                    task.StartTime = null;
                    task.EndTime = null;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Task updated successfully!";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirect = Url.Action("Index") });
                }
                
                return RedirectToAction("Index");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditTaskModal", model);
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Move(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var model = new MoveTaskViewModel
            {
                TaskId = task.TaskId,
                NewDate = task.StartDate,
                NewStartTime = task.StartTime?.ToString(@"hh\:mm"),
                NewEndTime = task.EndTime?.ToString(@"hh\:mm")
            };

            return PartialView("_MoveTaskModal", model);
        }

        [HttpPost]
        public async Task<IActionResult> Move(MoveTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var task = await _context.Tasks.FindAsync(model.TaskId);
                if (task == null) return NotFound();

                // Always check for conflicts and show warning, but don't prevent move
                var conflicts = await CheckTimeConflicts(model.NewDate, model.NewStartTime, model.NewEndTime, model.TaskId);
                
                if (conflicts.Any() && !model.IgnoreConflicts)
                {
                    model.ConflictingTasks = conflicts;
                    ViewBag.HasConflicts = true;
                    ViewBag.ConflictMessage = $"Time conflict detected with {conflicts.Count} existing task(s). You can still move this task.";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return PartialView("_MoveTaskModal", model);
                    }
                }

                task.StartDate = model.NewDate;
                task.EndDate = model.NewDate;
                task.ModifiedBy = GetCurrentAdminId();
                task.ModifiedDate = DateTime.Now;

                if (!string.IsNullOrEmpty(model.NewStartTime) && TimeSpan.TryParse(model.NewStartTime, out var startTime))
                    task.StartTime = startTime;
                if (!string.IsNullOrEmpty(model.NewEndTime) && TimeSpan.TryParse(model.NewEndTime, out var endTime))
                    task.EndTime = endTime;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Task moved successfully!";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirect = Url.Action("Index") });
                }
                
                return RedirectToAction("Index");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MoveTaskModal", model);
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.IsActive = false;
            task.ModifiedBy = GetCurrentAdminId();
            task.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Task deleted successfully!";
            return Json(new { success = true });
        }

        private async Task<List<TaskEventViewModel>> GetTasksForPeriod(DateTime start, DateTime end)
        {
            var tasks = new List<TaskEventViewModel>();

            // Get custom tasks
            var customTasks = await _context.Tasks
                .Where(t => t.IsActive && t.StartDate >= start && t.StartDate <= end)
                .Select(t => new TaskEventViewModel
                {
                    TaskId = t.TaskId,
                    Summary = t.Summary,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    StartTime = t.StartTime.HasValue ? t.StartTime.Value.ToString(@"hh\:mm") : null,
                    EndDate = t.EndDate,
                    EndTime = t.EndTime.HasValue ? t.EndTime.Value.ToString(@"hh\:mm") : null,
                    IsAllDay = t.IsAllDay,
                    TaskType = t.TaskType,
                    TypeColor = "#007bff",
                    TypeIcon = "fas fa-tasks"
                })
                .ToListAsync();

            tasks.AddRange(customTasks);

            // Get birthdays
            var birthdays = await _context.Members
                .Where(m => m.Status == "Active" && m.DateOfBirth.HasValue)
                .ToListAsync();

            foreach (var member in birthdays)
            {
                var birthday = new DateTime(start.Year, member.DateOfBirth!.Value.Month, member.DateOfBirth.Value.Day);
                if (birthday >= start && birthday <= end)
                {
                    tasks.Add(new TaskEventViewModel
                    {
                        TaskId = 0,
                        Summary = $"{member.Name}'s Birthday",
                        Description = $"Birthday celebration for {member.Name}",
                        StartDate = birthday,
                        EndDate = birthday,
                        IsAllDay = true,
                        TaskType = "Birthday",
                        TypeColor = "#dc3545",
                        TypeIcon = "fas fa-birthday-cake",
                        RelatedName = member.Name
                    });
                }
            }

            // Get church meetings
            var churches = await _context.Churches.Where(c => c.Status == "Active").ToListAsync();
            var currentDate = start;
            
            while (currentDate <= end)
            {
                var dayName = currentDate.DayOfWeek.ToString();
                
                foreach (var church in churches)
                {
                    if (church.MeetingDay1 == dayName || church.MeetingDay2 == dayName)
                    {
                        tasks.Add(new TaskEventViewModel
                        {
                            TaskId = 0,
                            Summary = $"{church.ChurchName} Meeting",
                            Description = $"Regular church meeting at {church.Location}",
                            StartDate = currentDate,
                            EndDate = currentDate,
                            IsAllDay = false,
                            StartTime = "10:00",
                            EndTime = "12:00",
                            TaskType = "ChurchMeeting",
                            TypeColor = "#28a745",
                            TypeIcon = "fas fa-church",
                            RelatedName = church.ChurchName
                        });
                    }
                }
                
                currentDate = currentDate.AddDays(1);
            }

            // Format time display
            foreach (var task in tasks)
            {
                if (task.IsAllDay)
                {
                    task.FormattedTime = "All Day";
                }
                else if (!string.IsNullOrEmpty(task.StartTime) && !string.IsNullOrEmpty(task.EndTime))
                {
                    if (TimeSpan.TryParse(task.StartTime, out var start24) && TimeSpan.TryParse(task.EndTime, out var end24))
                    {
                        var startTime12 = DateTime.Today.Add(start24).ToString("h:mm tt");
                        var endTime12 = DateTime.Today.Add(end24).ToString("h:mm tt");
                        task.FormattedTime = $"{startTime12} - {endTime12}";
                    }
                    else
                    {
                        task.FormattedTime = $"{task.StartTime} - {task.EndTime}";
                    }
                }
                else if (!string.IsNullOrEmpty(task.StartTime))
                {
                    if (TimeSpan.TryParse(task.StartTime, out var start24))
                    {
                        var startTime12 = DateTime.Today.Add(start24).ToString("h:mm tt");
                        task.FormattedTime = $"From {startTime12}";
                    }
                    else
                    {
                        task.FormattedTime = $"From {task.StartTime}";
                    }
                }
            }

            return tasks.OrderBy(t => t.StartDate).ThenBy(t => t.StartTime).ToList();
        }

        private async Task<List<TaskEventViewModel>> CheckTimeConflicts(DateTime date, string? startTime, string? endTime, int excludeTaskId)
        {
            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                return new List<TaskEventViewModel>();

            if (!TimeSpan.TryParse(startTime, out var newStart) || !TimeSpan.TryParse(endTime, out var newEnd))
                return new List<TaskEventViewModel>();

            // Get all tasks for the same date (including custom tasks, church meetings, etc.)
            var dayTasks = await GetTasksForPeriod(date, date);
            var conflicts = new List<TaskEventViewModel>();

            foreach (var task in dayTasks.Where(t => t.TaskId != excludeTaskId && !t.IsAllDay))
            {
                if (!string.IsNullOrEmpty(task.StartTime) && !string.IsNullOrEmpty(task.EndTime))
                {
                    if (TimeSpan.TryParse(task.StartTime, out var taskStart) && 
                        TimeSpan.TryParse(task.EndTime, out var taskEnd))
                    {
                        // Check for overlap: new task overlaps if it starts before existing ends and ends after existing starts
                        if (newStart < taskEnd && newEnd > taskStart)
                        {
                            conflicts.Add(task);
                        }
                    }
                }
            }

            return conflicts;
        }

        private int GetCurrentAdminId()
        {
            return int.Parse(User.FindFirst("AdminId")?.Value ?? "1");
        }
    }
}