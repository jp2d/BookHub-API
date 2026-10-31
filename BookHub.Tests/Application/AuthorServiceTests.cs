using BookHub.Application.Abstractions;
using BookHub.Application.Authors;
using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;
using NSubstitute;

namespace BookHub.Tests.Application
{
    public class AuthorServiceTests
    {
        private readonly IAuthorRepository _authors = Substitute.For<IAuthorRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly AuthorService _service;

        public AuthorServiceTests()
        {
            _service = new AuthorService(_authors, _unitOfWork);
        }

        [Fact]
        public async Task Create_WithValidName_AddsAndSaves()
        {
            var result = await _service.CreateAsync(new AuthorRequest(" Machado "));

            Assert.Equal("Machado", result.Name);
            _authors.Received(1).Add(Arg.Is<Author>(a => a.Name == "Machado"));
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Create_WithDuplicatedName_IsAllowed()
        {
            await _service.CreateAsync(new AuthorRequest("João Silva"));
            await _service.CreateAsync(new AuthorRequest("João Silva"));

            _authors.Received(2).Add(Arg.Any<Author>());
        }

        [Fact]
        public async Task Create_WithEmptyName_ThrowsDomainException()
        {
            await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(new AuthorRequest("")));

            _authors.DidNotReceive().Add(Arg.Any<Author>());
        }

        [Fact]
        public async Task GetById_WhenMissing_ThrowsNotFound()
        {
            _authors.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Author?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Update_WhenMissing_ThrowsNotFound()
        {
            _authors.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Author?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new AuthorRequest("X")));
        }

        [Fact]
        public async Task Update_WithValidName_ChangesAndSaves()
        {
            var author = new Author("Machado");
            _authors.GetByIdAsync(author.Id, Arg.Any<CancellationToken>()).Returns(author);

            var result = await _service.UpdateAsync(author.Id, new AuthorRequest("Clarice"));

            Assert.Equal("Clarice", result.Name);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Delete_WhenAuthorHasBooks_ThrowsConflictAndDoesNotRemove()
        {
            var author = new Author("Machado");
            _authors.GetByIdAsync(author.Id, Arg.Any<CancellationToken>()).Returns(author);
            _authors.HasBooksAsync(author.Id, Arg.Any<CancellationToken>()).Returns(true);

            await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(author.Id));

            _authors.DidNotReceive().Remove(Arg.Any<Author>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Delete_WhenAuthorHasNoBooks_RemovesAndSaves()
        {
            var author = new Author("Machado");
            _authors.GetByIdAsync(author.Id, Arg.Any<CancellationToken>()).Returns(author);
            _authors.HasBooksAsync(author.Id, Arg.Any<CancellationToken>()).Returns(false);

            await _service.DeleteAsync(author.Id);

            _authors.Received(1).Remove(author);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
