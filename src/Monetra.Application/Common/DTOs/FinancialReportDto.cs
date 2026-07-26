namespace Monetra.Application.Common.DTOs;

/// <summary>
/// Dados para geração de relatório financeiro.
/// </summary>
public class MonthlyReportData
{
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public List<CategoryBreakdown> CategoryBreakdown { get; set; } = new();
    public List<TransactionSummary> Transactions { get; set; } = new();
}

public class CategoryBreakdown
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6B7280";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TransactionSummary
{
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class AnnualReportData
{
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public List<MonthlySummary> Months { get; set; } = new();
}

public class MonthlySummary
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
}
