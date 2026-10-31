using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookHub.Infrastructure.Persistence.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly BookHubContext _context;

        public GenreRepository(BookHubContext context)
        {
            _context = context;
        }

        public Task<Genre?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Genres.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        public async Task<PagedResult<Genre>> SearchAsync(ListQuery query, CancellationToken cancellationToken = default)
        {
            var genres = _context.Genres.AsNoTracking();

            if (query.Search is not null)
            {
                var pattern = SearchExtensions.ToLikePattern(query.Search);
                genres = genres.Where(g => EF.Functions.ILike(g.Name, pattern, SearchExtensions.EscapeCharacter));
            }

            var total = await genres.CountAsync(cancellationToken);

            var items = await genres
                .OrderBy(g => g.Name).ThenBy(g => g.Id)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Genre>(items, query.Page, query.PageSize, total);
        }

        public Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default)
        {
            var normalized = name.ToLower();

            return _context.Genres.AnyAsync(
                g => g.Name.ToLower() == normalized && (excludeId == null || g.Id != excludeId),
                cancellationToken);
        }

        public Task<bool> HasBooksAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Books.AnyAsync(b => b.GenreId == id, cancellationToken);

        public void Add(Genre genre) => _context.Genres.Add(genre);

        public void Remove(Genre genre) => _context.Genres.Remove(genre);
    }
}
