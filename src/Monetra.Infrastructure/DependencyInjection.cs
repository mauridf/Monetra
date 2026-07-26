using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Data;
using Monetra.Infrastructure.Data.Interceptors;
using Monetra.Infrastructure.External;
using Monetra.Infrastructure.External.Cache;
using Monetra.Infrastructure.External.MessageBus;
using Monetra.Infrastructure.External.Storage;
using Monetra.Infrastructure.Outbox;
using Monetra.Infrastructure.Repositories;
using Monetra.Infrastructure.Services;
using StackExchange.Redis;

namespace Monetra.Infrastructure;

/// <summary>
/// Configuração de injeção de dependência da camada Infrastructure.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - PostgreSQL + EF Core
        services.AddDbContext<MonetraDbContext>(options =>
        {
            var connectionString = configuration["Database:ConnectionString"]
                ?? configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            });
        });

        // Interceptors
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<DomainEventInterceptor>();

        // Redis (opcional - com fallback)
        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                services.AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(redisConnectionString));

                services.AddScoped<ICacheService, RedisCacheService>();
            }
            catch
            {
                // Fallback para cache em memória
                services.AddScoped<ICacheService, MemoryCacheService>();
            }
        }
        else
        {
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        // Storage (MinIO com fallback local)
        var minioEndpoint = configuration["MinIo:Endpoint"];
        if (!string.IsNullOrEmpty(minioEndpoint))
        {
            services.AddScoped<IStorageService, MinioStorageService>();
        }
        else
        {
            services.AddScoped<IStorageService, LocalStorageService>();
        }

        // MessageBus (RabbitMQ com fallback em memória)
        var rabbitHost = configuration["RabbitMq:Host"];
        if (!string.IsNullOrEmpty(rabbitHost))
        {
            services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
        }
        else
        {
            services.AddSingleton<IMessageBus, InMemoryMessageBus>();
        }

        // Email / Notificações
        services.AddSingleton<SmtpEmailService>();
        services.AddScoped<INotificationService, SmtpNotificationService>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<BankAccountRepository>();
        services.AddScoped<TransactionRepository>();
        services.AddScoped<TransactionCategoryRepository>();
        services.AddScoped<WalletRepository>();
        services.AddScoped<NotificationRepository>();
        services.AddScoped<CreditCardRepository>();
        services.AddScoped<InvoiceRepository>();
        services.AddScoped<BudgetRepository>();

        // Outbox
        services.AddHostedService<OutboxProcessor>();

        // Serviços
        services.AddScoped<Application.Common.Interfaces.IPasswordHasher, PasswordService>();
        services.AddScoped<Application.Common.Interfaces.ITokenService, TokenService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IReportGeneratorService, External.Reports.QuestPdfReportService>();

        return services;
    }
}
