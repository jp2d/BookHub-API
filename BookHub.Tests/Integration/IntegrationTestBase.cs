using System.Net;
using System.Net.Http.Json;
using BookHub.Application.Authors;
using BookHub.Application.Books;
using BookHub.Application.Genres;

namespace BookHub.Tests.Integration
{
    [Collection(ApiCollection.Name)]
    public abstract class IntegrationTestBase
    {
        protected IntegrationTestBase(ApiFactory factory)
        {
            Client = factory.CreateClient();
        }

        protected HttpClient Client { get; }

        protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

        protected async Task<GenreResponse> CreateGenreAsync(string? name = null)
        {
            var response = await Client.PostAsJsonAsync("/api/genres", new GenreRequest(name ?? Unique("Genre")));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<GenreResponse>())!;
        }

        protected async Task<AuthorResponse> CreateAuthorAsync(string? name = null)
        {
            var response = await Client.PostAsJsonAsync("/api/authors", new AuthorRequest(name ?? Unique("Author")));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<AuthorResponse>())!;
        }

        protected async Task<BookResponse> CreateBookAsync(
            Guid authorId, Guid genreId, string? title = null, int year = 2000, string? isbn = null)
        {
            var request = new BookRequest(title ?? Unique("Book"), year, isbn, authorId, genreId);
            var response = await Client.PostAsJsonAsync("/api/books", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<BookResponse>())!;
        }

        protected static void AssertProblem(HttpResponseMessage response, HttpStatusCode expected)
        {
            Assert.Equal(expected, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }
    }
}
