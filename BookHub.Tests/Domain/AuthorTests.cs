using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;

namespace BookHub.Tests.Domain
{
    public class AuthorTests
    {
        [Fact]
        public void Constructor_WithValidName_TrimsNameAndGeneratesId()
        {
            var author = new Author("  Machado de Assis ");

            Assert.Equal("Machado de Assis", author.Name);
            Assert.NotEqual(Guid.Empty, author.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithEmptyName_Throws(string? name)
        {
            Assert.Throws<DomainException>(() => new Author(name!));
        }

        [Fact]
        public void Constructor_WithNameLongerThanLimit_Throws()
        {
            var name = new string('a', Author.NameMaxLength + 1);

            Assert.Throws<DomainException>(() => new Author(name));
        }

        [Fact]
        public void Update_WithValidName_ChangesName()
        {
            var author = new Author("Machado");

            author.Update("Clarice Lispector");

            Assert.Equal("Clarice Lispector", author.Name);
        }
    }
}
