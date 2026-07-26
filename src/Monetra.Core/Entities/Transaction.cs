using Monetra.Core.Enums;
using Monetra.Core.Events;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class Transaction : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid BankAccountId { get; private set; }
    public BankAccount BankAccount { get; private set; } = null!;

    public Guid? CategoryId { get; private set; }
    public TransactionCategory? Category { get; private set; }

    // Valores
    public decimal Amount { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public decimal? BalanceBefore { get; private set; }
    public decimal? BalanceAfter { get; private set; }

    // Datas
    public DateOnly TransactionDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateOnly? PaidDate { get; private set; }
    public DateOnly? CompetenceDate { get; private set; }

    // Descrição
    public string Description { get; private set; } = null!;
    public string? Notes { get; private set; }

    // Informações adicionais
    public PaymentMethod? PaymentMethod { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string? ReceiptUrl { get; private set; }

    // Status
    public TransactionStatus Status { get; private set; }
    public bool IsRecurring { get; private set; }
    public Guid? RecurrenceId { get; private set; }
    public RecurringTransaction? RecurringTransaction { get; private set; }

    // Conciliação
    public bool IsReconciled { get; private set; }
    public DateTime? ReconciledAt { get; private set; }

    // Tags
    public List<string> Tags { get; private set; } = new();

    // Soft delete
    public DateTime? DeletedAt { get; private set; }

    private Transaction() { }

    private Transaction(
        Guid userId,
        Guid bankAccountId,
        decimal amount,
        TransactionType transactionType,
        DateOnly transactionDate,
        string description)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        BankAccountId = bankAccountId;
        Amount = amount;
        TransactionType = transactionType;
        TransactionDate = transactionDate;
        Description = description;
        Status = TransactionStatus.Pending;
        IsRecurring = false;
        IsReconciled = false;
        Tags = new List<string>();
    }

    /// <summary>
    /// Cria uma nova transação financeira.
    /// </summary>
    public static Transaction Create(
        Guid userId,
        Guid bankAccountId,
        decimal amount,
        string transactionType,
        DateOnly transactionDate,
        string description,
        Guid? categoryId = null,
        DateOnly? dueDate = null,
        string? notes = null,
        string? paymentMethod = null,
        string? documentNumber = null,
        List<string>? tags = null)
    {
        if (amount <= 0)
            throw new DomainException("Valor da transação deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição é obrigatória.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId é obrigatório.");

        if (bankAccountId == Guid.Empty)
            throw new DomainException("BankAccountId é obrigatório.");

        if (!Enum.TryParse<TransactionType>(transactionType, true, out var type))
            throw new DomainException($"Tipo de transação inválido: {transactionType}");

        PaymentMethod? paymentMethodEnum = null;
        if (!string.IsNullOrWhiteSpace(paymentMethod))
        {
            if (!Enum.TryParse<PaymentMethod>(paymentMethod, true, out var pm))
                throw new DomainException($"Método de pagamento inválido: {paymentMethod}");
            paymentMethodEnum = pm;
        }

        var transaction = new Transaction(userId, bankAccountId, amount, type, transactionDate, description.Trim())
        {
            CategoryId = categoryId,
            DueDate = dueDate,
            Notes = notes,
            PaymentMethod = paymentMethodEnum,
            DocumentNumber = documentNumber,
            Tags = tags ?? new List<string>()
        };

        transaction.AddDomainEvent(new TransactionCreatedEvent(
            transaction.Id, userId, bankAccountId, amount, transactionType, DateTime.UtcNow));

        return transaction;
    }

    /// <summary>
    /// Atualiza dados da transação (apenas se não estiver conciliada).
    /// </summary>
    public void Update(
        decimal amount,
        DateOnly transactionDate,
        string description,
        Guid? categoryId = null,
        DateOnly? dueDate = null,
        string? notes = null,
        string? paymentMethod = null)
    {
        if (IsReconciled)
            throw new DomainException("Transação conciliada não pode ser editada.");

        if (amount <= 0)
            throw new DomainException("Valor da transação deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição é obrigatória.");

        Amount = amount;
        TransactionDate = transactionDate;
        Description = description.Trim();
        CategoryId = categoryId;
        DueDate = dueDate;
        Notes = notes;

        if (!string.IsNullOrWhiteSpace(paymentMethod) &&
            Enum.TryParse<PaymentMethod>(paymentMethod, true, out var pm))
        {
            PaymentMethod = pm;
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Marca transação como paga/recebida.
    /// </summary>
    public void Pay(DateOnly paidDate)
    {
        if (Status == TransactionStatus.Completed)
            throw new DomainException("Transação já está paga/recebida.");

        if (Status == TransactionStatus.Cancelled)
            throw new DomainException("Transação cancelada não pode ser paga.");

        if (Status == TransactionStatus.Reconciled)
            throw new DomainException("Transação conciliada não pode ser alterada.");

        // Validação: data de pagamento >= data de vencimento (se houver)
        if (DueDate.HasValue && paidDate < DueDate.Value)
            throw new DomainException("Data de pagamento não pode ser anterior à data de vencimento.");

        PaidDate = paidDate;
        Status = TransactionStatus.Completed;
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancela a transação.
    /// </summary>
    public void Cancel()
    {
        if (Status == TransactionStatus.Completed && IsReconciled)
            throw new DomainException("Transação conciliada não pode ser cancelada. Crie um estorno.");

        if (Status == TransactionStatus.Reconciled)
            throw new DomainException("Transação conciliada não pode ser cancelada. Crie um estorno.");

        Status = TransactionStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>
    /// Categoriza a transação.
    /// </summary>
    public void Categorize(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId é obrigatório.");

        CategoryId = categoryId;

        AddDomainEvent(new TransactionCategorizedEvent(Id, categoryId, DateTime.UtcNow));
        SetUpdatedAt();
    }

    /// <summary>
    /// Concilia a transação (confirma extrato bancário).
    /// </summary>
    public void Reconcile()
    {
        if (Status == TransactionStatus.Cancelled)
            throw new DomainException("Transação cancelada não pode ser conciliada.");

        IsReconciled = true;
        ReconciledAt = DateTime.UtcNow;
        Status = TransactionStatus.Reconciled;
        SetUpdatedAt();
    }

    /// <summary>
    /// Define saldos antes e depois (calculados pelo serviço).
    /// </summary>
    public void SetBalances(decimal balanceBefore, decimal balanceAfter)
    {
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
    }

    /// <summary>
    /// Adiciona tags à transação.
    /// </summary>
    public void AddTags(List<string> tags)
    {
        Tags.AddRange(tags.Where(t => !Tags.Contains(t)));
        SetUpdatedAt();
    }

    /// <summary>
    /// Remove tags da transação.
    /// </summary>
    public void RemoveTags(List<string> tags)
    {
        Tags.RemoveAll(t => tags.Contains(t));
        SetUpdatedAt();
    }

    /// <summary>
    /// Soft delete da transação.
    /// </summary>
    public void SoftDelete()
    {
        if (IsReconciled)
            throw new DomainException("Transação conciliada não pode ser excluída.");

        DeletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
