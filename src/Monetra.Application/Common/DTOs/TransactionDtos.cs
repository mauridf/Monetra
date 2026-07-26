namespace Monetra.Application.Common.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CategoryName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? AccountName { get; set; }
    public Guid BankAccountId { get; set; }
    public bool IsReconciled { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class BankAccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public decimal Balance { get; set; }
    public string Color { get; set; } = "#10B981";
    public string Icon { get; set; } = "account_balance";
    public bool IsActive { get; set; }
    public bool IncludeInTotals { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "category";
    public string Color { get; set; } = "#6B7280";
    public string TransactionType { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int Level { get; set; }
    public List<CategoryDto> Children { get; set; } = new();
}

public class WalletDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = "savings";
    public string Color { get; set; } = "#F59E0B";
    public decimal ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BalanceBefore { get; set; }
    public decimal? BalanceAfter { get; set; }
    public DateOnly Date { get; set; }
}
