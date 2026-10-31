using BookHub.Application.Common;
using BookHub.Domain.Entities;

namespace BookHub.Application.Abstractions
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Book>> SearchAsync(BookListQuery query, CancellationToken cancellationToken = default);
        void Add(Book book);
        void Remove(Book book);
    }
}
