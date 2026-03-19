namespace PBMChurch.Models.ViewModels
{
    public class ReportFilterViewModel
    {
        public int? ChurchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; } // Present/Absent
    }

    public class AttendanceReportViewModel
    {
        public ReportFilterViewModel Filters { get; set; } = new();
        public List<AttendanceReportItem> ReportData { get; set; } = new();
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalRecords { get; set; }
    }

    public class AttendanceReportItem
    {
        public DateTime AttendanceDate { get; set; }
        public int WeekNumber { get; set; }
        public string ChurchName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string Status => IsPresent ? "Present" : "Absent";
    }

    public class FinancialReportViewModel
    {
        public int? ChurchId { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public int Month { get; set; } = DateTime.Now.Month;
        
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        
        public List<IncomeReportItem> IncomeItems { get; set; } = new();
        public List<ExpenseReportItem> ExpenseItems { get; set; } = new();
    }

    public class IncomeReportItem
    {
        public int IncomeId { get; set; }
        public string ChurchName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime IncomeDate { get; set; }
        public string? Description { get; set; }
    }

    public class ExpenseReportItem
    {
        public int ExpenseId { get; set; }
        public string ChurchName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
    }
}
