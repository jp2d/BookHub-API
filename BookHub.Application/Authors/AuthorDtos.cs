using BookHub.Domain.Entities;

namespace BookHub.Application.Authors
{
    public record AuthorRequest(string Name);

    public record AuthorResponse(Guid Id, string Name);

    internal static class AuthorMappings
    {
        public static AuthorResponse ToResponse(this Author author) => new(author.Id, author.Name);
    }
}
