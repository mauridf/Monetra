using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monetra.Application.Services;
using Quartz;

namespace Monetra.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class RecurringTransactionJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurringTransactionJob> _logger;

    public RecurringTransactionJob(IServiceScopeFactory scopeFactory, ILogger<RecurringTransactionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Iniciando processamento de transações recorrentes...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<RecurringTransactionService>();
            var count = await service.ProcessDueRecurrencesAsync();

            _logger.LogInformation("{Count} transações recorrentes processadas", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar transações recorrentes");
            throw new JobExecutionException(ex, false);
        }
    }
}
