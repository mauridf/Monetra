using System.Transactions;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class TransactionCategory : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Icon { get; private set; }
    public string Color { get; private set; }
    public TransactionType TransactionType { get; private set; }

    // Hierarquia
    public Guid? ParentId { get; private set; }
    public TransactionCategory? Parent { get; private set; }
    public int Level { get; private set; }

    private readonly List<TransactionCategory> _children = new();
    public IReadOnlyCollection<TransactionCategory> Children => _children.AsReadOnly();

    // Orçamento
    public decimal? MonthlyBudgetLimit { get; private set; }

    // Status
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    // Transações
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private TransactionCategory() { }

    private TransactionCategory(
        string name,
        TransactionType transactionType,
        Guid? userId = null,
        Guid? parentId = null,
        bool isSystem = false)
    {
        Id = Guid.NewGuid();
        Name = name;
        TransactionType = transactionType;
        UserId = userId;
        ParentId = parentId;
        IsSystem = isSystem;
        IsActive = true;
        Icon = "category";
        Color = "#6B7280";
        DisplayOrder = 0;
        Level = 0;
    }

    /// <summary>
    /// Cria uma nova categoria (sistema ou usuário).
    /// </summary>
    public static TransactionCategory Create(
        string name,
        string transactionType,
        Guid? userId = null,
        Guid? parentId = null,
        bool isSystem = false,
        string? description = null,
        string? icon = null,
        string? color = null,
        decimal? monthlyBudgetLimit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria é obrigatório.");

        if (!Enum.TryParse<TransactionType>(transactionType, true, out var type))
            throw new DomainException($"Tipo de transação inválido: {transactionType}");

        var category = new TransactionCategory(name.Trim(), type, userId, parentId, isSystem)
        {
            Description = description,
            Icon = icon ?? "category",
            Color = color ?? "#6B7280",
            MonthlyBudgetLimit = monthlyBudgetLimit
        };

        return category;
    }

    /// <summary>
    /// Atualiza informações da categoria.
    /// </summary>
    public void Update(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        decimal? monthlyBudgetLimit = null)
    {
        if (IsSystem)
            throw new DomainException("Categorias do sistema não podem ser editadas.");

        Name = name.Trim();
        Description = description;
        Icon = icon ?? Icon;
        Color = color ?? Color;
        MonthlyBudgetLimit = monthlyBudgetLimit;
        SetUpdatedAt();
    }

    /// <summary>
    /// Define a hierarquia da categoria.
    /// </summary>
    public void SetParent(TransactionCategory? parent)
    {
        if (parent != null && parent.Id == Id)
            throw new DomainException("Uma categoria não pode ser pai dela mesma.");

        if (parent != null && parent.Level >= 2)
            throw new DomainException("Hierarquia máxima de 3 níveis atingida.");

        ParentId = parent?.Id;
        Parent = parent;
        Level = parent != null ? parent.Level + 1 : 0;
        SetUpdatedAt();
    }
}
