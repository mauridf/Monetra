namespace Monetra.Core.Interfaces;

/// <summary>
/// Serviço de armazenamento de arquivos (comprovantes, relatórios, etc).
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Faz upload de um arquivo.
    /// </summary>
    /// <returns>URL pública do arquivo</returns>
    Task<string> UploadAsync(
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Faz download de um arquivo.
    /// </summary>
    Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um arquivo do storage.
    /// </summary>
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera URL temporária de acesso (assinada).
    /// </summary>
    Task<string> GetPresignedUrlAsync(string fileUrl, TimeSpan expiration, CancellationToken cancellationToken = default);
}
