using BookHub.Domain.Entities;

namespace BookHub.Application.Books
{
    public record BookRequest(string Title, int PublicationYear, string? Isbn, Guid AuthorId, Guid GenreId);

    public record BookResponse(
        Guid Id,
        string Title,
        int PublicationYear,
        string? Isbn,
        Guid AuthorId,
        string AuthorName,
        Guid GenreId,
        string GenreName);

    internal static class BookMappings
    {
        public static BookResponse ToResponse(this Book book, string authorName, string genreName) =>
            new(book.Id, book.Title, book.PublicationYear, book.Isbn,
                book.AuthorId, authorName, book.GenreId, genreName);

        public static BookResponse ToResponse(this Book book) =>
            book.ToResponse(book.Author!.Name, book.Genre!.Name);
    }
}
