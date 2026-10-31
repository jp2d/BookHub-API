using System.Net;
using System.Net.Http.Json;
using BookHub.Application.Authors;
using BookHub.Application.Common;

namespace BookHub.Tests.Integration
{
    public class AuthorsEndpointsTests : IntegrationTestBase
    {
        public AuthorsEndpointsTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_ReturnsCreatedWithLocation()
        {
            var name = Unique("Author");

            var response = await Client.PostAsJsonAsync("/api/authors", new AuthorRequest(name));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<AuthorResponse>();
            Assert.Equal(name, body!.Name);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task Create_WithDuplicateName_IsAllowed()
        {
            var name = Unique("Author");

            var first = await CreateAuthorAsync(name);
            var second = await CreateAuthorAsync(name);

            Assert.NotEqual(first.Id, second.Id);
        }

        [Fact]
        public async Task Create_WithEmptyName_ReturnsBadRequest()
        {
            var response = await Client.PostAsJsonAsync("/api/authors", new AuthorRequest(""));

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_WhenMissing_ReturnsNotFoundProblem()
        {
            var response = await Client.GetAsync($"/api/authors/{Guid.NewGuid()}");

            AssertProblem(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_ChangesName()
        {
            var created = await CreateAuthorAsync();
            var newName = Unique("Renamed");

            var response = await Client.PutAsJsonAsync($"/api/authors/{created.Id}", new AuthorRequest(newName));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await Client.GetFromJsonAsync<AuthorResponse>($"/api/authors/{created.Id}");
            Assert.Equal(newName, updated!.Name);
        }

        [Fact]
        public async Task Delete_RemovesAuthor()
        {
            var created = await CreateAuthorAsync();

            var delete = await Client.DeleteAsync($"/api/authors/{created.Id}");
            var get = await Client.GetAsync($"/api/authors/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenAuthorHasBooks_ReturnsConflictAndKeepsAuthor()
        {
            var genre = await CreateGenreAsync();
            var author = await CreateAuthorAsync();
            await CreateBookAsync(author.Id, genre.Id);

            var delete = await Client.DeleteAsync($"/api/authors/{author.Id}");
            var get = await Client.GetAsync($"/api/authors/{author.Id}");

            AssertProblem(delete, HttpStatusCode.Conflict);
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        }

        [Fact]
        public async Task List_SupportsSearch()
        {
            var token = Unique("Searchable");
            await CreateAuthorAsync($"{token} Um");
            await CreateAuthorAsync($"{token} Dois");
            await CreateAuthorAsync(Unique("Outro"));

            var result = await Client.GetFromJsonAsync<PagedResult<AuthorResponse>>($"/api/authors?search={token}");

            Assert.Equal(2, result!.TotalCount);
            Assert.All(result.Items, a => Assert.StartsWith(token, a.Name));
        }
    }
}
