using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;

namespace BookHub.Tests.Domain
{
    public class GenreTests
    {
        [Fact]
        public void Constructor_WithValidName_TrimsNameAndGeneratesId()
        {
            var genre = new Genre("  Ficção  ");

            Assert.Equal("Ficção", genre.Name);
            Assert.NotEqual(Guid.Empty, genre.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithEmptyName_Throws(string? name)
        {
            Assert.Throws<DomainException>(() => new Genre(name!));
        }

        [Fact]
        public void Constructor_WithNameLongerThanLimit_Throws()
        {
            var name = new string('a', Genre.NameMaxLength + 1);

            Assert.Throws<DomainException>(() => new Genre(name));
        }

        [Fact]
        public void Update_WithValidName_ChangesName()
        {
            var genre = new Genre("Terror");

            genre.Update("Suspense");

            Assert.Equal("Suspense", genre.Name);
        }

        [Fact]
        public void Update_WithEmptyName_ThrowsAndKeepsName()
        {
            var genre = new Genre("Terror");

            Assert.Throws<DomainException>(() => genre.Update(" "));
            Assert.Equal("Terror", genre.Name);
        }
    }
}
