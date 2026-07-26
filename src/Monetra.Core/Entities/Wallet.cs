using Monetra.Core.Enums;
using Monetra.Core.Events;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class Wallet : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public WalletType WalletType { get; private set; }
    public string Icon { get; private set; }
    public string Color { get; private set; }

    // Meta
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateOnly? TargetDate { get; private set; }

    // Status
    public WalletStatus Status { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Contribuição automática
    public bool AutoContribute { get; private set; }
    public decimal? AutoContributeAmount { get; private set; }
    public string? AutoContributeFrequency { get; private set; }
    public int? AutoContributeDay { get; private set; }

    public int DisplayOrder { get; private set; }

    // Movimentações
    private readonly List<WalletTransaction> _transactions = new();
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    private Wallet(Guid userId, string name, WalletType walletType, decimal targetAmount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        WalletType = walletType;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        Status = WalletStatus.Active;
        IsArchived = false;
        Icon = "savings";
        Color = "#F59E0B";
        DisplayOrder = 0;
    }

    /// <summary>
    /// Cria uma nova carteira (meta financeira).
    /// </summary>
    public static Wallet Create(
        Guid userId,
        string name,
        string walletType,
        decimal targetAmount,
        DateOnly? targetDate = null,
        string? description = null,
        string? icon = null,
        string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da carteira é obrigatório.");

        if (targetAmount <= 0)
            throw new DomainException("Valor da meta deve ser maior que zero.");

        if (!Enum.TryParse<WalletType>(walletType, true, out var type))
            throw new DomainException($"Tipo de carteira inválido: {walletType}");

        var wallet = new Wallet(userId, name.Trim(), type, targetAmount)
        {
            TargetDate = targetDate,
            Description = description,
            Icon = icon ?? "savings",
            Color = color ?? "#F59E0B"
        };

        return wallet;
    }

    /// <summary>
    /// Atualiza dados da carteira.
    /// </summary>
    public void Update(
        string name,
        decimal targetAmount,
        DateOnly? targetDate = null,
        string? description = null,
        string? icon = null,
        string? color = null)
    {
        if (Status == WalletStatus.Completed)
            throw new DomainException("Carteira concluída não pode ser editada.");

        Name = name.Trim();
        TargetAmount = targetAmount;
        TargetDate = targetDate;
        Description = description;
        Icon = icon ?? Icon;
        Color = color ?? Color;
        SetUpdatedAt();
    }

    /// <summary>
    /// Contribui para a carteira.
    /// </summary>
    public void Contribute(decimal amount, string? description = null)
    {
        if (Status == WalletStatus.Completed)
            throw new DomainException("Carteira já foi concluída.");

        if (Status == WalletStatus.Cancelled)
            throw new DomainException("Carteira cancelada não aceita contribuições.");

        if (amount <= 0)
            throw new DomainException("Valor da contribuição deve ser maior que zero.");

        if (amount < 10)
            throw new DomainException("Contribuição mínima é de R$ 10,00.");

        var newAmount = CurrentAmount + amount;
        if (newAmount > TargetAmount)
            throw new DomainException($"Contribuição excede o valor da meta. Meta: {TargetAmount:C}, Atual: {CurrentAmount:C}");

        var balanceBefore = CurrentAmount;
        CurrentAmount = newAmount;

        // Registrar movimentação
        _transactions.Add(WalletTransaction.CreateContribution(
            Id, UserId, amount, description, balanceBefore, CurrentAmount));

        AddDomainEvent(new WalletContributedEvent(Id, UserId, amount, CurrentAmount, DateTime.UtcNow));

        // Verificar se atingiu a meta
        if (CurrentAmount >= TargetAmount)
        {
            Complete();
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Retira valor da carteira.
    /// </summary>
    public void Withdraw(decimal amount, string justification)
    {
        if (Status == WalletStatus.Completed)
            throw new DomainException("Carteira concluída não permite retiradas.");

        if (amount <= 0)
            throw new DomainException("Valor da retirada deve ser maior que zero.");

        if (amount > CurrentAmount)
            throw new InsufficientBalanceException(CurrentAmount, amount);

        if (WalletType == WalletType.EmergencyFund && string.IsNullOrWhiteSpace(justification))
            throw new DomainException("Justificativa é obrigatória para retirada da Reserva de Emergência.");

        var balanceBefore = CurrentAmount;
        CurrentAmount -= amount;

        _transactions.Add(WalletTransaction.CreateWithdrawal(
            Id, UserId, amount, justification, balanceBefore, CurrentAmount));

        SetUpdatedAt();
    }

    /// <summary>
    /// Marca carteira como concluída.
    /// </summary>
    public void Complete()
    {
        if (Status == WalletStatus.Completed)
            throw new DomainException("Carteira já está concluída.");

        Status = WalletStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new WalletGoalReachedEvent(Id, UserId, Name, TargetAmount, DateTime.UtcNow));
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancela a carteira.
    /// </summary>
    public void Cancel()
    {
        if (Status == WalletStatus.Completed)
            throw new DomainException("Carteira concluída não pode ser cancelada.");

        Status = WalletStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>
    /// Arquiva a carteira.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Calcula o percentual de progresso.
    /// </summary>
    public decimal GetProgressPercentage()
    {
        if (TargetAmount == 0) return 0;
        return Math.Round((CurrentAmount / TargetAmount) * 100, 2);
    }

    /// <summary>
    /// Calcula valor mensal necessário para atingir a meta até a data alvo.
    /// </summary>
    public decimal CalculateMonthlyNeeded()
    {
        if (!TargetDate.HasValue) return 0;
        if (CurrentAmount >= TargetAmount) return 0;

        var remaining = TargetAmount - CurrentAmount;
        var monthsRemaining = ((TargetDate.Value.Year - DateTime.Now.Year) * 12)
                              + TargetDate.Value.Month - DateTime.Now.Month;

        if (monthsRemaining <= 0) return remaining;

        return Math.Round(remaining / monthsRemaining, 2);
    }
}
