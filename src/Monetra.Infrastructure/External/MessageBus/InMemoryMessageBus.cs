using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;

namespace Monetra.Infrastructure.External.MessageBus;

/// <summary>
/// Implementação fallback de message bus em memória (para desenvolvimento sem RabbitMQ).
/// </summary>
public class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<object>> _queues = new();
    private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _handlers = new();
    private readonly ILogger<InMemoryMessageBus> _logger;

    public InMemoryMessageBus(ILogger<InMemoryMessageBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        var queue = _queues.GetOrAdd(queueName, _ => new ConcurrentQueue<object>());
        queue.Enqueue(message);

        _logger.LogDebug("Mensagem publicada na fila '{QueueName}' (in-memory): {MessageType}", queueName, typeof(T).Name);

        // Processar handlers assinados
        if (_handlers.TryGetValue(queueName, out var handlers))
        {
            foreach (var handler in handlers)
            {
                _ = Task.Run(() => handler(message), cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        var wrappedHandler = new Func<object, Task>(async (obj) =>
        {
            if (obj is T typedMessage)
            {
                await handler(typedMessage);
            }
        });

        _handlers.AddOrUpdate(
            queueName,
            _ => new List<Func<object, Task>> { wrappedHandler },
            (_, list) =>
            {
                list.Add(wrappedHandler);
                return list;
            });

        _logger.LogInformation("Assinatura criada na fila '{QueueName}' (in-memory)", queueName);

        return Task.CompletedTask;
    }

    public Task PublishWithOutboxAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        return PublishAsync(queueName, message, cancellationToken);
    }
}
