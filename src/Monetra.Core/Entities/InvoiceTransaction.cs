using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class InvoiceTransaction : Entity<Guid>
{
    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public Guid? CategoryId { get; private set; }
    public TransactionCategory? Category { get; private set; }

    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PurchaseDate { get; private set; }

    // Parcelamento
    public int Installments { get; private set; }
    public int InstallmentNumber { get; private set; }
    public decimal? InstallmentTotal { get; private set; }

    public string? MerchantName { get; private set; }

    private InvoiceTransaction() { }

    private InvoiceTransaction(
        Guid invoiceId,
        string description,
        decimal amount,
        DateOnly purchaseDate)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Description = description;
        Amount = amount;
        PurchaseDate = purchaseDate;
        Installments = 1;
        InstallmentNumber = 1;
    }

    /// <summary>
    /// Cria uma nova transação na fatura.
    /// </summary>
    public static InvoiceTransaction Create(
        Guid invoiceId,
        string description,
        decimal amount,
        DateOnly purchaseDate,
        Guid? categoryId = null,
        int installments = 1,
        int installmentNumber = 1,
        decimal? installmentTotal = null,
        string? merchantName = null)
    {
        if (installments < 1)
            throw new DomainException("Número de parcelas deve ser pelo menos 1.");

        if (installmentNumber < 1 || installmentNumber > installments)
            throw new DomainException("Número da parcela inválido.");

        return new InvoiceTransaction(invoiceId, description.Trim(), amount, purchaseDate)
        {
            CategoryId = categoryId,
            Installments = installments,
            InstallmentNumber = installmentNumber,
            InstallmentTotal = installmentTotal ?? amount,
            MerchantName = merchantName
        };
    }
}
