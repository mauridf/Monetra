using Quartz;
using Serilog;
using Monetra.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Monetra.Scheduler.Jobs;

/// <summary>
/// Job que processa transações recorrentes programadas para o dia.
/// Executa diariamente às 06:00.
/// </summary>
[DisallowConcurrentExecution]
public class RecurringTransactionJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("🔄 Iniciando processamento de transações recorrentes...");

        try
        {
            using var scope = context.Scheduler?.Context
                .Get<IServiceScopeFactory>()?.CreateScope()
                ?? throw new InvalidOperationException("ServiceScopeFactory não disponível");

            var service = scope.ServiceProvider
                .GetRequiredService<RecurringTransactionService>();

            var count = await service.ProcessDueRecurrencesAsync();

            Log.Information("✅ {Count} transações recorrentes processadas com sucesso", count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao processar transações recorrentes");
            throw new JobExecutionException(ex, false); // Refire on failure
        }
    }
}
