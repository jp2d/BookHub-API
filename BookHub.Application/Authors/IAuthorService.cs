using BookHub.Application.Common;

namespace BookHub.Application.Authors
{
    public interface IAuthorService
    {
        Task<PagedResult<AuthorResponse>> GetPagedAsync(ListQuery query, CancellationToken cancellationToken = default);
        Task<AuthorResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AuthorResponse> CreateAsync(AuthorRequest request, CancellationToken cancellationToken = default);
        Task<AuthorResponse> UpdateAsync(Guid id, AuthorRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
