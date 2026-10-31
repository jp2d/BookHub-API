using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookHub.Infrastructure.Persistence.Seed
{
    public class DatabaseSeeder
    {
        private static readonly string[] Genres =
        {
            "Ficção Científica", "Fantasia", "Romance", "Terror",
            "Mistério", "Clássicos", "Distopia", "Aventura"
        };

        private static readonly string[] Authors =
        {
            "Machado de Assis", "Clarice Lispector", "J. R. R. Tolkien", "George Orwell",
            "Isaac Asimov", "Stephen King", "Agatha Christie", "Jane Austen",
            "Frank Herbert", "Jorge Amado"
        };

        private static readonly (string Title, int Year, string? Isbn, string Author, string Genre)[] Books =
        {
            ("Dom Casmurro", 1899, null, "Machado de Assis", "Clássicos"),
            ("Memórias Póstumas de Brás Cubas", 1881, null, "Machado de Assis", "Clássicos"),
            ("Quincas Borba", 1891, null, "Machado de Assis", "Romance"),
            ("A Hora da Estrela", 1977, null, "Clarice Lispector", "Romance"),
            ("Perto do Coração Selvagem", 1943, null, "Clarice Lispector", "Romance"),
            ("O Hobbit", 1937, "9780547928227", "J. R. R. Tolkien", "Fantasia"),
            ("O Senhor dos Anéis: A Sociedade do Anel", 1954, null, "J. R. R. Tolkien", "Fantasia"),
            ("O Silmarillion", 1977, null, "J. R. R. Tolkien", "Fantasia"),
            ("1984", 1949, "9780451524935", "George Orwell", "Distopia"),
            ("A Revolução dos Bichos", 1945, null, "George Orwell", "Distopia"),
            ("Eu, Robô", 1950, null, "Isaac Asimov", "Ficção Científica"),
            ("Fundação", 1951, null, "Isaac Asimov", "Ficção Científica"),
            ("O Fim da Eternidade", 1955, null, "Isaac Asimov", "Ficção Científica"),
            ("O Iluminado", 1977, "9780307743657", "Stephen King", "Terror"),
            ("It: A Coisa", 1986, null, "Stephen King", "Terror"),
            ("Carrie, a Estranha", 1974, null, "Stephen King", "Terror"),
            ("Assassinato no Expresso do Oriente", 1934, null, "Agatha Christie", "Mistério"),
            ("E Não Sobrou Nenhum", 1939, null, "Agatha Christie", "Mistério"),
            ("Morte no Nilo", 1937, null, "Agatha Christie", "Mistério"),
            ("Orgulho e Preconceito", 1813, "9780141439518", "Jane Austen", "Romance"),
            ("Razão e Sensibilidade", 1811, null, "Jane Austen", "Romance"),
            ("Duna", 1965, "9780441172719", "Frank Herbert", "Ficção Científica"),
            ("Messias de Duna", 1969, null, "Frank Herbert", "Ficção Científica"),
            ("Capitães da Areia", 1937, null, "Jorge Amado", "Aventura"),
            ("Gabriela, Cravo e Canela", 1958, null, "Jorge Amado", "Romance")
        };

        private readonly BookHubContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(BookHubContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var hasData = await _context.Genres.AnyAsync(cancellationToken)
                || await _context.Authors.AnyAsync(cancellationToken)
                || await _context.Books.AnyAsync(cancellationToken);

            if (hasData)
            {
                _logger.LogInformation("Seed ignorado: o banco de dados já possui registros.");
                return;
            }

            var genres = Genres.Select(name => new Genre(name)).ToDictionary(g => g.Name);
            var authors = Authors.Select(name => new Author(name)).ToDictionary(a => a.Name);
            var books = Books.Select(b =>
                new Book(b.Title, b.Year, b.Isbn, authors[b.Author].Id, genres[b.Genre].Id)).ToList();

            _context.Genres.AddRange(genres.Values);
            _context.Authors.AddRange(authors.Values);
            _context.Books.AddRange(books);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Seed concluído: {Genres} gêneros, {Authors} autores e {Books} livros cadastrados.",
                genres.Count, authors.Count, books.Count);
        }
    }
}
