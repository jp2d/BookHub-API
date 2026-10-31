using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookHub.Infrastructure.Persistence.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly BookHubContext _context;

        public BookRepository(BookHubContext context)
        {
            _context = context;
        }

        public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        public async Task<PagedResult<Book>> SearchAsync(BookListQuery query, CancellationToken cancellationToken = default)
        {
            var books = _context.Books.AsNoTracking();

            if (query.Search is not null)
            {
                var pattern = SearchExtensions.ToLikePattern(query.Search);
                books = books.Where(b => EF.Functions.ILike(b.Title, pattern, SearchExtensions.EscapeCharacter));
            }

            if (query.AuthorId is not null)
                books = books.Where(b => b.AuthorId == query.AuthorId);

            if (query.GenreId is not null)
                books = books.Where(b => b.GenreId == query.GenreId);

            var total = await books.CountAsync(cancellationToken);

            var ordered = (query.SortBy, query.Descending) switch
            {
                ("year", false) => books.OrderBy(b => b.PublicationYear),
                ("year", true) => books.OrderByDescending(b => b.PublicationYear),
                (_, true) => books.OrderByDescending(b => b.Title),
                _ => books.OrderBy(b => b.Title)
            };

            var items = await ordered
                .ThenBy(b => b.Id)
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Book>(items, query.Page, query.PageSize, total);
        }

        public void Add(Book book) => _context.Books.Add(book);

        public void Remove(Book book) => _context.Books.Remove(book);
    }
}
