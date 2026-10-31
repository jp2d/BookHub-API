using BookHub.Application.Abstractions;
using BookHub.Application.Books;
using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;
using NSubstitute;

namespace BookHub.Tests.Application
{
    public class BookServiceTests
    {
        private readonly IBookRepository _books = Substitute.For<IBookRepository>();
        private readonly IAuthorRepository _authors = Substitute.For<IAuthorRepository>();
        private readonly IGenreRepository _genres = Substitute.For<IGenreRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly BookService _service;

        private readonly Author _author = new("Machado de Assis");
        private readonly Genre _genre = new("Romance");

        public BookServiceTests()
        {
            _service = new BookService(_books, _authors, _genres, _unitOfWork);

            _authors.GetByIdAsync(_author.Id, Arg.Any<CancellationToken>()).Returns(_author);
            _genres.GetByIdAsync(_genre.Id, Arg.Any<CancellationToken>()).Returns(_genre);
        }

        private BookRequest ValidRequest() =>
            new("Dom Casmurro", 1899, "978-85-359-0277-5", _author.Id, _genre.Id);

        [Fact]
        public async Task Create_WithValidData_AddsSavesAndReturnsNames()
        {
            var result = await _service.CreateAsync(ValidRequest());

            Assert.Equal("Dom Casmurro", result.Title);
            Assert.Equal("9788535902775", result.Isbn);
            Assert.Equal(_author.Name, result.AuthorName);
            Assert.Equal(_genre.Name, result.GenreName);
            _books.Received(1).Add(Arg.Is<Book>(b => b.AuthorId == _author.Id && b.GenreId == _genre.Id));
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Create_WithUnknownAuthor_ThrowsDomainException()
        {
            var request = ValidRequest() with { AuthorId = Guid.NewGuid() };

            await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request));

            _books.DidNotReceive().Add(Arg.Any<Book>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Create_WithUnknownGenre_ThrowsDomainException()
        {
            var request = ValidRequest() with { GenreId = Guid.NewGuid() };

            await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request));

            _books.DidNotReceive().Add(Arg.Any<Book>());
        }

        [Fact]
        public async Task Create_WithInvalidTitle_ThrowsDomainException()
        {
            var request = ValidRequest() with { Title = " " };

            await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request));

            _books.DidNotReceive().Add(Arg.Any<Book>());
        }

        [Fact]
        public async Task GetById_WhenMissing_ThrowsNotFound()
        {
            _books.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Book?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Update_WhenMissing_ThrowsNotFound()
        {
            _books.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Book?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), ValidRequest()));
        }

        [Fact]
        public async Task Update_WithValidData_ChangesBookAndReturnsNewNames()
        {
            var otherAuthor = new Author("Clarice Lispector");
            var otherGenre = new Genre("Conto");
            _authors.GetByIdAsync(otherAuthor.Id, Arg.Any<CancellationToken>()).Returns(otherAuthor);
            _genres.GetByIdAsync(otherGenre.Id, Arg.Any<CancellationToken>()).Returns(otherGenre);

            var book = new Book("Dom Casmurro", 1899, null, _author.Id, _genre.Id);
            _books.GetByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

            var result = await _service.UpdateAsync(
                book.Id, new BookRequest("A Hora da Estrela", 1977, null, otherAuthor.Id, otherGenre.Id));

            Assert.Equal("A Hora da Estrela", result.Title);
            Assert.Equal(otherAuthor.Name, result.AuthorName);
            Assert.Equal(otherGenre.Name, result.GenreName);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Update_WithUnknownAuthor_ThrowsDomainExceptionAndDoesNotSave()
        {
            var book = new Book("Dom Casmurro", 1899, null, _author.Id, _genre.Id);
            _books.GetByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

            var request = ValidRequest() with { AuthorId = Guid.NewGuid() };

            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(book.Id, request));

            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Delete_WhenMissing_ThrowsNotFound()
        {
            _books.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Book?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Delete_WhenFound_RemovesAndSaves()
        {
            var book = new Book("Dom Casmurro", 1899, null, _author.Id, _genre.Id);
            _books.GetByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

            await _service.DeleteAsync(book.Id);

            _books.Received(1).Remove(book);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
