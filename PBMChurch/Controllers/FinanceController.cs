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
    [RoleAuthorize("Admin")]
    public class FinanceController : Controller
    {
        private readonly AppDbContext _context;

        public FinanceController(AppDbContext context)
        {
            _context = context;
        }

        // Income Actions
        public async Task<IActionResult> Income(int? churchId, int? month, int? year)
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", churchId);
            ViewBag.SelectedChurch = churchId;
            ViewBag.SelectedMonth = month ?? DateTime.Now.Month;
            ViewBag.SelectedYear = year ?? DateTime.Now.Year;

            var query = _context.Income.Include(i => i.Church).AsQueryable();

            if (churchId.HasValue)
                query = query.Where(i => i.ChurchId == churchId.Value);

            if (month.HasValue)
                query = query.Where(i => i.IncomeDate.Month == month.Value);

            if (year.HasValue)
                query = query.Where(i => i.IncomeDate.Year == year.Value);

            var incomes = await query.OrderByDescending(i => i.IncomeDate).ToListAsync();
            return View(incomes);
        }

        // Financial Summary
        public async Task<IActionResult> Summary(int? churchId, int? month, int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;
            var selectedMonth = month;

            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", churchId);
            ViewBag.SelectedChurchId = churchId?.ToString();
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            // Get selected church name
            if (churchId.HasValue)
            {
                var church = await _context.Churches.FindAsync(churchId.Value);
                ViewBag.SelectedChurchName = church?.ChurchName;
            }

            // Build income query
            var incomeQuery = _context.Income.AsQueryable();
            if (churchId.HasValue)
                incomeQuery = incomeQuery.Where(i => i.ChurchId == churchId.Value);
            if (month.HasValue)
                incomeQuery = incomeQuery.Where(i => i.IncomeDate.Month == month.Value);
            if (year.HasValue)
                incomeQuery = incomeQuery.Where(i => i.IncomeDate.Year == year.Value);

            // Build expense query
            var expenseQuery = _context.Expenses.AsQueryable();
            if (churchId.HasValue)
                expenseQuery = expenseQuery.Where(e => e.ChurchId == churchId.Value);
            if (month.HasValue)
                expenseQuery = expenseQuery.Where(e => e.ExpenseDate.Month == month.Value);
            if (year.HasValue)
                expenseQuery = expenseQuery.Where(e => e.ExpenseDate.Year == year.Value);

            // Calculate totals
            var totalIncome = await incomeQuery.SumAsync(i => (decimal?)i.Amount) ?? 0;
            var totalExpenses = await expenseQuery.SumAsync(e => (decimal?)e.Amount) ?? 0;
            var netProfit = totalIncome - totalExpenses;

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.NetProfit = netProfit;

            // Income by Source
            var incomeBySource = await incomeQuery
                .GroupBy(i => i.Source)
                .Select(g => new { Source = g.Key, Total = g.Sum(i => i.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();
            ViewBag.IncomeBySource = incomeBySource.Cast<dynamic>().ToList();

            // Expense by Category
            var expenseByCategory = await expenseQuery
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();
            ViewBag.ExpenseByCategory = expenseByCategory.Cast<dynamic>().ToList();

            // Church-wise Summary (only if no specific church is selected)
            if (!churchId.HasValue)
            {
                var churches = await _context.Churches.Where(c => c.Status == "Active").ToListAsync();
                var churchSummary = new List<dynamic>();

                foreach (var church in churches)
                {
                    var churchIncomeQuery = _context.Income.Where(i => i.ChurchId == church.ChurchId);
                    var churchExpenseQuery = _context.Expenses.Where(e => e.ChurchId == church.ChurchId);

                    if (month.HasValue)
                    {
                        churchIncomeQuery = churchIncomeQuery.Where(i => i.IncomeDate.Month == month.Value);
                        churchExpenseQuery = churchExpenseQuery.Where(e => e.ExpenseDate.Month == month.Value);
                    }

                    if (year.HasValue)
                    {
                        churchIncomeQuery = churchIncomeQuery.Where(i => i.IncomeDate.Year == year.Value);
                        churchExpenseQuery = churchExpenseQuery.Where(e => e.ExpenseDate.Year == year.Value);
                    }

                    var churchIncome = await churchIncomeQuery.SumAsync(i => (decimal?)i.Amount) ?? 0;
                    var churchExpenses = await churchExpenseQuery.SumAsync(e => (decimal?)e.Amount) ?? 0;

                    churchSummary.Add(new
                    {
                        ChurchName = church.ChurchName,
                        Income = churchIncome,
                        Expenses = churchExpenses
                    });
                }

                ViewBag.ChurchSummary = churchSummary;
            }
            else
            {
                ViewBag.ChurchSummary = new List<dynamic>();
            }

            return View();
        }

        // Profit/Loss Report - Monthly breakdown
        public async Task<IActionResult> ProfitLoss(int? churchId, int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", churchId);
            ViewBag.SelectedChurchId = churchId;
            ViewBag.SelectedYear = selectedYear;

            // Get selected church name
            if (churchId.HasValue)
            {
                var church = await _context.Churches.FindAsync(churchId.Value);
                ViewBag.SelectedChurchName = church?.ChurchName;
            }

            // Build monthly data
            var monthlyData = new List<MonthlyFinancialData>();

            for (int month = 1; month <= 12; month++)
            {
                var incomeQuery = _context.Income.Where(i => i.IncomeDate.Year == selectedYear && i.IncomeDate.Month == month);
                var expenseQuery = _context.Expenses.Where(e => e.ExpenseDate.Year == selectedYear && e.ExpenseDate.Month == month);

                if (churchId.HasValue)
                {
                    incomeQuery = incomeQuery.Where(i => i.ChurchId == churchId.Value);
                    expenseQuery = expenseQuery.Where(e => e.ChurchId == churchId.Value);
                }

                var monthIncome = await incomeQuery.SumAsync(i => (decimal?)i.Amount) ?? 0;
                var monthExpenses = await expenseQuery.SumAsync(e => (decimal?)e.Amount) ?? 0;
                var monthProfit = monthIncome - monthExpenses;

                monthlyData.Add(new MonthlyFinancialData
                {
                    Month = new DateTime(selectedYear, month, 1).ToString("MMMM"),
                    Income = monthIncome,
                    Expenses = monthExpenses,
                    Profit = monthProfit
                });
            }

            ViewBag.MonthlyData = monthlyData;

            // Calculate year totals
            var yearIncome = monthlyData.Sum(m => m.Income);
            var yearExpenses = monthlyData.Sum(m => m.Expenses);
            var yearProfit = yearIncome - yearExpenses;

            ViewBag.YearIncome = yearIncome;
            ViewBag.YearExpenses = yearExpenses;
            ViewBag.YearProfit = yearProfit;

            return View();
        }

        // Helper class for monthly financial data
        public class MonthlyFinancialData
        {
            public string Month { get; set; } = string.Empty;
            public decimal Income { get; set; }
            public decimal Expenses { get; set; }
            public decimal Profit { get; set; }
        }

        public async Task<IActionResult> CreateIncome()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetIncomeForEdit(int id)
        {
            var income = await _context.Income.FindAsync(id);
            if (income == null)
                return NotFound();

            return Json(new
            {
                success = true,
                income = new
                {
                    incomeId = income.IncomeId,
                    churchId = income.ChurchId,
                    incomeDate = income.IncomeDate.ToString("yyyy-MM-dd"),
                    source = income.Source,
                    amount = income.Amount,
                    description = income.Description
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIncome(Income income)
        {
            if (ModelState.IsValid)
            {
                income.AddedBy = GetCurrentAdminId();
                income.AddedDate = DateTime.Now;
                _context.Income.Add(income);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Income record added successfully!";
                return RedirectToAction(nameof(Income));
            }
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", income.ChurchId);
            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateIncome(Income income)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    income.ModifiedBy = GetCurrentAdminId();
                    income.ModifiedDate = DateTime.Now;
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Income record updated successfully!";
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.IncomeId))
                        return Json(new { success = false, message = "Income record not found." });
                    throw;
                }
            }
            return Json(new { success = false, message = "Invalid data provided." });
        }

        public async Task<IActionResult> EditIncome(int id)
        {
            var income = await _context.Income.FindAsync(id);
            if (income == null)
                return NotFound();

            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", income.ChurchId);
            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIncome(int id, Income income)
        {
            if (id != income.IncomeId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    income.ModifiedBy = GetCurrentAdminId();
                    income.ModifiedDate = DateTime.Now;
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Income record updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.IncomeId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Income));
            }
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", income.ChurchId);
            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIncome(int id)
        {
            var income = await _context.Income.FindAsync(id);
            if (income != null)
            {
                _context.Income.Remove(income);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Income record deleted successfully!";
            }
            return RedirectToAction(nameof(Income));
        }

        // Expense Actions
        public async Task<IActionResult> Expense(int? churchId, int? month, int? year)
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", churchId);
            ViewBag.SelectedChurch = churchId;
            ViewBag.SelectedMonth = month ?? DateTime.Now.Month;
            ViewBag.SelectedYear = year ?? DateTime.Now.Year;

            var query = _context.Expenses.Include(e => e.Church).AsQueryable();

            if (churchId.HasValue)
                query = query.Where(e => e.ChurchId == churchId.Value);

            if (month.HasValue)
                query = query.Where(e => e.ExpenseDate.Month == month.Value);

            if (year.HasValue)
                query = query.Where(e => e.ExpenseDate.Year == year.Value);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return View(expenses);
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenseForEdit(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            return Json(new
            {
                success = true,
                expense = new
                {
                    expenseId = expense.ExpenseId,
                    churchId = expense.ChurchId,
                    expenseDate = expense.ExpenseDate.ToString("yyyy-MM-dd"),
                    category = expense.Category,
                    amount = expense.Amount,
                    description = expense.Description
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExpense(Expense expense)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    expense.ModifiedBy = GetCurrentAdminId();
                    expense.ModifiedDate = DateTime.Now;
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Expense record updated successfully!";
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.ExpenseId))
                        return Json(new { success = false, message = "Expense record not found." });
                    throw;
                }
            }
            return Json(new { success = false, message = "Invalid data provided." });
        }

        public async Task<IActionResult> CreateExpense()
        {
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.AddedBy = GetCurrentAdminId();
                expense.AddedDate = DateTime.Now;
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense record added successfully!";
                return RedirectToAction(nameof(Expense));
            }
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", expense.ChurchId);
            return View(expense);
        }

        public async Task<IActionResult> EditExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", expense.ChurchId);
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExpense(int id, Expense expense)
        {
            if (id != expense.ExpenseId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    expense.ModifiedBy = GetCurrentAdminId();
                    expense.ModifiedDate = DateTime.Now;
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Expense record updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.ExpenseId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Expense));
            }
            ViewBag.Churches = new SelectList(await _context.Churches.Where(c => c.Status == "Active").ToListAsync(), "ChurchId", "ChurchName", expense.ChurchId);
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense record deleted successfully!";
            }
            return RedirectToAction(nameof(Expense));
        }

        private bool IncomeExists(int id)
        {
            return _context.Income.Any(e => e.IncomeId == id);
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.ExpenseId == id);
        }

        private int GetCurrentAdminId()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(adminId, out var id) ? id : 0;
        }
    }
}
