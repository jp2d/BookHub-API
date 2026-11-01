using BookHub.Domain.Entities;
using BookHub.Infrastructure;
using BookHub.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookHub.Tests.Integration
{
    [Collection(ApiCollection.Name)]
    public class DatabaseSeederTests
    {
        private readonly ApiFactory _factory;

        public DatabaseSeederTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private Task<BookHubContext> CreateEmptyDatabaseAsync() => TestDatabase.CreateMigratedAsync(_factory.ConnectionString);

        private static DatabaseSeeder NewSeeder(BookHubContext context) =>
            new(context, NullLogger<DatabaseSeeder>.Instance);

        [Fact]
        public async Task Seed_OnEmptyDatabase_PopulatesGenresAuthorsAndBooks()
        {
            await using var context = await CreateEmptyDatabaseAsync();

            await NewSeeder(context).SeedAsync();

            Assert.True(await context.Genres.CountAsync() > 0);
            Assert.True(await context.Authors.CountAsync() > 0);
            Assert.True(await context.Books.CountAsync() > 0);
            Assert.False(await context.Books.AnyAsync(b => b.Author == null || b.Genre == null));
        }

        [Fact]
        public async Task Seed_RunTwice_DoesNotDuplicateData()
        {
            await using var context = await CreateEmptyDatabaseAsync();
            var seeder = NewSeeder(context);

            await seeder.SeedAsync();
            var genres = await context.Genres.CountAsync();
            var authors = await context.Authors.CountAsync();
            var books = await context.Books.CountAsync();

            await seeder.SeedAsync();

            Assert.Equal(genres, await context.Genres.CountAsync());
            Assert.Equal(authors, await context.Authors.CountAsync());
            Assert.Equal(books, await context.Books.CountAsync());
        }

        [Fact]
        public async Task Seed_WhenDatabaseAlreadyHasData_DoesNothing()
        {
            await using var context = await CreateEmptyDatabaseAsync();
            context.Genres.Add(new Genre("Meu gênero"));
            await context.SaveChangesAsync();

            await NewSeeder(context).SeedAsync();

            Assert.Equal(1, await context.Genres.CountAsync());
            Assert.Equal(0, await context.Authors.CountAsync());
            Assert.Equal(0, await context.Books.CountAsync());
        }
    }
}
