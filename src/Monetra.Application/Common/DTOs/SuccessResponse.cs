namespace Monetra.Application.Common.DTOs;

/// <summary>
/// Resposta padronizada de sucesso da API.
/// </summary>
public class SuccessResponse<T>
{
    public bool Success => true;
    public T? Data { get; set; }
    public MetaDetails Meta { get; set; } = new();

    public static SuccessResponse<T> Create(T data)
    {
        return new SuccessResponse<T>
        {
            Data = data,
            Meta = new MetaDetails
            {
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString()
            }
        };
    }
}

/// <summary>
/// Resposta paginada de sucesso.
/// </summary>
public class PaginatedSuccessResponse<T>
{
    public bool Success => true;
    public List<T> Data { get; set; } = new();
    public PaginationMeta Meta { get; set; } = new();

    public static PaginatedSuccessResponse<T> Create(PaginatedResult<T> result)
    {
        return new PaginatedSuccessResponse<T>
        {
            Data = result.Items,
            Meta = new PaginationMeta
            {
                Page = result.Page,
                PerPage = result.PerPage,
                Total = result.Total,
                TotalPages = result.TotalPages,
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString()
            }
        };
    }
}

public class PaginationMeta : MetaDetails
{
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}
