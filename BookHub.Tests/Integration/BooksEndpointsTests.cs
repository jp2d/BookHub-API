using System.Net;
using System.Net.Http.Json;
using BookHub.Application.Books;
using BookHub.Application.Common;

namespace BookHub.Tests.Integration
{
    public class BooksEndpointsTests : IntegrationTestBase
    {
        public BooksEndpointsTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_ReturnsBookWithAuthorAndGenreNames()
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();
            var request = new BookRequest("Dom Casmurro", 1899, "978-85-359-0277-5", author.Id, genre.Id);

            var response = await Client.PostAsJsonAsync("/api/books", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            var body = await response.Content.ReadFromJsonAsync<BookResponse>();
            Assert.Equal("Dom Casmurro", body!.Title);
            Assert.Equal(1899, body.PublicationYear);
            Assert.Equal("9788535902775", body.Isbn);
            Assert.Equal(author.Name, body.AuthorName);
            Assert.Equal(genre.Name, body.GenreName);
        }

        [Fact]
        public async Task Create_WithUnknownAuthor_ReturnsBadRequest()
        {
            var genre = await CreateGenreAsync();

            var response = await Client.PostAsJsonAsync("/api/books",
                new BookRequest("X", 2000, null, Guid.NewGuid(), genre.Id));

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_WithUnknownGenre_ReturnsBadRequest()
        {
            var author = await CreateAuthorAsync();

            var response = await Client.PostAsJsonAsync("/api/books",
                new BookRequest("X", 2000, null, author.Id, Guid.NewGuid()));

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("", 2000, null)]
        [InlineData("Título", 0, null)]
        [InlineData("Título", 9999, null)]
        [InlineData("Título", 2000, "123")]
        public async Task Create_WithInvalidData_ReturnsBadRequest(string title, int year, string? isbn)
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();

            var response = await Client.PostAsJsonAsync("/api/books",
                new BookRequest(title, year, isbn, author.Id, genre.Id));

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_ReturnsRelationship()
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();
            var book = await CreateBookAsync(author.Id, genre.Id);

            var response = await Client.GetFromJsonAsync<BookResponse>($"/api/books/{book.Id}");

            Assert.Equal(book, response);
        }

        [Fact]
        public async Task GetById_WhenMissing_ReturnsNotFoundProblem()
        {
            var response = await Client.GetAsync($"/api/books/{Guid.NewGuid()}");

            AssertProblem(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_ChangesFieldsAndRelationship()
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();
            var otherAuthor = await CreateAuthorAsync();
            var otherGenre = await CreateGenreAsync();
            var book = await CreateBookAsync(author.Id, genre.Id, isbn: "0306406152");

            var response = await Client.PutAsJsonAsync($"/api/books/{book.Id}",
                new BookRequest("Novo título", 2010, null, otherAuthor.Id, otherGenre.Id));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await Client.GetFromJsonAsync<BookResponse>($"/api/books/{book.Id}");
            Assert.Equal("Novo título", updated!.Title);
            Assert.Equal(2010, updated.PublicationYear);
            Assert.Null(updated.Isbn);
            Assert.Equal(otherAuthor.Name, updated.AuthorName);
            Assert.Equal(otherGenre.Name, updated.GenreName);
        }

        [Fact]
        public async Task Update_WhenMissing_ReturnsNotFound()
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();

            var response = await Client.PutAsJsonAsync($"/api/books/{Guid.NewGuid()}",
                new BookRequest("X", 2000, null, author.Id, genre.Id));

            AssertProblem(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesBook_AndAllowsDeletingAuthorAndGenre()
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();
            var book = await CreateBookAsync(author.Id, genre.Id);

            var delete = await Client.DeleteAsync($"/api/books/{book.Id}");
            var get = await Client.GetAsync($"/api/books/{book.Id}");
            var deleteAuthor = await Client.DeleteAsync($"/api/authors/{author.Id}");
            var deleteGenre = await Client.DeleteAsync($"/api/genres/{genre.Id}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, deleteAuthor.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, deleteGenre.StatusCode);
        }

        [Fact]
        public async Task List_FiltersByAuthorAndGenre()
        {
            var author = await CreateAuthorAsync();
            var otherAuthor = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();
            var otherGenre = await CreateGenreAsync();
            var expected = await CreateBookAsync(author.Id, genre.Id);
            await CreateBookAsync(otherAuthor.Id, genre.Id);
            await CreateBookAsync(author.Id, otherGenre.Id);

            var result = await Client.GetFromJsonAsync<PagedResult<BookResponse>>(
                $"/api/books?authorId={author.Id}&genreId={genre.Id}");

            Assert.Equal(1, result!.TotalCount);
            Assert.Equal(expected.Id, result.Items[0].Id);
        }

        [Fact]
        public async Task List_SearchesByTitleAndSortsByYear()
        {
            var author = await CreateAuthorAsync();
            var genre = await CreateGenreAsync();
            var token = Unique("Sorted");
            await CreateBookAsync(author.Id, genre.Id, $"{token} B", 1990);
            await CreateBookAsync(author.Id, genre.Id, $"{token} A", 2010);
            await CreateBookAsync(author.Id, genre.Id, $"{token} C", 1950);

            var asc = await Client.GetFromJsonAsync<PagedResult<BookResponse>>($"/api/books?search={token}&sortBy=year");
            var desc = await Client.GetFromJsonAsync<PagedResult<BookResponse>>($"/api/books?search={token}&sortBy=year&descending=true");
            var byTitle = await Client.GetFromJsonAsync<PagedResult<BookResponse>>($"/api/books?search={token}");

            Assert.Equal(new[] { 1950, 1990, 2010 }, asc!.Items.Select(b => b.PublicationYear));
            Assert.Equal(new[] { 2010, 1990, 1950 }, desc!.Items.Select(b => b.PublicationYear));
            Assert.Equal(new[] { $"{token} A", $"{token} B", $"{token} C" }, byTitle!.Items.Select(b => b.Title));
        }

        [Fact]
        public async Task List_PageSizeAboveLimit_IsCapped()
        {
            var result = await Client.GetFromJsonAsync<PagedResult<BookResponse>>("/api/books?pageSize=5000&page=0");

            Assert.Equal(Paging.MaxPageSize, result!.PageSize);
            Assert.Equal(1, result.Page);
        }
    }
}
