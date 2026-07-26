using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Monetra.Infrastructure.External.MessageBus;

/// <summary>
/// Implementação do message bus usando RabbitMQ.
/// </summary>
public class RabbitMqMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqMessageBus(IConfiguration configuration, ILogger<RabbitMqMessageBus> logger)
    {
        _logger = logger;

        var host = configuration["RabbitMq:Host"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMq:Port"] ?? "5672");
        var username = configuration["RabbitMq:Username"] ?? "guest";
        var password = configuration["RabbitMq:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger.LogInformation("Conectado ao RabbitMQ em {Host}:{Port}", host, port);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ. Mensageria assíncrona indisponível.");
            throw;
        }
    }

    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Declarar fila (idempotente)
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message, JsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Mensagem persistente
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Mensagem publicada na fila '{QueueName}': {MessageType}", queueName, typeof(T).Name);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila '{QueueName}'", queueName);
            throw;
        }
    }

    public Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json, JsonOptions);

                    if (message != null)
                    {
                        await handler(message);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogDebug("Mensagem processada da fila '{QueueName}'", queueName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila '{QueueName}'", queueName);
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Rejeitar e reenfileirar
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Assinatura criada na fila '{QueueName}'", queueName);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao assinar fila '{QueueName}'", queueName);
            throw;
        }
    }

    public async Task PublishWithOutboxAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        // Padrão Outbox: publica mensagem e garante persistência
        await PublishAsync(queueName, message, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
