using BookHub.Application.Common;

namespace BookHub.Application.Genres
{
    public interface IGenreService
    {
        Task<PagedResult<GenreResponse>> GetPagedAsync(ListQuery query, CancellationToken cancellationToken = default);
        Task<GenreResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<GenreResponse> CreateAsync(GenreRequest request, CancellationToken cancellationToken = default);
        Task<GenreResponse> UpdateAsync(Guid id, GenreRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
