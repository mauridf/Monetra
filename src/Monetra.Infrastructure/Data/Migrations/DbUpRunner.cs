using System.Reflection;
using DbUp;
using Microsoft.Extensions.Logging;

namespace Monetra.Infrastructure.Data.Migrations;

/// <summary>
/// Executa migrations do banco de dados usando DbUp.
/// Scripts SQL versionados são executados em ordem sequencial.
/// </summary>
public static class DbUpRunner
{
    /// <summary>
    /// Executa todas as migrations pendentes no banco de dados.
    /// </summary>
    /// <param name="connectionString">String de conexão PostgreSQL</param>
    /// <param name="logger">Logger para output das migrations</param>
    /// <returns>True se todas as migrations foram executadas com sucesso</returns>
    public static bool RunMigrations(string connectionString, ILogger logger)
    {
        logger.LogInformation("Iniciando execução de migrations...");

        // Configurar DbUp com PostgreSQL
        var builder = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransaction()
            .LogTo(new DbUpLogger(logger));

        builder.Configure(c => c.VariablesEnabled = false);

        var upgrader = builder.Build();

        // Verificar se o banco existe e criar se necessário
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        // Executar migrations
        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.LogError(result.Error, "Falha ao executar migrations: {Error}", result.Error.Message);
            return false;
        }

        logger.LogInformation("Migrations executadas com sucesso!");

        foreach (var script in result.Scripts)
        {
            logger.LogInformation("Script executado: {ScriptName}", script.Name);
        }

        return true;
    }
}

/// <summary>
/// Logger adapter para DbUp integrar com Serilog/Microsoft.Extensions.Logging
/// </summary>
internal class DbUpLogger : DbUp.Engine.Output.IUpgradeLog
{
    private readonly ILogger _logger;

    public DbUpLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void WriteInformation(string format, params object[] args)
    {
        _logger.LogInformation(format, args);
    }

    public void WriteError(string format, params object[] args)
    {
        _logger.LogError(format, args);
    }

    public void WriteWarning(string format, params object[] args)
    {
        _logger.LogWarning(format, args);
    }
}
