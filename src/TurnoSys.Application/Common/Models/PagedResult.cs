namespace TurnoSys.Application.Common.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}
