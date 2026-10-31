using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace BookHub.Tests.Integration
{
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
            .Build();

        async Task IAsyncLifetime.InitializeAsync()
        {
            await _postgres.StartAsync();
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
        }
    }

    [CollectionDefinition(Name)]
    public class ApiCollection : ICollectionFixture<ApiFactory>
    {
        public const string Name = "Api";
    }
}
