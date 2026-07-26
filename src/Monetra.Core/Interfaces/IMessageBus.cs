namespace Monetra.Core.Interfaces;

/// <summary>
/// Serviço de mensageria para comunicação assíncrona.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publica uma mensagem em uma fila/tópico.
    /// </summary>
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Assina para receber mensagens de uma fila.
    /// </summary>
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publica mensagem usando o padrão Outbox (garantia de entrega).
    /// </summary>
    Task PublishWithOutboxAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class;
}
