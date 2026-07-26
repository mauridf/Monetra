namespace Monetra.Application.Common.DTOs;

/// <summary>
/// Resultado paginado para listagens.
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PerPage);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PaginatedResult<T> Create(List<T> items, int total, int page, int perPage)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            Total = total,
            Page = page,
            PerPage = perPage
        };
    }
}
