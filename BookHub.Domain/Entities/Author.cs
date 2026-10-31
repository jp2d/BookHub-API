using BookHub.Domain.Exceptions;

namespace BookHub.Domain.Entities
{
    public class Author
    {
        public const int NameMaxLength = 200;

        private readonly List<Book> _books = new();

        private Author()
        {
            Name = string.Empty;
        }

        public Author(string name)
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
                throw new DomainException("O nome do autor é obrigatório.");

            name = name.Trim();

            if (name.Length > NameMaxLength)
                throw new DomainException($"O nome do autor deve ter no máximo {NameMaxLength} caracteres.");

            return name;
        }
    }
}
