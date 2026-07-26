using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;
using Quartz;

namespace Monetra.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class PremiumExpirationJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PremiumExpirationJob> _logger;

    public PremiumExpirationJob(IServiceScopeFactory scopeFactory, ILogger<PremiumExpirationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Verificando expiração de premium...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expired = await userRepo.GetExpiredPremiumUsersAsync();
            foreach (var user in expired)
            {
                user.SetPremium(false, null);
                userRepo.Update(user);
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("{Count} usuários tiveram premium expirado", expired.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar expiração de premium");
            throw new JobExecutionException(ex, false);
        }
    }
}
