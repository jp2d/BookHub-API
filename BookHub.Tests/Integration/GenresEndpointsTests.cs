using System.Net;
using System.Net.Http.Json;
using BookHub.Application.Common;
using BookHub.Application.Genres;

namespace BookHub.Tests.Integration
{
    public class GenresEndpointsTests : IntegrationTestBase
    {
        public GenresEndpointsTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_ReturnsCreatedWithLocation()
        {
            var name = Unique("Genre");

            var response = await Client.PostAsJsonAsync("/api/genres", new GenreRequest(name));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<GenreResponse>();
            Assert.Equal(name, body!.Name);
            Assert.EndsWith($"/api/Genres/{body.Id}", response.Headers.Location!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_WithDuplicateNameIgnoringCase_ReturnsConflict()
        {
            var name = Unique("Genre");
            await CreateGenreAsync(name);

            var response = await Client.PostAsJsonAsync("/api/genres", new GenreRequest(name.ToUpperInvariant()));

            AssertProblem(response, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Create_WithEmptyName_ReturnsBadRequest()
        {
            var response = await Client.PostAsJsonAsync("/api/genres", new GenreRequest(" "));

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_WithMalformedJson_ReturnsBadRequest()
        {
            var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/genres", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenExists_ReturnsGenre()
        {
            var created = await CreateGenreAsync();

            var response = await Client.GetAsync($"/api/genres/{created.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(created, await response.Content.ReadFromJsonAsync<GenreResponse>());
        }

        [Fact]
        public async Task GetById_WhenMissing_ReturnsNotFoundProblem()
        {
            var response = await Client.GetAsync($"/api/genres/{Guid.NewGuid()}");

            AssertProblem(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_ChangesName()
        {
            var created = await CreateGenreAsync();
            var newName = Unique("Renamed");

            var response = await Client.PutAsJsonAsync($"/api/genres/{created.Id}", new GenreRequest(newName));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await Client.GetFromJsonAsync<GenreResponse>($"/api/genres/{created.Id}");
            Assert.Equal(newName, updated!.Name);
        }

        [Fact]
        public async Task Update_WithNameOfAnotherGenre_ReturnsConflict()
        {
            var first = await CreateGenreAsync();
            var second = await CreateGenreAsync();

            var response = await Client.PutAsJsonAsync($"/api/genres/{second.Id}", new GenreRequest(first.Name));

            AssertProblem(response, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Update_WhenMissing_ReturnsNotFound()
        {
            var response = await Client.PutAsJsonAsync($"/api/genres/{Guid.NewGuid()}", new GenreRequest("X"));

            AssertProblem(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesGenre()
        {
            var created = await CreateGenreAsync();

            var delete = await Client.DeleteAsync($"/api/genres/{created.Id}");
            var get = await Client.GetAsync($"/api/genres/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenGenreHasBooks_ReturnsConflictAndKeepsGenre()
        {
            var genre = await CreateGenreAsync();
            var author = await CreateAuthorAsync();
            await CreateBookAsync(author.Id, genre.Id);

            var delete = await Client.DeleteAsync($"/api/genres/{genre.Id}");
            var get = await Client.GetAsync($"/api/genres/{genre.Id}");

            AssertProblem(delete, HttpStatusCode.Conflict);
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        }

        [Fact]
        public async Task List_SupportsSearchAndPagination()
        {
            var token = Unique("Paged");
            await CreateGenreAsync($"{token}-a");
            await CreateGenreAsync($"{token}-b");
            await CreateGenreAsync($"{token}-c");
            await CreateGenreAsync(Unique("Other"));

            var page1 = await Client.GetFromJsonAsync<PagedResult<GenreResponse>>($"/api/genres?search={token}&page=1&pageSize=2");
            var page2 = await Client.GetFromJsonAsync<PagedResult<GenreResponse>>($"/api/genres?search={token.ToUpperInvariant()}&page=2&pageSize=2");

            Assert.Equal(3, page1!.TotalCount);
            Assert.Equal(2, page1.TotalPages);
            Assert.Equal(new[] { $"{token}-a", $"{token}-b" }, page1.Items.Select(g => g.Name));
            Assert.Equal(new[] { $"{token}-c" }, page2!.Items.Select(g => g.Name));
        }
    }
}
