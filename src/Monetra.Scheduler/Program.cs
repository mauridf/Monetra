using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando Monetra Scheduler...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Quartz será configurado posteriormente

    var host = builder.Build();

    Log.Information("Monetra Scheduler iniciado com sucesso");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Scheduler terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
