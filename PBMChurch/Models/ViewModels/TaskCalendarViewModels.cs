using System.ComponentModel.DataAnnotations;

namespace PBMChurch.Models.ViewModels
{
    public class TaskCalendarViewModel
    {
        public List<TaskEventViewModel> Events { get; set; } = new();
        public DateTime CurrentDate { get; set; } = DateTime.Today;
        public int TodayTaskCount { get; set; }
        public List<TaskEventViewModel> TodayTasks { get; set; } = new();
    }

    public class TaskEventViewModel
    {
        public int TaskId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public string? StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string? EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public string TypeColor { get; set; } = "#007bff";
        public string TypeIcon { get; set; } = "fas fa-tasks";
        public string? RelatedName { get; set; }
        public string FormattedTime { get; set; } = string.Empty;
    }

    public class CreateTaskViewModel
    {
        [Required]
        [StringLength(200)]
        public string Summary { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        public string? StartTime { get; set; }

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public string? EndTime { get; set; }

        public bool IsAllDay { get; set; }
        
        public bool IgnoreConflicts { get; set; }
    }

    public class EditTaskViewModel : CreateTaskViewModel
    {
        public int TaskId { get; set; }
    }

    public class MoveTaskViewModel
    {
        public int TaskId { get; set; }
        
        [Required]
        public DateTime NewDate { get; set; }
        
        public string? NewStartTime { get; set; }
        public string? NewEndTime { get; set; }
        public List<TaskEventViewModel> ConflictingTasks { get; set; } = new();
        
        public bool IgnoreConflicts { get; set; }
    }
}