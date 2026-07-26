using System.Transactions;
using Monetra.Core.Enums;
using Monetra.Core.Events;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class BankAccount : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    // Dados da conta
    public string Name { get; private set; } = null!;
    public AccountType AccountType { get; private set; }
    public string? BankName { get; private set; }
    public string? BankCode { get; private set; }
    public string? Agency { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? AccountDigit { get; private set; }

    // Saldo
    public decimal Balance { get; private set; }
    public decimal InitialBalance { get; private set; }
    public DateOnly? BalanceDate { get; private set; }

    // Aparência
    public string Color { get; private set; } = null!;
    public string Icon { get; private set; } = null!;

    // Status
    public bool IsActive { get; private set; }
    public bool IsArchived { get; private set; }
    public bool IncludeInTotals { get; private set; }
    public int DisplayOrder { get; private set; }

    // Histórico de saldo
    private readonly List<BankAccountBalance> _balanceHistory = new();
    public IReadOnlyCollection<BankAccountBalance> BalanceHistory => _balanceHistory.AsReadOnly();

    // Transações
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private BankAccount() { }

    private BankAccount(
        Guid userId,
        string name,
        AccountType accountType,
        decimal initialBalance,
        DateOnly? balanceDate,
        string? bankName)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        AccountType = accountType;
        InitialBalance = initialBalance;
        Balance = initialBalance;
        BalanceDate = balanceDate;
        BankName = bankName;
        Color = "#10B981";
        Icon = "account_balance";
        IsActive = true;
        IsArchived = false;
        IncludeInTotals = true;
        DisplayOrder = 0;
    }

    /// <summary>
    /// Cria uma nova conta bancária.
    /// </summary>
    public static BankAccount Create(
        Guid userId,
        string name,
        string accountType,
        decimal initialBalance = 0,
        DateOnly? balanceDate = null,
        string? bankName = null,
        string? bankCode = null,
        string? agency = null,
        string? accountNumber = null,
        string? accountDigit = null,
        string? color = null,
        string? icon = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da conta é obrigatório.");

        if (!Enum.TryParse<AccountType>(accountType, true, out var type))
            throw new DomainException($"Tipo de conta inválido: {accountType}");

        if (initialBalance < 0 && type != AccountType.CreditCard)
            throw new DomainException("Saldo inicial não pode ser negativo para este tipo de conta.");

        var account = new BankAccount(userId, name.Trim(), type, initialBalance, balanceDate, bankName)
        {
            BankCode = bankCode,
            Agency = agency,
            AccountNumber = accountNumber,
            AccountDigit = accountDigit,
            Color = color ?? "#10B981",
            Icon = icon ?? "account_balance"
        };

        // Registrar saldo inicial no histórico
        account.AddBalanceHistory(initialBalance, balanceDate ?? DateOnly.FromDateTime(DateTime.UtcNow));

        return account;
    }

    /// <summary>
    /// Atualiza informações da conta.
    /// </summary>
    public void Update(
        string name,
        AccountType accountType,
        string? bankName = null,
        string? bankCode = null,
        string? agency = null,
        string? accountNumber = null,
        string? accountDigit = null,
        string? color = null,
        string? icon = null)
    {
        Name = name.Trim();
        AccountType = accountType;
        BankName = bankName;
        BankCode = bankCode;
        Agency = agency;
        AccountNumber = accountNumber;
        AccountDigit = accountDigit;
        Color = color ?? Color;
        Icon = icon ?? Icon;
        SetUpdatedAt();
    }

    /// <summary>
    /// Adiciona valor ao saldo (receita).
    /// </summary>
    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Valor de crédito deve ser maior que zero.");

        var oldBalance = Balance;
        Balance += amount;

        AddDomainEvent(new AccountBalanceChangedEvent(Id, UserId, oldBalance, Balance, amount, null, DateTime.UtcNow));
        SetUpdatedAt();
    }

    /// <summary>
    /// Subtrai valor do saldo (despesa).
    /// </summary>
    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Valor de débito deve ser maior que zero.");

        if (amount > Balance && AccountType != AccountType.CreditCard)
            throw new InsufficientBalanceException(Balance, amount);

        var oldBalance = Balance;
        Balance -= amount;

        AddDomainEvent(new AccountBalanceChangedEvent(Id, UserId, oldBalance, Balance, -amount, null, DateTime.UtcNow));
        SetUpdatedAt();
    }

    /// <summary>
    /// Arquivar conta (não exclui transações).
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Reativar conta arquivada.
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Atualiza o saldo manualmente (conciliação).
    /// </summary>
    public void ReconcileBalance(decimal newBalance, DateOnly date)
    {
        var oldBalance = Balance;
        Balance = newBalance;

        AddBalanceHistory(newBalance, date);
        AddDomainEvent(new AccountBalanceChangedEvent(Id, UserId, oldBalance, newBalance, newBalance - oldBalance, null, DateTime.UtcNow));
        SetUpdatedAt();
    }

    /// <summary>
    /// Adiciona registro de histórico de saldo.
    /// </summary>
    private void AddBalanceHistory(decimal balance, DateOnly date)
    {
        _balanceHistory.Add(BankAccountBalance.Create(Id, balance, date));
    }
}
