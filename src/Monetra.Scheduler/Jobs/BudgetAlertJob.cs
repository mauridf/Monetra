using Quartz;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Infrastructure.Data;

namespace Monetra.Scheduler.Jobs;

/// <summary>
/// Job que verifica orçamentos estourados e envia alertas.
/// Executa toda segunda-feira às 09:00.
/// </summary>
[DisallowConcurrentExecution]
public class BudgetAlertJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("📊 Verificando alertas de orçamento...");

        try
        {
            using var scope = context.Scheduler?.Context
                .Get<IServiceScopeFactory>()?.CreateScope()
                ?? throw new InvalidOperationException("ServiceScopeFactory não disponível");

            var dbContext = scope.ServiceProvider.GetRequiredService<MonetraDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Core.Interfaces.IUnitOfWork>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Buscar orçamentos ativos
            var activeBudgets = await dbContext.Budgets
                .Include(b => b.Categories)
                    .ThenInclude(bc => bc.Category)
                .Where(b => b.Status == "active")
                .Where(b => b.StartDate <= today && b.EndDate >= today)
                .ToListAsync();

            var alertCount = 0;

            foreach (var budget in activeBudgets)
            {
                foreach (var category in budget.Categories)
                {
                    if (category.IsNearLimit() || category.IsOverLimit())
                    {
                        var percentage = category.GetSpentPercentage();
                        var severity = percentage >= 100 ? "danger" : "warning";
                        var categoryName = category.Category?.Name ?? "Categoria";

                        var notification = Notification.Create(
                            budget.UserId,
                            NotificationType.BudgetExceeded.ToString(),
                            percentage >= 100
                                ? $"Orçamento estourado: {categoryName}"
                                : $"Orçamento próximo do limite: {categoryName}",
                            $"Você gastou {category.SpentAmount:C} de {category.LimitAmount:C} ({percentage:F1}%) " +
                            $"em {categoryName} no orçamento '{budget.Name}'.",
                            $"{{\"budgetId\":\"{budget.Id}\",\"categoryId\":\"{category.CategoryId}\",\"percentage\":{percentage}}}",
                            "budget",
                            budget.Id);

                        await dbContext.Notifications.AddAsync(notification);
                        alertCount++;

                        Log.Information("Alerta de orçamento para usuário {UserId}: {CategoryName} - {Percentage:F1}%",
                            budget.UserId, categoryName, percentage);
                    }
                }
            }

            if (alertCount > 0)
            {
                await unitOfWork.SaveChangesAsync();
            }

            Log.Information("✅ {Count} alertas de orçamento enviados", alertCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao verificar alertas de orçamento");
            throw new JobExecutionException(ex, false);
        }
    }
}
