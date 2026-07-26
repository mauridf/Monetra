namespace Monetra.Application.Common.DTOs;

/// <summary>
/// Resumo financeiro do dashboard.
/// </summary>
public class DashboardSummaryDto
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyBalance { get; set; }
    public int PendingTransactionsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
    public List<WalletProgressDto> WalletsProgress { get; set; } = new();
}

/// <summary>
/// Progresso de uma carteira.
/// </summary>
public class WalletProgressDto
{
    public Guid WalletId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string Color { get; set; } = "#F59E0B";
    public string Icon { get; set; } = "savings";
}

/// <summary>
/// Dados para gráfico de fluxo de caixa.
/// </summary>
public class CashFlowChartDto
{
    public List<CashFlowMonthDto> Months { get; set; } = new();
}

public class CashFlowMonthDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// Dados para gráfico de pizza por categoria.
/// </summary>
public class CategoryPieChartDto
{
    public List<CategorySliceDto> Categories { get; set; } = new();
}

public class CategorySliceDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = "#6B7280";
    public string Icon { get; set; } = "category";
}

/// <summary>
/// Transações próximas do vencimento.
/// </summary>
public class UpcomingTransactionDto
{
    public Guid TransactionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6B7280";
}

/// <summary>
/// Alerta do dashboard.
/// </summary>
public class DashboardAlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info"; // info, warning, danger, success
}
