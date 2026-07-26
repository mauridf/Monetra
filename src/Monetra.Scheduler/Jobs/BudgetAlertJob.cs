using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;
using Quartz;

namespace Monetra.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class BudgetAlertJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BudgetAlertJob> _logger;

    public BudgetAlertJob(IServiceScopeFactory scopeFactory, ILogger<BudgetAlertJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Verificando alertas de orçamento...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var budgetRepo = scope.ServiceProvider.GetRequiredService<BudgetRepository>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var now = DateTime.UtcNow;

            var activeBudgets = await budgetRepo.FindAsync(
                b => b.Status == "active" && b.StartDate <= DateOnly.FromDateTime(now) && b.EndDate >= DateOnly.FromDateTime(now));

            foreach (var budget in activeBudgets)
            {
                foreach (var cat in budget.Categories)
                {
                    if (cat.IsOverLimit())
                    {
                        var notification = Notification.Create(
                            budget.UserId, "BudgetExceeded",
                            "Orçamento estourado", $"Categoria excedeu o limite em {cat.SpentAmount - cat.LimitAmount:C}",
                            null, "budget", budget.Id);
                        await notificationRepo.AddAsync(notification);
                    }
                    else if (cat.IsNearLimit())
                    {
                        var notification = Notification.Create(
                            budget.UserId, "BudgetExceeded",
                            "Orçamento próximo do limite", $"Categoria está a {100 - cat.GetSpentPercentage():F0}% do limite",
                            null, "budget", budget.Id);
                        await notificationRepo.AddAsync(notification);
                    }
                }
            }

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar orçamentos");
            throw new JobExecutionException(ex, false);
        }
    }
}
