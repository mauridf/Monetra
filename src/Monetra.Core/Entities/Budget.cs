using Monetra.Core.Enums;
using Monetra.Core.Events;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class Budget : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public BudgetPeriod Period { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    public decimal TotalLimit { get; private set; }
    public decimal TotalSpent { get; private set; }

    public string Status { get; private set; } = null!; // draft, active, completed, cancelled
    public bool IsTemplate { get; private set; }

    // Categorias do orçamento
    private readonly List<BudgetCategory> _categories = new();
    public IReadOnlyCollection<BudgetCategory> Categories => _categories.AsReadOnly();

    private Budget() { }

    private Budget(
        Guid userId,
        string name,
        BudgetPeriod period,
        DateOnly startDate,
        DateOnly endDate,
        decimal totalLimit)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Period = period;
        StartDate = startDate;
        EndDate = endDate;
        TotalLimit = totalLimit;
        TotalSpent = 0;
        Status = "draft";
        IsTemplate = false;
    }

    /// <summary>
    /// Cria um novo orçamento.
    /// </summary>
    public static Budget Create(
        Guid userId,
        string name,
        string period,
        DateOnly startDate,
        DateOnly endDate,
        decimal totalLimit,
        bool isTemplate = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do orçamento é obrigatório.");

        if (totalLimit <= 0)
            throw new DomainException("Limite total deve ser maior que zero.");

        if (endDate <= startDate)
            throw new DomainException("Data final deve ser posterior à data inicial.");

        if (!Enum.TryParse<BudgetPeriod>(period, true, out var budgetPeriod))
            throw new DomainException($"Período inválido: {period}");

        return new Budget(userId, name.Trim(), budgetPeriod, startDate, endDate, totalLimit)
        {
            IsTemplate = isTemplate
        };
    }

    /// <summary>
    /// Atualiza dados do orçamento.
    /// </summary>
    public void Update(
        string name,
        decimal totalLimit,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        if (Status == "completed" || Status == "cancelled")
            throw new DomainException($"Orçamento com status '{Status}' não pode ser editado.");

        Name = name.Trim();
        TotalLimit = totalLimit;

        if (startDate.HasValue)
            StartDate = startDate.Value;

        if (endDate.HasValue)
            EndDate = endDate.Value;

        SetUpdatedAt();
    }

    /// <summary>
    /// Adiciona uma categoria ao orçamento.
    /// </summary>
    public BudgetCategory AddCategory(Guid categoryId, decimal limitAmount)
    {
        if (Status == "completed" || Status == "cancelled")
            throw new DomainException($"Orçamento com status '{Status}' não aceita novas categorias.");

        // Verificar se categoria já existe no orçamento
        if (_categories.Any(c => c.CategoryId == categoryId))
            throw new DomainException("Categoria já existe neste orçamento.");

        var budgetCategory = BudgetCategory.Create(Id, categoryId, limitAmount);
        _categories.Add(budgetCategory);

        return budgetCategory;
    }

    /// <summary>
    /// Remove uma categoria do orçamento.
    /// </summary>
    public void RemoveCategory(Guid budgetCategoryId)
    {
        var category = _categories.FirstOrDefault(c => c.Id == budgetCategoryId)
            ?? throw new NotFoundException("Categoria do orçamento não encontrada.");

        _categories.Remove(category);
    }

    /// <summary>
    /// Registra um gasto em uma categoria.
    /// </summary>
    public void RegisterSpending(Guid categoryId, decimal amount)
    {
        var budgetCategory = _categories.FirstOrDefault(c => c.CategoryId == categoryId)
            ?? throw new NotFoundException("Categoria não encontrada no orçamento.");

        budgetCategory.AddSpending(amount);
        TotalSpent += amount;

        // Verificar alertas
        var percentageSpent = budgetCategory.LimitAmount > 0
            ? (budgetCategory.SpentAmount / budgetCategory.LimitAmount) * 100
            : 0;

        if (percentageSpent >= 100)
        {
            AddDomainEvent(new BudgetExceededEvent(
                Id, UserId, categoryId, $"Categoria {categoryId}",
                budgetCategory.LimitAmount, budgetCategory.SpentAmount,
                percentageSpent, DateTime.UtcNow));
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Ativa o orçamento.
    /// </summary>
    public void Activate()
    {
        if (Status != "draft")
            throw new DomainException("Apenas orçamentos em rascunho podem ser ativados.");

        Status = "active";
        SetUpdatedAt();
    }

    /// <summary>
    /// Completa o orçamento.
    /// </summary>
    public void Complete()
    {
        if (Status != "active")
            throw new DomainException("Apenas orçamentos ativos podem ser concluídos.");

        Status = "completed";
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancela o orçamento.
    /// </summary>
    public void Cancel()
    {
        if (Status == "completed")
            throw new DomainException("Orçamento concluído não pode ser cancelado.");

        Status = "cancelled";
        SetUpdatedAt();
    }

    /// <summary>
    /// Calcula o percentual de gasto total.
    /// </summary>
    public decimal GetSpentPercentage()
    {
        if (TotalLimit == 0) return 0;
        return Math.Round((TotalSpent / TotalLimit) * 100, 2);
    }
}
