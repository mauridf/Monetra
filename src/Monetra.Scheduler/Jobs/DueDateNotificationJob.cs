using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Interfaces;
using Quartz;

namespace Monetra.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class DueDateNotificationJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DueDateNotificationJob> _logger;

    public DueDateNotificationJob(IServiceScopeFactory scopeFactory, ILogger<DueDateNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Gerando notificações de contas a vencer...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var transactionRepo = scope.ServiceProvider.GetRequiredService<IRepository<Transaction>>();
            var invoiceRepo = scope.ServiceProvider.GetRequiredService<IRepository<Invoice>>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var threeDaysFromNow = today.AddDays(3);

            var dueTransactions = await transactionRepo.FindAsync(
                t => t.DueDate >= today && t.DueDate <= threeDaysFromNow && t.Status == TransactionStatus.Pending);

            foreach (var tx in dueTransactions)
            {
                var notification = Notification.Create(
                    tx.UserId, "DueDate",
                    "Conta a vencer", $"A transação '{tx.Description}' vence em {tx.DueDate}",
                    null, "transaction", tx.Id);
                await notificationRepo.AddAsync(notification);
            }

            var dueInvoices = await invoiceRepo.FindAsync(
                i => i.DueDate >= today && i.DueDate <= threeDaysFromNow && (i.Status == "open" || i.Status == "closed"));

            foreach (var inv in dueInvoices)
            {
                var notification = Notification.Create(
                    inv.UserId, "DueDate",
                    "Fatura a vencer", $"Fatura de cartão de crédito vence em {inv.DueDate}",
                    null, "invoice", inv.Id);
                await notificationRepo.AddAsync(notification);
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("{Count} notificações de vencimento geradas", dueTransactions.Count() + dueInvoices.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar notificações");
            throw new JobExecutionException(ex, false);
        }
    }
}
