using BookHub.Domain.Exceptions;

namespace BookHub.Domain.Entities
{
    public class Genre
    {
        public const int NameMaxLength = 100;

        private readonly List<Book> _books = new();

        private Genre()
        {
            Name = string.Empty;
        }

        public Genre(string name)
        {
            Id = Guid.NewGuid();
            Name = ValidateName(name);
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyCollection<Book> Books => _books;

        public void Update(string name)
        {
            Name = ValidateName(name);
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("O nome do gênero é obrigatório.");

            name = name.Trim();

            if (name.Length > NameMaxLength)
                throw new DomainException($"O nome do gênero deve ter no máximo {NameMaxLength} caracteres.");

            return name;
        }
    }
}
