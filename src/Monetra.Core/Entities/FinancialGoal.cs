using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class FinancialGoal : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private FinancialGoal() { }

    private FinancialGoal(Guid userId, string name, decimal targetAmount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        IsCompleted = false;
    }

    /// <summary>
    /// Cria uma nova meta financeira.
    /// </summary>
    public static FinancialGoal Create(
        Guid userId,
        string name,
        decimal targetAmount,
        DateOnly? targetDate = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da meta é obrigatório.");

        if (targetAmount <= 0)
            throw new DomainException("Valor da meta deve ser maior que zero.");

        return new FinancialGoal(userId, name.Trim(), targetAmount)
        {
            TargetDate = targetDate,
            Description = description
        };
    }

    /// <summary>
    /// Atualiza progresso da meta.
    /// </summary>
    public void UpdateProgress(decimal currentAmount)
    {
        if (currentAmount < 0)
            throw new DomainException("Valor atual não pode ser negativo.");

        CurrentAmount = currentAmount;

        if (CurrentAmount >= TargetAmount && !IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }

        SetUpdatedAt();
    }
}
