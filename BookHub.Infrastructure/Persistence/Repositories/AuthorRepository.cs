using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookHub.Infrastructure.Persistence.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly BookHubContext _context;

        public AuthorRepository(BookHubContext context)
        {
            _context = context;
        }

        public Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Authors.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        public async Task<PagedResult<Author>> SearchAsync(ListQuery query, CancellationToken cancellationToken = default)
        {
            var authors = _context.Authors.AsNoTracking();

            if (query.Search is not null)
            {
                var pattern = SearchExtensions.ToLikePattern(query.Search);
                authors = authors.Where(a => EF.Functions.ILike(a.Name, pattern, SearchExtensions.EscapeCharacter));
            }

            var total = await authors.CountAsync(cancellationToken);

            var items = await authors
                .OrderBy(a => a.Name).ThenBy(a => a.Id)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Author>(items, query.Page, query.PageSize, total);
        }

        public Task<bool> HasBooksAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Books.AnyAsync(b => b.AuthorId == id, cancellationToken);

        public void Add(Author author) => _context.Authors.Add(author);

        public void Remove(Author author) => _context.Authors.Remove(author);
    }
}
