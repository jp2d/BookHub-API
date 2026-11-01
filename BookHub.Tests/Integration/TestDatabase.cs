using BookHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookHub.Tests.Integration
{
    public static class TestDatabase
    {
        // Creates a brand new database inside the shared PostgreSQL container and applies the migrations.
        public static async Task<BookHubContext> CreateMigratedAsync(string containerConnectionString)
        {
            var databaseName = $"test_{Guid.NewGuid():N}";

            await using (var admin = new NpgsqlConnection(containerConnectionString))
            {
                await admin.OpenAsync();
                await using var create = new NpgsqlCommand($"CREATE DATABASE {databaseName}", admin);
                await create.ExecuteNonQueryAsync();
            }

            var connectionString = new NpgsqlConnectionStringBuilder(containerConnectionString) { Database = databaseName };
            var options = new DbContextOptionsBuilder<BookHubContext>().UseNpgsql(connectionString.ConnectionString).Options;

            var context = new BookHubContext(options);
            await context.Database.MigrateAsync();
            return context;
        }
    }
}
