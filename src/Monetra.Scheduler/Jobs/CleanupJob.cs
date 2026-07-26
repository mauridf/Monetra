using Quartz;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Infrastructure.Data;

namespace Monetra.Scheduler.Jobs;

/// <summary>
/// Job de limpeza de dados antigos e temporários.
/// Executa todo domingo às 03:00.
/// </summary>
[DisallowConcurrentExecution]
public class CleanupJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("🧹 Iniciando limpeza de dados...");

        try
        {
            using var scope = context.Scheduler?.Context
                .Get<IServiceScopeFactory>()?.CreateScope()
                ?? throw new InvalidOperationException("ServiceScopeFactory não disponível");

            var dbContext = scope.ServiceProvider.GetRequiredService<MonetraDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-90); // 90 dias
            var cleanedCount = 0;

            // Limpar notificações lidas com mais de 30 dias
            var oldNotifications = await dbContext.Notifications
                .Where(n => n.IsRead && n.ReadAt < cutoffDate)
                .Take(1000) // Limitar para evitar lock longo
                .ToListAsync();

            dbContext.Notifications.RemoveRange(oldNotifications);
            cleanedCount += oldNotifications.Count;

            // Limpar logs de atividade antigos (mais de 90 dias)
            var oldLogs = await dbContext.ActivityLogs
                .Where(l => l.CreatedAt < cutoffDate)
                .Take(1000)
                .ToListAsync();

            dbContext.ActivityLogs.RemoveRange(oldLogs);
            cleanedCount += oldLogs.Count;

            // Limpar mensagens de outbox processadas
            var processedOutbox = await dbContext.OutboxMessages
                .Where(m => m.Status == "sent" && m.ProcessedAt < cutoffDate)
                .Take(1000)
                .ToListAsync();

            dbContext.OutboxMessages.RemoveRange(processedOutbox);
            cleanedCount += processedOutbox.Count;

            await dbContext.SaveChangesAsync();

            Log.Information("✅ Limpeza concluída: {Count} registros removidos", cleanedCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao executar limpeza de dados");
            throw new JobExecutionException(ex, false);
        }
    }
}
