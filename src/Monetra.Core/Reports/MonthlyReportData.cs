namespace Monetra.Core.Reports;

public class MonthlyReportData
{
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public List<CategoryReportItem> CategoryBreakdown { get; set; } = new();
    public List<TransactionReportItem> Transactions { get; set; } = new();
}

public class CategoryReportItem
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TransactionReportItem
{
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
