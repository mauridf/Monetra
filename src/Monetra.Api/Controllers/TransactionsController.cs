using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de transações financeiras.
/// </summary>
[Authorize]
public class TransactionsController : BaseController
{
    private readonly TransactionRepository _transactionRepo;
    private readonly BankAccountRepository _accountRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public TransactionsController(
        TransactionRepository transactionRepo,
        BankAccountRepository accountRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _transactionRepo = transactionRepo;
        _accountRepo = accountRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista transações com filtros e paginação.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? type,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? accountId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        TransactionType? transactionType = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransactionType>(type, true, out var parsedType))
            transactionType = parsedType;

        TransactionStatus? transactionStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TransactionStatus>(status, true, out var parsedStatus))
            transactionStatus = parsedStatus;

        var (items, total) = await _transactionRepo.GetFilteredAsync(
            userId,
            startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null,
            endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null,
            transactionType,
            categoryId,
            accountId,
            transactionStatus,
            search,
            page,
            perPage);

        var dtos = items.Select(t => new TransactionDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Type = t.TransactionType.ToString(),
            TransactionDate = t.TransactionDate,
            DueDate = t.DueDate,
            PaidDate = t.PaidDate,
            Description = t.Description,
            Notes = t.Notes,
            Status = t.Status.ToString(),
            PaymentMethod = t.PaymentMethod?.ToString(),
            CategoryName = t.Category?.Name,
            CategoryId = t.CategoryId,
            AccountName = t.BankAccount?.Name,
            BankAccountId = t.BankAccountId,
            IsReconciled = t.IsReconciled,
            Tags = t.Tags,
            CreatedAt = t.CreatedAt
        }).ToList();

        return OkPaginated(PaginatedResult<TransactionDto>.Create(dtos, total, page, perPage));
    }

    /// <summary>
    /// Obtém detalhes de uma transação.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var transaction = await _transactionRepo.GetByIdAsync(id);
        if (transaction == null || transaction.DeletedAt.HasValue)
            return NotFound();

        return Ok(new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.TransactionType.ToString(),
            TransactionDate = transaction.TransactionDate,
            DueDate = transaction.DueDate,
            PaidDate = transaction.PaidDate,
            Description = transaction.Description,
            Notes = transaction.Notes,
            Status = transaction.Status.ToString(),
            PaymentMethod = transaction.PaymentMethod?.ToString(),
            CategoryName = transaction.Category?.Name,
            CategoryId = transaction.CategoryId,
            AccountName = transaction.BankAccount?.Name,
            BankAccountId = transaction.BankAccountId,
            IsReconciled = transaction.IsReconciled,
            Tags = transaction.Tags,
            CreatedAt = transaction.CreatedAt
        });
    }

    /// <summary>
    /// Cria uma nova transação.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        // Buscar conta bancária
        var account = await _accountRepo.GetByIdAsync(request.BankAccountId)
            ?? throw new NotFoundException("Conta bancária não encontrada.");

        if (account.UserId != userId)
            throw new UnauthorizedException("Conta não pertence ao usuário.");

        // Criar transação
        var transaction = Transaction.Create(
            userId,
            request.BankAccountId,
            request.Amount,
            request.TransactionType,
            DateOnly.FromDateTime(request.TransactionDate),
            request.Description,
            request.CategoryId,
            request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : null,
            request.Notes,
            request.PaymentMethod,
            tags: request.Tags);

        // Atualizar saldo da conta
        var balanceBefore = account.Balance;

        if (request.TransactionType.Equals("income", StringComparison.OrdinalIgnoreCase))
        {
            account.Credit(request.Amount);
        }
        else if (request.TransactionType.Equals("expense", StringComparison.OrdinalIgnoreCase))
        {
            account.Debit(request.Amount);
        }

        transaction.SetBalances(balanceBefore, account.Balance);

        // Marcar como paga se não houver data de vencimento futura
        if (!request.DueDate.HasValue || request.DueDate.Value <= DateTime.UtcNow)
        {
            transaction.Pay(DateOnly.FromDateTime(request.PaidDate ?? DateTime.UtcNow));
        }

        await _transactionRepo.AddAsync(transaction);
        _accountRepo.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetTransaction), new { id = transaction.Id }, new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.TransactionType.ToString(),
            TransactionDate = transaction.TransactionDate,
            Description = transaction.Description,
            Status = transaction.Status.ToString(),
            AccountName = account.Name,
            BankAccountId = account.Id,
            CreatedAt = transaction.CreatedAt
        });
    }

    /// <summary>
    /// Atualiza uma transação.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        var transaction = await _transactionRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Transação não encontrada.");

        if (transaction.IsReconciled)
            throw new DomainException("Transação conciliada não pode ser editada.");

        transaction.Update(
            request.Amount,
            DateOnly.FromDateTime(request.TransactionDate),
            request.Description,
            request.CategoryId,
            request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : null,
            request.Notes,
            request.PaymentMethod);

        _transactionRepo.Update(transaction);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove uma transação (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var transaction = await _transactionRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Transação não encontrada.");

        transaction.SoftDelete();
        _transactionRepo.Update(transaction);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTransactionRequest
{
    public Guid BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "expense";
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateTransactionRequest
{
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
}
