using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly BankAccountRepository _accountRepo;
    private readonly TransactionRepository _transactionRepo;
    private readonly WalletRepository _walletRepo;
    private readonly NotificationRepository _notificationRepo;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(
        BankAccountRepository accountRepo,
        TransactionRepository transactionRepo,
        WalletRepository walletRepo,
        NotificationRepository notificationRepo,
        ICurrentUserService currentUser)
    {
        _accountRepo = accountRepo;
        _transactionRepo = transactionRepo;
        _walletRepo = walletRepo;
        _notificationRepo = notificationRepo;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Obtém resumo geral do dashboard.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalBalance = await _accountRepo.GetTotalBalanceAsync(userId);
        var (income, expense) = await _transactionRepo.GetMonthlyTotalsAsync(
            userId, today.Year, today.Month);

        var pendingCount = (await _transactionRepo.GetPendingAsync(userId)).Count;
        var unreadCount = await _notificationRepo.GetUnreadCountAsync(userId);

        var wallets = await _walletRepo.GetActiveByUserAsync(userId);
        var walletProgress = wallets.Select(w => new WalletProgressDto
        {
            WalletId = w.Id,
            Name = w.Name,
            TargetAmount = w.TargetAmount,
            CurrentAmount = w.CurrentAmount,
            ProgressPercentage = w.GetProgressPercentage(),
            Color = w.Color,
            Icon = w.Icon
        }).ToList();

        return Ok(new DashboardSummaryDto
        {
            TotalBalance = totalBalance,
            MonthlyIncome = income,
            MonthlyExpense = expense,
            MonthlyBalance = income - expense,
            PendingTransactionsCount = pendingCount,
            UnreadNotificationsCount = unreadCount,
            WalletsProgress = walletProgress
        });
    }
}
