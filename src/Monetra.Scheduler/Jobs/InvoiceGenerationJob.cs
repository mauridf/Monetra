using Quartz;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Scheduler.Jobs;

/// <summary>
/// Job que gera faturas de cartão de crédito no dia de fechamento.
/// Executa diariamente às 02:00.
/// </summary>
[DisallowConcurrentExecution]
public class InvoiceGenerationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("📄 Iniciando geração de faturas de cartão de crédito...");

        try
        {
            using var scope = context.Scheduler?.Context
                .Get<IServiceScopeFactory>()?.CreateScope()
                ?? throw new InvalidOperationException("ServiceScopeFactory não disponível");

            var dbContext = scope.ServiceProvider.GetRequiredService<MonetraDbContext>();
            var invoiceRepo = scope.ServiceProvider.GetRequiredService<InvoiceRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Core.Interfaces.IUnitOfWork>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentMonth = today.Month;
            var currentYear = today.Year;

            // Buscar cartões cujo dia de fechamento é hoje
            var cards = await dbContext.CreditCards
                .Where(c => c.IsActive && c.ClosingDay == today.Day)
                .ToListAsync();

            var generatedCount = 0;

            foreach (var card in cards)
            {
                // Verificar se já existe fatura para este período
                var exists = await invoiceRepo.ExistsForPeriodAsync(
                    card.Id, currentMonth, currentYear);

                if (exists)
                {
                    Log.Debug("Fatura já existe para cartão {CardId} ({CardName}) - {Month}/{Year}",
                        card.Id, card.Name, currentMonth, currentYear);
                    continue;
                }

                // Calcular datas de fechamento e vencimento
                var closingDate = today;
                var dueDate = CalculateDueDate(currentYear, currentMonth, card.DueDay);

                // Criar fatura
                var invoice = Invoice.Create(
                    card.Id,
                    card.UserId,
                    currentMonth,
                    currentYear,
                    closingDate,
                    dueDate);

                await invoiceRepo.AddAsync(invoice);
                generatedCount++;

                Log.Information("Fatura gerada para cartão {CardName}: {Month}/{Year}, Vencimento: {DueDate}",
                    card.Name, currentMonth, currentYear, dueDate);
            }

            if (generatedCount > 0)
            {
                await unitOfWork.SaveChangesAsync();
            }

            Log.Information("✅ {Count} faturas geradas com sucesso", generatedCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao gerar faturas de cartão de crédito");
            throw new JobExecutionException(ex, false);
        }
    }

    /// <summary>
    /// Calcula a data de vencimento baseado no mês de referência e dia de vencimento.
    /// </summary>
    private static DateOnly CalculateDueDate(int year, int month, int dueDay)
    {
        // Vencimento é no mês seguinte ao fechamento
        var dueMonth = month == 12 ? 1 : month + 1;
        var dueYear = month == 12 ? year + 1 : year;

        // Ajustar dia para último dia do mês se necessário
        var lastDayOfMonth = DateTime.DaysInMonth(dueYear, dueMonth);
        var actualDueDay = Math.Min(dueDay, lastDayOfMonth);

        return new DateOnly(dueYear, dueMonth, actualDueDay);
    }
}
