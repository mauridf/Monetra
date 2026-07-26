using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;

namespace Monetra.Infrastructure.External.Storage;

/// <summary>
/// Implementação fallback de storage local (para desenvolvimento sem MinIO).
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(ILogger<LocalStorageService> logger)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _logger = logger;

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Diretório de uploads criado: {Path}", _basePath);
        }
    }

    public async Task<string> UploadAsync(
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var folderName = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(_basePath, folderName);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);

        await using var fileWriteStream = File.Create(filePath);
        await fileStream.CopyToAsync(fileWriteStream, cancellationToken);

        var relativePath = Path.Combine(folderName, fileName);
        _logger.LogInformation("Arquivo salvo localmente: {Path}", relativePath);

        return relativePath;
    }

    public Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, fileUrl);

        if (!File.Exists(filePath))
        {
            _logger.LogError("Arquivo não encontrado: {Path}", filePath);
            throw new FileNotFoundException("Arquivo não encontrado.", filePath);
        }

        var stream = File.OpenRead(filePath);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, fileUrl);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Arquivo removido localmente: {Path}", fileUrl);
        }

        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(string fileUrl, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        // Em local, retorna o path relativo (não há URL assinada real)
        return Task.FromResult(fileUrl);
    }
}
