using BookHub.Domain.Entities;

namespace BookHub.Application.Genres
{
    public record GenreRequest(string Name);

    public record GenreResponse(Guid Id, string Name);

    internal static class GenreMappings
    {
        public static GenreResponse ToResponse(this Genre genre) => new(genre.Id, genre.Name);
    }
}
