using System.Net;

namespace BookHub.Tests.Integration
{
    public class HealthEndpointTests : IntegrationTestBase
    {
        public HealthEndpointTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Health_WhenTheDatabaseIsReachable_ReturnsHealthy()
        {
            var response = await Client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }
    }
}
