using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Monetra.Infrastructure.External.MessageBus;

public class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
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
            Password = password
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _logger.LogInformation("Conectado ao RabbitMQ em {Host}:{Port}", host, port);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ. Mensageria assíncrona indisponível.");
            throw;
        }
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _channel!.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message, JsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Mensagem publicada na fila '{QueueName}': {MessageType}", queueName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila '{QueueName}'", queueName);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _channel!.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json, JsonOptions);

                    if (message != null)
                    {
                        await handler(message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                        _logger.LogDebug("Mensagem processada da fila '{QueueName}'", queueName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila '{QueueName}'", queueName);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Assinatura criada na fila '{QueueName}'", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao assinar fila '{QueueName}'", queueName);
            throw;
        }
    }

    public async Task PublishWithOutboxAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(queueName, message, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
