namespace Monetra.Application.Common.DTOs;

/// <summary>
/// Resposta padronizada de erro da API.
/// </summary>
public class ErrorResponse
{
    public bool Success => false;
    public ErrorDetails Error { get; set; } = new();
    public MetaDetails Meta { get; set; } = new();

    public static ErrorResponse Create(string code, string message, List<ErrorDetail>? details = null)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = code,
                Message = message,
                Details = details ?? new List<ErrorDetail>()
            },
            Meta = new MetaDetails
            {
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString()
            }
        };
    }
}

public class ErrorDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetail> Details { get; set; } = new();
}

public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public static ErrorDetail Create(string field, string message)
    {
        return new ErrorDetail { Field = field, Message = message };
    }
}

public class MetaDetails
{
    public DateTime Timestamp { get; set; }
    public string RequestId { get; set; } = string.Empty;
}
