using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class BudgetCategory : Entity<Guid>
{
    public Guid BudgetId { get; private set; }
    public Budget Budget { get; private set; } = null!;

    public Guid CategoryId { get; private set; }
    public TransactionCategory Category { get; private set; } = null!;

    public decimal LimitAmount { get; private set; }
    public decimal SpentAmount { get; private set; }

    private BudgetCategory() { }

    private BudgetCategory(Guid budgetId, Guid categoryId, decimal limitAmount)
    {
        Id = Guid.NewGuid();
        BudgetId = budgetId;
        CategoryId = categoryId;
        LimitAmount = limitAmount;
        SpentAmount = 0;
    }

    /// <summary>
    /// Cria uma categoria de orçamento.
    /// </summary>
    public static BudgetCategory Create(Guid budgetId, Guid categoryId, decimal limitAmount)
    {
        if (limitAmount <= 0)
            throw new DomainException("Limite da categoria deve ser maior que zero.");

        return new BudgetCategory(budgetId, categoryId, limitAmount);
    }

    /// <summary>
    /// Adiciona gasto à categoria.
    /// </summary>
    public void AddSpending(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Valor do gasto deve ser maior que zero.");

        SpentAmount += amount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Calcula o percentual de gasto da categoria.
    /// </summary>
    public decimal GetSpentPercentage()
    {
        if (LimitAmount == 0) return 0;
        return Math.Round((SpentAmount / LimitAmount) * 100, 2);
    }

    /// <summary>
    /// Verifica se estourou o limite.
    /// </summary>
    public bool IsOverLimit()
    {
        return SpentAmount > LimitAmount;
    }

    /// <summary>
    /// Verifica se está próximo do limite (80%+).
    /// </summary>
    public bool IsNearLimit()
    {
        return GetSpentPercentage() >= 80;
    }
}
