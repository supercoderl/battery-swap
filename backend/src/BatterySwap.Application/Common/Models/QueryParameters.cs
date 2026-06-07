namespace BatterySwap.Application.Common.Models;

/// <summary>Base query string parameters for pagination, search and sorting.</summary>
public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
    }

    /// <summary>Free-text search term.</summary>
    public string? Search { get; set; }

    /// <summary>Field name to sort by.</summary>
    public string? SortBy { get; set; }

    /// <summary>True for descending order.</summary>
    public bool SortDescending { get; set; }
}
