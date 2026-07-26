using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Core.Interfaces;
using Monetra.Core.Reports;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class ReportsController : BaseController
{
    private readonly TransactionRepository _transactionRepo;
    private readonly IReportGeneratorService _reportGenerator;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(
        TransactionRepository transactionRepo,
        IReportGeneratorService reportGenerator,
        ICurrentUserService currentUser)
    {
        _transactionRepo = transactionRepo;
        _reportGenerator = reportGenerator;
        _currentUser = currentUser;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var (income, expense) = await _transactionRepo.GetMonthlyTotalsAsync(userId, year, month);
        var balance = income - expense;

        return Ok(new MonthlyReportData
        {
            MonthName = new DateTime(year, month, 1).ToString("MMMM"),
            Year = year,
            TotalIncome = income,
            TotalExpense = expense,
            Balance = balance
        });
    }

    [HttpGet("annual")]
    public async Task<IActionResult> GetAnnualReport([FromQuery] int year)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var months = new List<MonthlyReport>();

        for (int m = 1; m <= 12; m++)
        {
            var (income, expense) = await _transactionRepo.GetMonthlyTotalsAsync(userId, year, m);
            months.Add(new MonthlyReport
            {
                Month = new DateTime(year, m, 1).ToString("MMMM"),
                Income = income,
                Expense = expense,
                Balance = income - expense
            });
        }

        return Ok(new AnnualReport
        {
            Year = year,
            TotalIncome = months.Sum(x => x.Income),
            TotalExpense = months.Sum(x => x.Expense),
            Balance = months.Sum(x => x.Balance),
            Months = months
        });
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var (income, expense) = await _transactionRepo.GetMonthlyTotalsAsync(userId, year, month);
        var balance = income - expense;

        var data = new MonthlyReportData
        {
            MonthName = new DateTime(year, month, 1).ToString("MMMM"),
            Year = year,
            TotalIncome = income,
            TotalExpense = expense,
            Balance = balance
        };

        var pdfBytes = await _reportGenerator.GenerateMonthlyReportAsync(data);
        return File(pdfBytes, "application/pdf", $"relatorio-{month}-{year}.pdf");
    }
}

public class MonthlyReport
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
}

public class AnnualReport
{
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public List<MonthlyReport> Months { get; set; } = new();
}
