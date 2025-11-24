namespace courses_buynsell_api.DTOs.Course;

public class CourseQueryParameters
{
    private int _page = 1;
    private int _pageSize = 10;
    public int Page
    {
        get => _page;
        set => _page = (value <= 0) ? 1 : value;
    }
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 10 : Math.Min(value, 100);
    }
    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public int? SellerId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } = "newest";
    public string? Level { get; set; }
    public bool IncludeUnapproved { get; set; } = false; // admin

}