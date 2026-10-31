using BookHub.Domain.Exceptions;

namespace BookHub.Domain.Entities
{
    public class Book
    {
        public const int TitleMaxLength = 200;
        public const int IsbnMaxLength = 13;

        private Book()
        {
            Title = string.Empty;
        }

        public Book(string title, int publicationYear, string? isbn, Guid authorId, Guid genreId)
        {
            Id = Guid.NewGuid();
            Title = ValidateTitle(title);
            PublicationYear = ValidatePublicationYear(publicationYear);
            Isbn = NormalizeIsbn(isbn);
            AuthorId = ValidateReference(authorId, "autor");
            GenreId = ValidateReference(genreId, "gênero");
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public int PublicationYear { get; private set; }
        public string? Isbn { get; private set; }

        public Guid AuthorId { get; private set; }
        public Author? Author { get; private set; }

        public Guid GenreId { get; private set; }
        public Genre? Genre { get; private set; }

        public void Update(string title, int publicationYear, string? isbn, Guid authorId, Guid genreId)
        {
            Title = ValidateTitle(title);
            PublicationYear = ValidatePublicationYear(publicationYear);
            Isbn = NormalizeIsbn(isbn);
            AuthorId = ValidateReference(authorId, "autor");
            GenreId = ValidateReference(genreId, "gênero");
        }

        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("O título do livro é obrigatório.");

            title = title.Trim();

            if (title.Length > TitleMaxLength)
                throw new DomainException($"O título do livro deve ter no máximo {TitleMaxLength} caracteres.");

            return title;
        }

        private static int ValidatePublicationYear(int year)
        {
            var currentYear = DateTime.UtcNow.Year;

            if (year < 1 || year > currentYear)
                throw new DomainException($"O ano de publicação deve estar entre 1 e {currentYear}.");

            return year;
        }

        private static string? NormalizeIsbn(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            var normalized = isbn.Replace("-", string.Empty).Replace(" ", string.Empty);

            var validLength = normalized.Length is 10 or 13;
            var validChars = normalized.Take(normalized.Length - 1).All(char.IsDigit)
                && (char.IsDigit(normalized[^1]) || (normalized.Length == 10 && char.ToUpperInvariant(normalized[^1]) == 'X'));

            if (!validLength || !validChars)
                throw new DomainException("O ISBN deve conter 10 ou 13 dígitos.");

            return normalized.ToUpperInvariant();
        }

        private static Guid ValidateReference(Guid id, string name)
        {
            if (id == Guid.Empty)
                throw new DomainException($"O {name} do livro é obrigatório.");

            return id;
        }
    }
}
