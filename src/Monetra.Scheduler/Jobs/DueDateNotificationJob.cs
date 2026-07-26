using Quartz;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Infrastructure.Data;

namespace Monetra.Scheduler.Jobs;

/// <summary>
/// Job que notifica usuários sobre contas a vencer.
/// Executa diariamente às 08:00.
/// </summary>
[DisallowConcurrentExecution]
public class DueDateNotificationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("🔔 Iniciando notificações de contas a vencer...");

        try
        {
            using var scope = context.Scheduler?.Context
                .Get<IServiceScopeFactory>()?.CreateScope()
                ?? throw new InvalidOperationException("ServiceScopeFactory não disponível");

            var dbContext = scope.ServiceProvider.GetRequiredService<MonetraDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Core.Interfaces.IUnitOfWork>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var notifyDate = today.AddDays(3); // Notificar 3 dias antes

            // Buscar transações que vencem nos próximos 3 dias
            var pendingTransactions = await dbContext.Transactions
                .Where(t => t.Status == TransactionStatus.Pending)
                .Where(t => t.DueDate.HasValue && t.DueDate.Value <= notifyDate)
                .Where(t => t.DueDate.HasValue && t.DueDate.Value >= today)
                .Where(t => t.DeletedAt == null)
                .ToListAsync();

            var notificationCount = 0;

            foreach (var transaction in pendingTransactions)
            {
                var daysUntilDue = (transaction.DueDate!.Value.DayNumber - today.DayNumber);

                var notification = Notification.Create(
                    transaction.UserId,
                    NotificationType.DueDate.ToString(),
                    "Conta a vencer",
                    $"A transação '{transaction.Description}' de {transaction.Amount:C} vence em {daysUntilDue} dia(s).",
                    null,
                    "transaction",
                    transaction.Id);

                await dbContext.Notifications.AddAsync(notification);
                notificationCount++;
            }

            // Verificar faturas próximas do vencimento
            var nearDueInvoices = await dbContext.Invoices
                .Where(i => i.Status == "closed" || i.Status == "open")
                .Where(i => i.DueDate <= notifyDate && i.DueDate >= today)
                .ToListAsync();

            foreach (var invoice in nearDueInvoices)
            {
                var daysUntilDue = (invoice.DueDate.DayNumber - today.DayNumber);

                var notification = Notification.Create(
                    invoice.UserId,
                    NotificationType.DueDate.ToString(),
                    "Fatura próxima do vencimento",
                    $"Fatura do cartão fecha em {daysUntilDue} dia(s). Valor: {invoice.TotalAmount:C}",
                    null,
                    "invoice",
                    invoice.Id);

                await dbContext.Notifications.AddAsync(notification);
                notificationCount++;
            }

            if (notificationCount > 0)
            {
                await unitOfWork.SaveChangesAsync();
            }

            Log.Information("✅ {Count} notificações de vencimento enviadas", notificationCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao enviar notificações de vencimento");
            throw new JobExecutionException(ex, false);
        }
    }
}
