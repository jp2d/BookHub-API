using System.Net;

namespace BookHub.Tests.Integration
{
    public class CorsTests : IntegrationTestBase
    {
        public CorsTests(ApiFactory factory) : base(factory)
        {
        }

        private async Task<HttpResponseMessage> GetWithOriginAsync(string origin)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/genres");
            request.Headers.Add("Origin", origin);
            return await Client.SendAsync(request);
        }

        [Fact]
        public async Task Request_FromAllowedOrigin_ReceivesCorsHeader()
        {
            var response = await GetWithOriginAsync("http://localhost:5173");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
            Assert.Equal("http://localhost:5173", values!.Single());
        }

        [Fact]
        public async Task Request_FromUnlistedOrigin_DoesNotReceiveCorsHeader()
        {
            var response = await GetWithOriginAsync("http://evil.example");

            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }
}
