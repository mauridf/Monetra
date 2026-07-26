using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class WalletsController : BaseController
{
    private readonly WalletRepository _walletRepo;
    private readonly BankAccountRepository _accountRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public WalletsController(
        WalletRepository walletRepo,
        BankAccountRepository accountRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _walletRepo = walletRepo;
        _accountRepo = accountRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista todas as carteiras do usuário.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWallets()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var wallets = await _walletRepo.GetActiveByUserAsync(userId);

        var dtos = wallets.Select(w => new WalletDto
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            WalletType = w.WalletType.ToString(),
            TargetAmount = w.TargetAmount,
            CurrentAmount = w.CurrentAmount,
            TargetDate = w.TargetDate,
            Status = w.Status.ToString(),
            Icon = w.Icon,
            Color = w.Color,
            ProgressPercentage = w.GetProgressPercentage(),
            CreatedAt = w.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Obtém progresso de todas as carteiras.
    /// </summary>
    [HttpGet("progress")]
    public async Task<IActionResult> GetWalletsProgress()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var progress = await _walletRepo.GetProgressAsync(userId);

        return Ok(progress.Select(p => new
        {
            WalletId = p.WalletId,
            Name = p.Name,
            ProgressPercentage = p.Progress
        }));
    }

    /// <summary>
    /// Obtém detalhes de uma carteira com movimentações.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWallet(Guid id)
    {
        var wallet = await _walletRepo.GetWithTransactionsAsync(id);
        if (wallet == null)
            return NotFound();

        return Ok(new WalletDto
        {
            Id = wallet.Id,
            Name = wallet.Name,
            Description = wallet.Description,
            WalletType = wallet.WalletType.ToString(),
            TargetAmount = wallet.TargetAmount,
            CurrentAmount = wallet.CurrentAmount,
            TargetDate = wallet.TargetDate,
            Status = wallet.Status.ToString(),
            Icon = wallet.Icon,
            Color = wallet.Color,
            ProgressPercentage = wallet.GetProgressPercentage(),
            CreatedAt = wallet.CreatedAt
        });
    }

    /// <summary>
    /// Cria uma nova carteira (meta financeira).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var wallet = Wallet.Create(
            userId,
            request.Name,
            request.WalletType,
            request.TargetAmount,
            request.TargetDate.HasValue ? DateOnly.FromDateTime(request.TargetDate.Value) : null,
            request.Description,
            request.Icon,
            request.Color);

        await _walletRepo.AddAsync(wallet);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetWallet), new { id = wallet.Id }, new WalletDto
        {
            Id = wallet.Id,
            Name = wallet.Name,
            WalletType = wallet.WalletType.ToString(),
            TargetAmount = wallet.TargetAmount,
            CurrentAmount = wallet.CurrentAmount,
            ProgressPercentage = 0,
            Status = wallet.Status.ToString(),
            Icon = wallet.Icon,
            Color = wallet.Color,
            CreatedAt = wallet.CreatedAt
        });
    }

    /// <summary>
    /// Contribui para uma carteira.
    /// </summary>
    [HttpPost("{id:guid}/contribute")]
    public async Task<IActionResult> ContributeToWallet(Guid id, [FromBody] ContributeToWalletRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var wallet = await _walletRepo.GetByIdAsync(id)
            ?? throw new Core.Exceptions.NotFoundException("Carteira não encontrada.");

        // Verificar se há conta bancária para debitar
        if (request.BankAccountId.HasValue)
        {
            var account = await _accountRepo.GetByIdAsync(request.BankAccountId.Value);
            if (account == null || account.UserId != userId)
                throw new Core.Exceptions.NotFoundException("Conta bancária não encontrada.");

            account.Debit(request.Amount);
            _accountRepo.Update(account);
        }

        wallet.Contribute(request.Amount, request.Description);
        _walletRepo.Update(wallet);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new WalletDto
        {
            Id = wallet.Id,
            Name = wallet.Name,
            CurrentAmount = wallet.CurrentAmount,
            TargetAmount = wallet.TargetAmount,
            ProgressPercentage = wallet.GetProgressPercentage(),
            Status = wallet.Status.ToString()
        });
    }

    /// <summary>
    /// Retira valor de uma carteira.
    /// </summary>
    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> WithdrawFromWallet(Guid id, [FromBody] WithdrawFromWalletRequest request)
    {
        var wallet = await _walletRepo.GetByIdAsync(id)
            ?? throw new Core.Exceptions.NotFoundException("Carteira não encontrada.");

        wallet.Withdraw(request.Amount, request.Justification ?? "Retirada");
        _walletRepo.Update(wallet);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Atualiza uma carteira.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWallet(Guid id, [FromBody] UpdateWalletRequest request)
    {
        var wallet = await _walletRepo.GetByIdAsync(id)
            ?? throw new Core.Exceptions.NotFoundException("Carteira não encontrada.");

        wallet.Update(
            request.Name,
            request.TargetAmount,
            request.TargetDate.HasValue ? DateOnly.FromDateTime(request.TargetDate.Value) : null,
            request.Description,
            request.Icon,
            request.Color);

        _walletRepo.Update(wallet);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove uma carteira.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWallet(Guid id)
    {
        var wallet = await _walletRepo.GetByIdAsync(id)
            ?? throw new Core.Exceptions.NotFoundException("Carteira não encontrada.");

        _walletRepo.Remove(wallet);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateWalletRequest
{
    public string Name { get; set; } = string.Empty;
    public string WalletType { get; set; } = "goal";
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class UpdateWalletRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class ContributeToWalletRequest
{
    public decimal Amount { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? Description { get; set; }
}

public class WithdrawFromWalletRequest
{
    public decimal Amount { get; set; }
    public string? Justification { get; set; }
}
