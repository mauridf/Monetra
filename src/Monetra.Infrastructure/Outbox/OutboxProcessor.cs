using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagens do outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MonetraDbContext>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var messages = await context.OutboxMessages
            .Where(m => m.Status == "pending" && m.RetryCount < m.MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await messageBus.PublishAsync(message.Type, message.Content, cancellationToken);
                message.MarkAsSent();
                _logger.LogDebug("Mensagem outbox {MessageId} enviada: {Type}", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                _logger.LogWarning(ex, "Falha ao enviar mensagem outbox {MessageId}", message.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
