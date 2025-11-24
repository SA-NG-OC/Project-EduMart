namespace courses_buynsell_api.DTOs;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages =>  (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    
}