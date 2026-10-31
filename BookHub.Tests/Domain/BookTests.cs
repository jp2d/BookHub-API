using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;

namespace BookHub.Tests.Domain
{
    public class BookTests
    {
        private static readonly Guid AuthorId = Guid.NewGuid();
        private static readonly Guid GenreId = Guid.NewGuid();

        private static Book NewBook(
            string title = "Dom Casmurro",
            int year = 1899,
            string? isbn = null,
            Guid? authorId = null,
            Guid? genreId = null) =>
            new(title, year, isbn, authorId ?? AuthorId, genreId ?? GenreId);

        [Fact]
        public void Constructor_WithValidData_CreatesBook()
        {
            var book = NewBook("  Dom Casmurro ", 1899, "978-85-359-0277-5");

            Assert.NotEqual(Guid.Empty, book.Id);
            Assert.Equal("Dom Casmurro", book.Title);
            Assert.Equal(1899, book.PublicationYear);
            Assert.Equal("9788535902775", book.Isbn);
            Assert.Equal(AuthorId, book.AuthorId);
            Assert.Equal(GenreId, book.GenreId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Constructor_WithEmptyTitle_Throws(string? title)
        {
            Assert.Throws<DomainException>(() => NewBook(title!));
        }

        [Fact]
        public void Constructor_WithTitleLongerThanLimit_Throws()
        {
            Assert.Throws<DomainException>(() => NewBook(new string('a', Book.TitleMaxLength + 1)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Constructor_WithNonPositiveYear_Throws(int year)
        {
            Assert.Throws<DomainException>(() => NewBook(year: year));
        }

        [Fact]
        public void Constructor_WithFutureYear_Throws()
        {
            Assert.Throws<DomainException>(() => NewBook(year: DateTime.UtcNow.Year + 1));
        }

        [Fact]
        public void Constructor_WithCurrentYear_Succeeds()
        {
            var book = NewBook(year: DateTime.UtcNow.Year);

            Assert.Equal(DateTime.UtcNow.Year, book.PublicationYear);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithoutIsbn_StoresNull(string? isbn)
        {
            Assert.Null(NewBook(isbn: isbn).Isbn);
        }

        [Theory]
        [InlineData("9788535902775", "9788535902775")]
        [InlineData("978-85-359-0277-5", "9788535902775")]
        [InlineData("978 85 359 0277 5", "9788535902775")]
        [InlineData("0306406152", "0306406152")]
        [InlineData("080442957x", "080442957X")]
        public void Constructor_WithValidIsbn_NormalizesValue(string isbn, string expected)
        {
            Assert.Equal(expected, NewBook(isbn: isbn).Isbn);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("97885359027")]
        [InlineData("97885359027ab")]
        [InlineData("978853590277X")]
        public void Constructor_WithInvalidIsbn_Throws(string isbn)
        {
            Assert.Throws<DomainException>(() => NewBook(isbn: isbn));
        }

        [Fact]
        public void Constructor_WithEmptyAuthorId_Throws()
        {
            Assert.Throws<DomainException>(() => NewBook(authorId: Guid.Empty));
        }

        [Fact]
        public void Constructor_WithEmptyGenreId_Throws()
        {
            Assert.Throws<DomainException>(() => NewBook(genreId: Guid.Empty));
        }

        [Fact]
        public void Update_WithValidData_ChangesAllFields()
        {
            var book = NewBook();
            var newAuthor = Guid.NewGuid();
            var newGenre = Guid.NewGuid();

            book.Update("Memórias Póstumas", 1881, "0306406152", newAuthor, newGenre);

            Assert.Equal("Memórias Póstumas", book.Title);
            Assert.Equal(1881, book.PublicationYear);
            Assert.Equal("0306406152", book.Isbn);
            Assert.Equal(newAuthor, book.AuthorId);
            Assert.Equal(newGenre, book.GenreId);
        }

        [Fact]
        public void Update_WithInvalidData_Throws()
        {
            var book = NewBook();

            Assert.Throws<DomainException>(() => book.Update("", 1881, null, AuthorId, GenreId));
        }
    }
}
