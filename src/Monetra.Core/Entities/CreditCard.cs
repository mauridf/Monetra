using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class CreditCard : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public string Brand { get; private set; } = null!;
    public string? LastDigits { get; private set; }

    // Limite
    public decimal CreditLimit { get; private set; }
    public decimal AvailableLimit { get; private set; }

    // Fatura
    public int ClosingDay { get; private set; }
    public int DueDay { get; private set; }

    // Aparência
    public string Color { get; private set; } = null!;

    // Status
    public bool IsActive { get; private set; }
    public bool IsArchived { get; private set; }
    public int DisplayOrder { get; private set; }

    // Faturas
    private readonly List<Invoice> _invoices = new();
    public IReadOnlyCollection<Invoice> Invoices => _invoices.AsReadOnly();

    private CreditCard() { }

    private CreditCard(
        Guid userId,
        string name,
        string brand,
        decimal creditLimit,
        int closingDay,
        int dueDay)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Brand = brand;
        CreditLimit = creditLimit;
        AvailableLimit = creditLimit;
        ClosingDay = closingDay;
        DueDay = dueDay;
        Color = "#EF4444";
        IsActive = true;
        IsArchived = false;
        DisplayOrder = 0;
    }

    /// <summary>
    /// Cria um novo cartão de crédito.
    /// </summary>
    public static CreditCard Create(
        Guid userId,
        string name,
        string brand,
        decimal creditLimit,
        int closingDay,
        int dueDay,
        string? lastDigits = null,
        string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do cartão é obrigatório.");

        if (string.IsNullOrWhiteSpace(brand))
            throw new DomainException("Bandeira do cartão é obrigatória.");

        if (creditLimit <= 0)
            throw new DomainException("Limite de crédito deve ser maior que zero.");

        if (closingDay < 1 || closingDay > 31)
            throw new DomainException("Dia de fechamento inválido (1-31).");

        if (dueDay < 1 || dueDay > 31)
            throw new DomainException("Dia de vencimento inválido (1-31).");

        return new CreditCard(userId, name.Trim(), brand.Trim(), creditLimit, closingDay, dueDay)
        {
            LastDigits = lastDigits,
            Color = color ?? "#EF4444"
        };
    }

    /// <summary>
    /// Atualiza dados do cartão.
    /// </summary>
    public void Update(
        string name,
        string brand,
        decimal creditLimit,
        int closingDay,
        int dueDay,
        string? lastDigits = null,
        string? color = null)
    {
        Name = name.Trim();
        Brand = brand.Trim();
        CreditLimit = creditLimit;
        ClosingDay = closingDay;
        DueDay = dueDay;
        LastDigits = lastDigits;
        Color = color ?? Color;
        SetUpdatedAt();
    }

    /// <summary>
    /// Atualiza limite disponível (após gerar fatura).
    /// </summary>
    public void UpdateAvailableLimit(decimal newAvailableLimit)
    {
        AvailableLimit = newAvailableLimit;
        SetUpdatedAt();
    }

    /// <summary>
    /// Deduz do limite disponível.
    /// </summary>
    public void DeductFromLimit(decimal amount)
    {
        if (amount > AvailableLimit)
            throw new DomainException("Limite disponível insuficiente.");

        AvailableLimit -= amount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Restaura limite disponível (após pagamento).
    /// </summary>
    public void RestoreLimit(decimal amount)
    {
        AvailableLimit += amount;
        if (AvailableLimit > CreditLimit)
            AvailableLimit = CreditLimit;
        SetUpdatedAt();
    }
}
