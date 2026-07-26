using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de contas bancárias.
/// </summary>
[Authorize]
public class BankAccountsController : BaseController
{
    private readonly BankAccountRepository _accountRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public BankAccountsController(
        BankAccountRepository accountRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _accountRepo = accountRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista todas as contas bancárias do usuário.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var accounts = await _accountRepo.GetActiveByUserAsync(userId);

        var dtos = accounts.Select(a => new BankAccountDto
        {
            Id = a.Id,
            Name = a.Name,
            AccountType = a.AccountType.ToString(),
            BankName = a.BankName,
            Balance = a.Balance,
            Color = a.Color,
            Icon = a.Icon,
            IsActive = a.IsActive,
            IncludeInTotals = a.IncludeInTotals,
            CreatedAt = a.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Obtém resumo consolidado de todas as contas.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetAccountsSummary()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var totalBalance = await _accountRepo.GetTotalBalanceAsync(userId);
        var accounts = await _accountRepo.GetActiveByUserAsync(userId);

        return Ok(new
        {
            TotalBalance = totalBalance,
            AccountCount = accounts.Count,
            Accounts = accounts.Select(a => new BankAccountDto
            {
                Id = a.Id,
                Name = a.Name,
                AccountType = a.AccountType.ToString(),
                Balance = a.Balance,
                Color = a.Color
            })
        });
    }

    /// <summary>
    /// Obtém detalhes de uma conta específica.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var account = await _accountRepo.GetWithBalanceHistoryAsync(id);
        if (account == null)
            return NotFound();

        return Ok(new BankAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            AccountType = account.AccountType.ToString(),
            BankName = account.BankName,
            Balance = account.Balance,
            Color = account.Color,
            Icon = account.Icon,
            IsActive = account.IsActive,
            IncludeInTotals = account.IncludeInTotals,
            CreatedAt = account.CreatedAt
        });
    }

    /// <summary>
    /// Cria uma nova conta bancária.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateBankAccountRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var account = BankAccount.Create(
            userId,
            request.Name,
            request.AccountType,
            request.InitialBalance,
            request.BalanceDate != null ? DateOnly.FromDateTime(request.BalanceDate.Value) : null,
            request.BankName,
            request.BankCode,
            request.Agency,
            request.AccountNumber,
            request.AccountDigit,
            request.Color,
            request.Icon);

        await _accountRepo.AddAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetAccount), new { id = account.Id }, new BankAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            AccountType = account.AccountType.ToString(),
            BankName = account.BankName,
            Balance = account.Balance,
            Color = account.Color,
            Icon = account.Icon,
            IsActive = account.IsActive,
            IncludeInTotals = account.IncludeInTotals,
            CreatedAt = account.CreatedAt
        });
    }

    /// <summary>
    /// Atualiza uma conta bancária.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateBankAccountRequest request)
    {
        var account = await _accountRepo.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        account.Update(
            request.Name,
            Enum.Parse<AccountType>(request.AccountType, true),
            request.BankName,
            request.BankCode,
            request.Agency,
            request.AccountNumber,
            request.AccountDigit,
            request.Color,
            request.Icon);

        _accountRepo.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove uma conta bancária (soft delete se houver transações).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var account = await _accountRepo.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        // Se tiver transações, arquivar em vez de excluir
        if (account.Transactions.Count > 0)
        {
            account.Archive();
            _accountRepo.Update(account);
        }
        else
        {
            _accountRepo.Remove(account);
        }

        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Arquiva uma conta bancária.
    /// </summary>
    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveAccount(Guid id)
    {
        var account = await _accountRepo.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        account.Archive();
        _accountRepo.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs para BankAccount
public class CreateBankAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "checking";
    public decimal InitialBalance { get; set; } = 0;
    public DateTime? BalanceDate { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? Agency { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountDigit { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class UpdateBankAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "checking";
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? Agency { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountDigit { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
