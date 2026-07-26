using Monetra.Core.Events;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class Invoice : AggregateRoot<Guid>
{
    public Guid CreditCardId { get; private set; }
    public CreditCard CreditCard { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    // Período
    public int ReferenceMonth { get; private set; }
    public int ReferenceYear { get; private set; }

    // Datas
    public DateOnly ClosingDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateOnly? PaymentDate { get; private set; }

    // Valores
    public decimal TotalAmount { get; private set; }
    public decimal? MinimumPayment { get; private set; }
    public decimal PaidAmount { get; private set; }

    // Status
    public string Status { get; private set; } // open, closed, paid, overdue, cancelled
    public Guid? PaymentTransactionId { get; private set; }

    // Transações da fatura
    private readonly List<InvoiceTransaction> _transactions = new();
    public IReadOnlyCollection<InvoiceTransaction> Transactions => _transactions.AsReadOnly();

    private Invoice() { }

    private Invoice(
        Guid creditCardId,
        Guid userId,
        int referenceMonth,
        int referenceYear,
        DateOnly closingDate,
        DateOnly dueDate)
    {
        Id = Guid.NewGuid();
        CreditCardId = creditCardId;
        UserId = userId;
        ReferenceMonth = referenceMonth;
        ReferenceYear = referenceYear;
        ClosingDate = closingDate;
        DueDate = dueDate;
        TotalAmount = 0;
        PaidAmount = 0;
        Status = "open";
    }

    /// <summary>
    /// Cria uma nova fatura.
    /// </summary>
    public static Invoice Create(
        Guid creditCardId,
        Guid userId,
        int referenceMonth,
        int referenceYear,
        DateOnly closingDate,
        DateOnly dueDate)
    {
        if (referenceMonth < 1 || referenceMonth > 12)
            throw new DomainException("Mês de referência inválido (1-12).");

        return new Invoice(creditCardId, userId, referenceMonth, referenceYear, closingDate, dueDate);
    }

    /// <summary>
    /// Adiciona uma transação na fatura.
    /// </summary>
    public InvoiceTransaction AddTransaction(
        string description,
        decimal amount,
        DateOnly purchaseDate,
        Guid? categoryId = null,
        int installments = 1,
        int installmentNumber = 1,
        decimal? installmentTotal = null,
        string? merchantName = null)
    {
        if (Status == "paid" || Status == "cancelled")
            throw new DomainException($"Não é possível adicionar transação em fatura com status '{Status}'.");

        if (amount <= 0)
            throw new DomainException("Valor da transação deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição da transação é obrigatória.");

        var transaction = InvoiceTransaction.Create(
            Id, description, amount, purchaseDate, categoryId,
            installments, installmentNumber, installmentTotal, merchantName);

        _transactions.Add(transaction);
        TotalAmount += amount;
        SetUpdatedAt();

        return transaction;
    }

    /// <summary>
    /// Fecha a fatura.
    /// </summary>
    public void Close()
    {
        if (Status != "open")
            throw new DomainException($"Fatura com status '{Status}' não pode ser fechada.");

        Status = "closed";
        MinimumPayment = TotalAmount * 0.15m; // 15% do total
        SetUpdatedAt();
    }

    /// <summary>
    /// Paga a fatura.
    /// </summary>
    public void Pay(decimal amount, DateOnly paymentDate, Guid? transactionId = null)
    {
        if (Status == "paid")
            throw new DomainException("Fatura já está paga.");

        if (Status == "cancelled")
            throw new DomainException("Fatura cancelada não pode ser paga.");

        if (amount <= 0)
            throw new DomainException("Valor do pagamento deve ser maior que zero.");

        PaidAmount = amount;
        PaymentDate = paymentDate;
        PaymentTransactionId = transactionId;

        if (PaidAmount >= TotalAmount)
        {
            Status = "paid";
        }
        else
        {
            // Pagamento parcial
            Status = "closed";
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Verifica se fatura está vencida.
    /// </summary>
    public void CheckOverdue()
    {
        if (Status == "closed" && DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            Status = "overdue";
            SetUpdatedAt();

            AddDomainEvent(new InvoiceDueDateNearEvent(
                Id, UserId, CreditCardId, CreditCard?.Name ?? "Cartão",
                TotalAmount, DueDate, 0, DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Cancela a fatura.
    /// </summary>
    public void Cancel()
    {
        if (Status == "paid")
            throw new DomainException("Fatura paga não pode ser cancelada.");

        Status = "cancelled";
        SetUpdatedAt();
    }
}
