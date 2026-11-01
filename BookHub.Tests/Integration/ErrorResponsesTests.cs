using System.Net;

namespace BookHub.Tests.Integration
{
    // Every error the API returns, including the ones raised by the framework, is a problem+json document.
    public class ErrorResponsesTests : IntegrationTestBase
    {
        public ErrorResponsesTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task MalformedJson_ReturnsProblemJson()
        {
            var response = await Client.PostAsync("/api/genres", new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task InvalidQueryValue_ReturnsProblemJson()
        {
            var response = await Client.GetAsync("/api/books?authorId=not-a-guid");

            AssertProblem(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UnsupportedMediaType_ReturnsProblemJson()
        {
            var response = await Client.PostAsync("/api/genres", new StringContent("x", System.Text.Encoding.UTF8, "text/plain"));

            AssertProblem(response, HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async Task UnknownRoute_ReturnsProblemJson()
        {
            var response = await Client.GetAsync("/api/does-not-exist");

            AssertProblem(response, HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("/api/genres?page=2147483647&pageSize=100")]
        [InlineData("/api/authors?page=2147483647&pageSize=100")]
        [InlineData("/api/books?page=2147483647&pageSize=100")]
        public async Task HugePageNumber_ReturnsAnEmptyPageInsteadOfFailing(string url)
        {
            var response = await Client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
