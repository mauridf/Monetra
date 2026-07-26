using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class RecurringTransactionsController : BaseController
{
    private readonly IRepository<RecurringTransaction> _recurringRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RecurringTransactionsController(
        IRepository<RecurringTransaction> recurringRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _recurringRepo = recurringRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecurringTransactions()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var items = await _recurringRepo.FindAsync(r => r.UserId == userId && r.IsActive);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecurringTransaction([FromBody] CreateRecurringRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var recurring = RecurringTransaction.Create(
            userId,
            request.BankAccountId,
            request.Description,
            request.Amount,
            request.TransactionType,
            request.RecurrenceType,
            DateOnly.FromDateTime(request.StartDate),
            request.CategoryId,
            dayOfMonth: request.DayOfMonth);

        await _recurringRepo.AddAsync(recurring);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetRecurringTransactions), new { id = recurring.Id }, recurring);
    }

    [HttpPatch("{id:guid}/pause")]
    public async Task<IActionResult> PauseRecurring(Guid id)
    {
        var recurring = await _recurringRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Transação recorrente não encontrada.");
        recurring.Pause();
        _recurringRepo.Update(recurring);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/resume")]
    public async Task<IActionResult> ResumeRecurring(Guid id)
    {
        var recurring = await _recurringRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Transação recorrente não encontrada.");
        recurring.Resume();
        _recurringRepo.Update(recurring);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRecurring(Guid id)
    {
        var recurring = await _recurringRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Transação recorrente não encontrada.");
        _recurringRepo.Remove(recurring);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateRecurringRequest
{
    public Guid BankAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "expense";
    public string RecurrenceType { get; set; } = "monthly";
    public DateTime StartDate { get; set; }
    public int? DayOfMonth { get; set; }
    public bool AutoCreate { get; set; } = true;
}
