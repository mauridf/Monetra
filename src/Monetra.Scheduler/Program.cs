using Serilog;
using Monetra.Infrastructure;
using Monetra.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Configuração inicial do Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("⏰ Iniciando Monetra Scheduler...");

    var builder = Host.CreateApplicationBuilder(args);

    // Configuração
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    // Serilog
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // Infrastructure (banco de dados, repositórios)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Quartz Scheduler
    builder.Services.AddQuartzJobs();

    var host = builder.Build();

    Log.Information("✅ Monetra Scheduler configurado e iniciado com sucesso!");
    Log.Information("📋 Jobs configurados:");
    Log.Information("   🔄 RecurringTransactionJob - Diariamente 06:00");
    Log.Information("   📄 InvoiceGenerationJob - Diariamente 02:00");
    Log.Information("   🔔 DueDateNotificationJob - Diariamente 08:00");
    Log.Information("   📊 BudgetAlertJob - Segundas 09:00");
    Log.Information("   🧹 CleanupJob - Domingos 03:00");
    Log.Information("   ⭐ PremiumExpirationJob - Diariamente 04:00");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Scheduler terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
