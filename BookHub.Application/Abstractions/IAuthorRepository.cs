using BookHub.Application.Common;
using BookHub.Domain.Entities;

namespace BookHub.Application.Abstractions
{
    public interface IAuthorRepository
    {
        Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Author>> SearchAsync(ListQuery query, CancellationToken cancellationToken = default);
        Task<bool> HasBooksAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(Author author);
        void Remove(Author author);
    }
}
