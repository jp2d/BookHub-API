using BookHub.Application.Common;

namespace BookHub.Application.Books
{
    public interface IBookService
    {
        Task<PagedResult<BookResponse>> GetPagedAsync(BookListQuery query, CancellationToken cancellationToken = default);
        Task<BookResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BookResponse> CreateAsync(BookRequest request, CancellationToken cancellationToken = default);
        Task<BookResponse> UpdateAsync(Guid id, BookRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
