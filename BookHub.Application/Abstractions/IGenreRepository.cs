using BookHub.Application.Common;
using BookHub.Domain.Entities;

namespace BookHub.Application.Abstractions
{
    public interface IGenreRepository
    {
        Task<Genre?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Genre>> SearchAsync(ListQuery query, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default);
        Task<bool> HasBooksAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(Genre genre);
        void Remove(Genre genre);
    }
}
