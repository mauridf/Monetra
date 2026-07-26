using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Monetra.Core.Interfaces;

namespace Monetra.Infrastructure.External.Storage;

/// <summary>
/// Implementação do serviço de storage usando MinIO (S3-compatible).
/// </summary>
public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IConfiguration configuration, ILogger<MinioStorageService> logger)
    {
        var endpoint = configuration["MinIo:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIo:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIo:SecretKey"] ?? "minioadmin";
        _bucketName = configuration["MinIo:BucketName"] ?? "monetra";

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();

        _logger = logger;

        // Garantir que o bucket existe
        EnsureBucketExistsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!found)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
                _logger.LogInformation("Bucket '{BucketName}' criado com sucesso", _bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível verificar/criar bucket MinIO. Usando fallback local.");
        }
    }

    public async Task<string> UploadAsync(
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var objectName = $"{Guid.NewGuid()}/{fileName}";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _client.PutObjectAsync(putObjectArgs, cancellationToken);

            var url = $"{_bucketName}/{objectName}";
            _logger.LogInformation("Arquivo enviado para MinIO: {Url}", url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload para MinIO: {FileName}", fileName);
            throw new InvalidOperationException("Falha ao armazenar arquivo.", ex);
        }
    }

    public async Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var parts = fileUrl.Split('/', 2);
            if (parts.Length != 2)
                throw new ArgumentException("URL de arquivo inválida", nameof(fileUrl));

            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(parts[1])
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _client.GetObjectAsync(getObjectArgs, cancellationToken);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download do MinIO: {Url}", fileUrl);
            throw new InvalidOperationException("Falha ao recuperar arquivo.", ex);
        }
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var parts = fileUrl.Split('/', 2);
            if (parts.Length != 2) return;

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(parts[1]);

            await _client.RemoveObjectAsync(removeObjectArgs, cancellationToken);
            _logger.LogInformation("Arquivo removido do MinIO: {Url}", fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao remover arquivo do MinIO: {Url}", fileUrl);
        }
    }

    public async Task<string> GetPresignedUrlAsync(string fileUrl, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var parts = fileUrl.Split('/', 2);
            if (parts.Length != 2)
                return fileUrl;

            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(parts[1])
                .WithExpiry((int)expiration.TotalSeconds);

            return await _client.PresignedGetObjectAsync(presignedGetObjectArgs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao gerar URL assinada: {Url}", fileUrl);
            return fileUrl;
        }
    }
}
