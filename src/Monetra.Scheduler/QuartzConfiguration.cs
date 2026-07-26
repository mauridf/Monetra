using Microsoft.Extensions.DependencyInjection;
using Monetra.Scheduler.Jobs;
using Quartz;

namespace Monetra.Scheduler;

/// <summary>
/// Configuração dos jobs Quartz.NET com seus schedules.
/// </summary>
public static class QuartzConfiguration
{
    /// <summary>
    /// Adiciona e configura todos os jobs do scheduler.
    /// </summary>
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            // Usar identificador único para a instância do scheduler
            q.SchedulerId = "MonetraScheduler";
            q.SchedulerName = "Monetra Quartz Scheduler";

            // Configurar jobs
            q.AddRecurringTransactionJob();
            q.AddInvoiceGenerationJob();
            q.AddDueDateNotificationJob();
            q.AddBudgetAlertJob();
            q.AddCleanupJob();
            q.AddPremiumExpirationJob();
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
            options.AwaitApplicationStarted = true;
        });

        return services;
    }

    /// <summary>
    /// Job: Processa transações recorrentes (diariamente às 06:00).
    /// </summary>
    private static void AddRecurringTransactionJob(this IServiceCollectionQuartzConfigurator q)
    {
        var jobKey = new JobKey("RecurringTransactionJob");

        q.AddJob<RecurringTransactionJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("RecurringTransactionTrigger")
            .WithCronSchedule("0 0 6 * * ?", x => x
                .WithMisfireHandlingInstructionFireAndProceed())); // Diariamente 06:00
    }

    /// <summary>
    /// Job: Gera faturas de cartão de crédito (diariamente às 02:00).
    /// </summary>
    private static void AddInvoiceGenerationJob(this IServiceCollectionQuartzConfigurator q)
    {
        var jobKey = new JobKey("InvoiceGenerationJob");

        q.AddJob<InvoiceGenerationJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("InvoiceGenerationTrigger")
            .WithCronSchedule("0 0 2 * * ?", x => x
                .WithMisfireHandlingInstructionFireAndProceed())); // Diariamente 02:00
    }

    /// <summary>
    /// Job: Notifica usuários sobre contas a vencer (diariamente às 08:00).
    /// </summary>
    private static void AddDueDateNotificationJob(this IServiceCollectionQuartzConfigurator q)
    {
        var jobKey = new JobKey("DueDateNotificationJob");

        q.AddJob<DueDateNotificationJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("DueDateNotificationTrigger")
            .WithCronSchedule("0 0 8 * * ?", x => x
                .WithMisfireHandlingInstructionFireAndProceed())); // Diariamente 08:00
    }

    /// <summary>
    /// Job: Alerta de orçamento estourado (semanal, segunda 09:00).
    /// </summary>
    private static void AddBudgetAlertJob(this IServiceCollectionQuartzConfigurator q)
    {
        var jobKey = new JobKey("BudgetAlertJob");

        q.AddJob<BudgetAlertJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("BudgetAlertTrigger")
            .WithCronSchedule("0 0 9 * * MON", x => x
                .WithMisfireHandlingInstructionFireAndProceed())); // Toda segunda 09:00
    }

    /// <summary>
    /// Job: Limpeza de dados antigos (semanal, domingo 03:00).
    /// </summary>
    private static void AddCleanupJob(this IServiceCollectionQuartzConfigurator q)
    {
        var jobKey = new JobKey("CleanupJob");

        q.AddJob<CleanupJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("CleanupTrigger")
            .WithCronSchedule("0 0 3 * * SUN", x => x
                .WithMisfireHandlingInstructionFireAndProceed())); // Todo domingo 03:00
    }

    /// <summary>
    /// Job: Verifica expiração de premium (diariamente às 04:00).
    /// </summary>
    private static void AddPremiumExpirationJob(this IServiceCollectionQuartzConfigurator q)
    {
        var jobKey = new JobKey("PremiumExpirationJob");

        q.AddJob<PremiumExpirationJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("PremiumExpirationTrigger")
            .WithCronSchedule("0 0 4 * * ?", x => x
                .WithMisfireHandlingInstructionFireAndProceed())); // Diariamente 04:00
    }
}
