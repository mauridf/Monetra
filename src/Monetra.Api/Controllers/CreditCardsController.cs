using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class CreditCardsController : BaseController
{
    private readonly CreditCardRepository _creditCardRepo;
    private readonly InvoiceRepository _invoiceRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreditCardsController(
        CreditCardRepository creditCardRepo,
        InvoiceRepository invoiceRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _creditCardRepo = creditCardRepo;
        _invoiceRepo = invoiceRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista cartões de crédito do usuário.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCreditCards()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var cards = await _creditCardRepo.GetActiveWithOpenInvoicesAsync(userId);

        return Ok(cards.Select(c => new
        {
            c.Id,
            c.Name,
            c.Brand,
            c.LastDigits,
            c.CreditLimit,
            c.AvailableLimit,
            c.ClosingDay,
            c.DueDay,
            c.Color,
            c.IsActive,
            OpenInvoicesCount = c.Invoices.Count(i => i.Status == "open" || i.Status == "closed"),
            c.CreatedAt
        }));
    }

    /// <summary>
    /// Cria um novo cartão de crédito.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCreditCard([FromBody] CreateCreditCardRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var card = CreditCard.Create(
            userId,
            request.Name,
            request.Brand,
            request.CreditLimit,
            request.ClosingDay,
            request.DueDay,
            request.LastDigits,
            request.Color);

        await _creditCardRepo.AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetCreditCards), new { id = card.Id }, new
        {
            card.Id,
            card.Name,
            card.Brand,
            card.CreditLimit,
            card.AvailableLimit,
            card.ClosingDay,
            card.DueDay
        });
    }

    /// <summary>
    /// Lista faturas de um cartão.
    /// </summary>
    [HttpGet("{id:guid}/invoices")]
    public async Task<IActionResult> GetInvoices(Guid id)
    {
        var invoices = await _invoiceRepo.FindAsync(i => i.CreditCardId == id);

        return Ok(invoices.Select(i => new
        {
            i.Id,
            i.ReferenceMonth,
            i.ReferenceYear,
            i.ClosingDate,
            i.DueDate,
            i.TotalAmount,
            i.PaidAmount,
            i.Status,
            i.PaymentDate
        }));
    }

    /// <summary>
    /// Obtém fatura atual (aberta) do cartão.
    /// </summary>
    [HttpGet("{id:guid}/invoices/current")]
    public async Task<IActionResult> GetCurrentInvoice(Guid id)
    {
        var invoices = await _invoiceRepo.FindAsync(i =>
            i.CreditCardId == id && (i.Status == "open" || i.Status == "closed"));

        var current = invoices.OrderByDescending(i => i.ReferenceYear)
                              .ThenByDescending(i => i.ReferenceMonth)
                              .FirstOrDefault();

        if (current == null)
            return NotFound(new { Message = "Nenhuma fatura aberta encontrada." });

        return Ok(new
        {
            current.Id,
            current.ReferenceMonth,
            current.ReferenceYear,
            current.ClosingDate,
            current.DueDate,
            current.TotalAmount,
            current.MinimumPayment,
            current.PaidAmount,
            current.Status
        });
    }

    /// <summary>
    /// Paga uma fatura.
    /// </summary>
    [HttpPost("{id:guid}/invoices/{invoiceId:guid}/pay")]
    public async Task<IActionResult> PayInvoice(Guid id, Guid invoiceId, [FromBody] PayInvoiceRequest request)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
            ?? throw new Core.Exceptions.NotFoundException("Fatura não encontrada.");

        invoice.Pay(request.Amount, DateOnly.FromDateTime(request.PaymentDate));
        _invoiceRepo.Update(invoice);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateCreditCardRequest
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = "visa";
    public decimal CreditLimit { get; set; }
    public int ClosingDay { get; set; }
    public int DueDay { get; set; }
    public string? LastDigits { get; set; }
    public string? Color { get; set; }
}

public class PayInvoiceRequest
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
