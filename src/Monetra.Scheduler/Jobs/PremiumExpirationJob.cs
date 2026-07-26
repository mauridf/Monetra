using Quartz;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Scheduler.Jobs;

/// <summary>
/// Job que verifica e processa expiração de planos premium.
/// Executa diariamente às 04:00.
/// </summary>
[DisallowConcurrentExecution]
public class PremiumExpirationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("⭐ Verificando expiração de planos premium...");

        try
        {
            using var scope = context.Scheduler?.Context
                .Get<IServiceScopeFactory>()?.CreateScope()
                ?? throw new InvalidOperationException("ServiceScopeFactory não disponível");

            var userRepo = scope.ServiceProvider.GetRequiredService<UserRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Core.Interfaces.IUnitOfWork>();

            var expiredUsers = await userRepo.GetExpiredPremiumUsersAsync();

            var updatedCount = 0;

            foreach (var user in expiredUsers)
            {
                user.SetPremium(false, null);
                userRepo.Update(user);
                updatedCount++;

                Log.Information("Premium expirado para usuário {UserId} ({Email})",
                    user.Id, user.Email.Value);
            }

            if (updatedCount > 0)
            {
                await unitOfWork.SaveChangesAsync();
            }

            Log.Information("✅ {Count} usuários tiveram premium removido", updatedCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao processar expiração de premium");
            throw new JobExecutionException(ex, false);
        }
    }
}
