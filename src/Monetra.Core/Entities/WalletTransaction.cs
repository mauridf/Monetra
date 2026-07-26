using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class WalletTransaction : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    public Guid? TransactionId { get; private set; }
    public Transaction? Transaction { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public string Type { get; private set; } // contribution, withdrawal
    public string? Description { get; private set; }
    public decimal? BalanceBefore { get; private set; }
    public decimal? BalanceAfter { get; private set; }
    public DateOnly Date { get; private set; }

    private WalletTransaction() { }

    private WalletTransaction(
        Guid walletId,
        Guid userId,
        decimal amount,
        string type,
        string? description,
        decimal? balanceBefore,
        decimal? balanceAfter)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        UserId = userId;
        Amount = amount;
        Type = type;
        Description = description;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        Date = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    /// <summary>
    /// Cria movimentação de contribuição.
    /// </summary>
    public static WalletTransaction CreateContribution(
        Guid walletId,
        Guid userId,
        decimal amount,
        string? description,
        decimal balanceBefore,
        decimal balanceAfter)
    {
        return new WalletTransaction(walletId, userId, amount, "contribution", description, balanceBefore, balanceAfter);
    }

    /// <summary>
    /// Cria movimentação de retirada.
    /// </summary>
    public static WalletTransaction CreateWithdrawal(
        Guid walletId,
        Guid userId,
        decimal amount,
        string justification,
        decimal balanceBefore,
        decimal balanceAfter)
    {
        if (string.IsNullOrWhiteSpace(justification))
            throw new DomainException("Justificativa é obrigatória para retirada.");

        return new WalletTransaction(walletId, userId, amount, "withdrawal", justification, balanceBefore, balanceAfter);
    }
}
