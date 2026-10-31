using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookHub.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BookHubContext>
    {
        private const string DefaultConnection =
            "Host=localhost;Port=5432;Database=bookhubdb;Username=bookhub;Password=bookhubpwd";

        public BookHubContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? DefaultConnection;

            var options = new DbContextOptionsBuilder<BookHubContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new BookHubContext(options);
        }
    }
}
