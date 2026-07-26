using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;
using Quartz;

namespace Monetra.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class InvoiceGenerationJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InvoiceGenerationJob> _logger;

    public InvoiceGenerationJob(IServiceScopeFactory scopeFactory, ILogger<InvoiceGenerationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Gerando faturas de cartão de crédito...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var creditCardRepo = scope.ServiceProvider.GetRequiredService<CreditCardRepository>();
            var invoiceRepo = scope.ServiceProvider.GetRequiredService<InvoiceRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var activeCards = await creditCardRepo.GetActiveWithOpenInvoicesAsync(Guid.Empty);

            foreach (var card in activeCards.Where(c => c.ClosingDay == today.Day))
            {
                var exists = await invoiceRepo.ExistsForPeriodAsync(card.Id, today.Month, today.Year);
                if (exists) continue;

                var dueDate = today.AddMonths(1);
                dueDate = dueDate.Day != card.DueDay
                    ? dueDate.AddDays(card.DueDay - dueDate.Day)
                    : dueDate;

                var invoice = Invoice.Create(card.Id, card.UserId, today.Month, today.Year,
                    today, dueDate);

                await invoiceRepo.AddAsync(invoice);
                _logger.LogInformation("Fatura gerada: Cartão {CardId}, {Month}/{Year}", card.Id, today.Month, today.Year);
            }

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar faturas");
            throw new JobExecutionException(ex, false);
        }
    }
}
