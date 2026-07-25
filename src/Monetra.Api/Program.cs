using Serilog;
using Scalar.AspNetCore;

// Configuração inicial do Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando Monetra API...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    });

    // Adicionar serviços
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        var corsOrigins = builder.Configuration.GetSection("Application:CorsOrigins").Get<string[]>()
                          ?? Array.Empty<string>();

        options.AddPolicy("Default", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Configurar pipeline HTTP
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "Monetra API";
            options.Theme = ScalarTheme.Default;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("Default");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check básico
    app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
