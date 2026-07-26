using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Monetra.Api.Filters;
using Monetra.Api.Middlewares;
using Monetra.Application;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure;
using Monetra.Infrastructure.Data.Migrations;
using Monetra.Infrastructure.Services;
using Scalar.AspNetCore;
using Serilog;

// =============================================
// Configuração inicial do Serilog
// =============================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Iniciando Monetra API...");

    var builder = WebApplication.CreateBuilder(args);

    // =============================================
    // Serilog
    // =============================================
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    });

    // =============================================
    // Adicionar serviços
    // =============================================

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey não configurada.");

    var key = Encoding.UTF8.GetBytes(secretKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "monetra-api",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "monetra-app",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };

        // Eventos JWT para logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Falha na autenticação JWT: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("Token JWT validado para: {User}",
                    context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Premium", policy =>
            policy.RequireClaim("isPremium", "true"));

        options.AddPolicy("Admin", policy =>
            policy.RequireRole("admin"));

        options.AddPolicy("EmailVerified", policy =>
            policy.RequireClaim("emailVerified", "true"));
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Global: 100 req/min
        options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter
            .Create<HttpContext, string>(context =>
                System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    "global",
                    _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

        // Login: 5 req/min
        options.AddPolicy("LoginEndpoint", context =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Register: 3 req/hora
        options.AddPolicy("RegisterEndpoint", context =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 3,
                    Window = TimeSpan.FromHours(1)
                }));
    });

    // Controllers + Filters
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidateModelFilter>();
    });

    // Scalar / OpenAPI
    builder.Services.AddOpenApi();

    // CORS
    builder.Services.AddCors(options =>
    {
        var corsOrigins = builder.Configuration
            .GetSection("Application:CorsOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:3000" };

        options.AddPolicy("Default", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks();

    // Camadas da aplicação
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Serviços scoped
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    var app = builder.Build();

    // =============================================
    // Executar Migrations (DbUp)
    // =============================================
    var connectionString = builder.Configuration["Database:ConnectionString"];
    if (!string.IsNullOrEmpty(connectionString))
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        Log.Information("Executando migrations do banco de dados...");
        var success = DbUpRunner.RunMigrations(connectionString, logger);

        if (!success)
        {
            Log.Warning("Algumas migrations falharam. Verifique os logs.");
        }
    }

    // =============================================
    // Pipeline HTTP (Ordem é importante!)
    // =============================================

    // 1. Tratamento global de exceções (primeiro na pipeline)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // 2. Logging de requests
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 3. Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000}ms";
    });

    // 4. Rate limiting
    app.UseRateLimiter();

    // 5. HTTPS redirection
    app.UseHttpsRedirection();

    // 6. CORS
    app.UseCors("Default");

    // 7. Autenticação
    app.UseAuthentication();

    // 8. Contexto do usuário
    app.UseMiddleware<UserContextMiddleware>();

    // 9. Autorização
    app.UseAuthorization();

    // 10. Mapear controllers
    app.MapControllers();

    // 11. Health checks
    app.MapHealthChecks("/health");

    // Health check básico (liveness)
    app.MapGet("/health/live", () => Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Version = "1.0.0"
    }));

    // Health check detalhado (readiness)
    app.MapGet("/health/ready", () =>
    {
        // Aqui verificaria conexão com banco, Redis, etc.
        return Results.Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
    });

    // 12. Documentação (apenas em Dev)
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "Monetra API v1";
            options.Theme = ScalarTheme.DeepSpace;
            options.Layout = ScalarLayout.Modern;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.ShowSidebar = true;
            options.HideDownloadButton = false;
        });
    }

    // =============================================
    // Iniciar aplicação
    // =============================================
    Log.Information("✅ Monetra API iniciada com sucesso!");
    Log.Information("📡 API: http://localhost:5000");
    Log.Information("📊 Health: http://localhost:5000/health");
    Log.Information("📚 Docs: http://localhost:5000/scalar/v1");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
