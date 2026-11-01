using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookHub.Tests.Integration
{
    // The database itself must reject genre names that differ only by case, so a race between two
    // requests that both pass the service's pre-check cannot create duplicates.
    [Collection(ApiCollection.Name)]
    public class GenreNameIndexTests
    {
        private readonly ApiFactory _factory;

        public GenreNameIndexTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("TERROR")]
        [InlineData("terror")]
        [InlineData("TeRrOr")]
        public async Task SavingANameThatDiffersOnlyByCase_IsRejectedAsAConflict(string duplicate)
        {
            await using var context = await TestDatabase.CreateMigratedAsync(_factory.ConnectionString);
            context.Genres.Add(new Genre("Terror"));
            await context.SaveChangesAsync();

            context.Genres.Add(new Genre(duplicate));

            await Assert.ThrowsAsync<ConflictException>(() => context.SaveChangesAsync());
        }

        [Fact]
        public async Task DifferentNames_AreAccepted()
        {
            await using var context = await TestDatabase.CreateMigratedAsync(_factory.ConnectionString);
            context.Genres.AddRange(new Genre("Terror"), new Genre("Suspense"));

            await context.SaveChangesAsync();

            Assert.Equal(2, await context.Genres.CountAsync());
        }
    }
}
