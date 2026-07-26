using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monetra.Infrastructure.Data;
using Quartz;

namespace Monetra.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class CleanupJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(IServiceScopeFactory scopeFactory, ILogger<CleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Iniciando limpeza de dados antigos...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MonetraDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-90);

            dbContext.Notifications.RemoveRange(
                dbContext.Notifications.Where(n => n.IsRead && n.SentAt < cutoff));

            dbContext.ActivityLogs.RemoveRange(
                dbContext.ActivityLogs.Where(a => a.CreatedAt < cutoff));

            dbContext.OutboxMessages.RemoveRange(
                dbContext.OutboxMessages.Where(m => m.Status == "sent" && m.SentAt < cutoff));

            var deleted = await dbContext.SaveChangesAsync();
            _logger.LogInformation("{Count} registros antigos removidos", deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante limpeza de dados");
            throw new JobExecutionException(ex, false);
        }
    }
}
