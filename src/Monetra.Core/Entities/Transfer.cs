using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class Transfer : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    // Origem e destino
    public Guid? FromAccountId { get; private set; }
    public BankAccount? FromAccount { get; private set; }

    public Guid? ToAccountId { get; private set; }
    public BankAccount? ToAccount { get; private set; }

    public Guid? ToWalletId { get; private set; }
    public Wallet? ToWallet { get; private set; }

    // Transações geradas
    public Guid? FromTransactionId { get; private set; }
    public Transaction? FromTransaction { get; private set; }

    public Guid? ToTransactionId { get; private set; }
    public Transaction? ToTransaction { get; private set; }

    // Valores
    public decimal Amount { get; private set; }
    public DateOnly TransferDate { get; private set; }
    public string? Description { get; private set; }
    public decimal Fee { get; private set; }
    public Guid? FeeAccountId { get; private set; }

    public string Status { get; private set; } = null!; // pending, completed, cancelled

    private Transfer() { }

    private Transfer(
        Guid userId,
        decimal amount,
        DateOnly transferDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        TransferDate = transferDate;
        Fee = 0;
        Status = "completed";
    }

    /// <summary>
    /// Cria transferência entre contas.
    /// </summary>
    public static Transfer CreateBetweenAccounts(
        Guid userId,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        DateOnly transferDate,
        string? description = null,
        decimal fee = 0)
    {
        if (fromAccountId == toAccountId)
            throw new DomainException("Não é possível transferir para a mesma conta.");

        if (amount <= 0)
            throw new DomainException("Valor da transferência deve ser maior que zero.");

        var transfer = new Transfer(userId, amount, transferDate)
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Description = description,
            Fee = fee
        };

        return transfer;
    }

    /// <summary>
    /// Cria transferência para carteira.
    /// </summary>
    public static Transfer CreateToWallet(
        Guid userId,
        Guid fromAccountId,
        Guid toWalletId,
        decimal amount,
        DateOnly transferDate,
        string? description = null)
    {
        if (amount <= 0)
            throw new DomainException("Valor da transferência deve ser maior que zero.");

        var transfer = new Transfer(userId, amount, transferDate)
        {
            FromAccountId = fromAccountId,
            ToWalletId = toWalletId,
            Description = description
        };

        return transfer;
    }

    /// <summary>
    /// Vincula as transações geradas pela transferência.
    /// </summary>
    public void LinkTransactions(Guid fromTransactionId, Guid toTransactionId)
    {
        FromTransactionId = fromTransactionId;
        ToTransactionId = toTransactionId;
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancela a transferência.
    /// </summary>
    public void Cancel()
    {
        if (Status == "cancelled")
            throw new DomainException("Transferência já está cancelada.");

        Status = "cancelled";
        SetUpdatedAt();
    }
}
