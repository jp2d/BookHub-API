using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;

namespace BookHub.Application.Books
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _books;
        private readonly IAuthorRepository _authors;
        private readonly IGenreRepository _genres;
        private readonly IUnitOfWork _unitOfWork;

        public BookService(
            IBookRepository books,
            IAuthorRepository authors,
            IGenreRepository genres,
            IUnitOfWork unitOfWork)
        {
            _books = books;
            _authors = authors;
            _genres = genres;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<BookResponse>> GetPagedAsync(BookListQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _books.SearchAsync(query.Normalize(), cancellationToken);
            return result.Map(b => b.ToResponse());
        }

        public async Task<BookResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var book = await _books.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Livro não encontrado.");

            return book.ToResponse();
        }

        public async Task<BookResponse> CreateAsync(BookRequest request, CancellationToken cancellationToken = default)
        {
            var book = new Book(request.Title, request.PublicationYear, request.Isbn, request.AuthorId, request.GenreId);
            var (author, genre) = await LoadReferencesAsync(book.AuthorId, book.GenreId, cancellationToken);

            _books.Add(book);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return book.ToResponse(author.Name, genre.Name);
        }

        public async Task<BookResponse> UpdateAsync(Guid id, BookRequest request, CancellationToken cancellationToken = default)
        {
            var book = await _books.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Livro não encontrado.");

            book.Update(request.Title, request.PublicationYear, request.Isbn, request.AuthorId, request.GenreId);
            var (author, genre) = await LoadReferencesAsync(book.AuthorId, book.GenreId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return book.ToResponse(author.Name, genre.Name);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var book = await _books.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Livro não encontrado.");

            _books.Remove(book);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<(Author Author, Genre Genre)> LoadReferencesAsync(
            Guid authorId, Guid genreId, CancellationToken cancellationToken)
        {
            var author = await _authors.GetByIdAsync(authorId, cancellationToken)
                ?? throw new DomainException("O autor informado não existe.");

            var genre = await _genres.GetByIdAsync(genreId, cancellationToken)
                ?? throw new DomainException("O gênero informado não existe.");

            return (author, genre);
        }
    }
}
