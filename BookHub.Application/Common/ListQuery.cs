namespace BookHub.Application.Common
{
    public static class Paging
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }

    public record ListQuery(string? Search = null, int Page = 1, int PageSize = Paging.DefaultPageSize)
    {
        public int Skip => (Page - 1) * PageSize;

        public ListQuery Normalize() => this with
        {
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            Page = Math.Max(1, Page),
            PageSize = Math.Clamp(PageSize, 1, Paging.MaxPageSize)
        };
    }

    public record BookListQuery(
        string? Search = null,
        Guid? AuthorId = null,
        Guid? GenreId = null,
        string? SortBy = null,
        bool Descending = false,
        int Page = 1,
        int PageSize = Paging.DefaultPageSize)
    {
        public int Skip => (Page - 1) * PageSize;

        public BookListQuery Normalize() => this with
        {
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            SortBy = SortBy?.Trim().ToLowerInvariant(),
            Page = Math.Max(1, Page),
            PageSize = Math.Clamp(PageSize, 1, Paging.MaxPageSize)
        };
    }
}
