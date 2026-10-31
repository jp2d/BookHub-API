using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Application.Exceptions;
using BookHub.Application.Genres;
using BookHub.Domain.Entities;
using BookHub.Domain.Exceptions;
using NSubstitute;

namespace BookHub.Tests.Application
{
    public class GenreServiceTests
    {
        private readonly IGenreRepository _genres = Substitute.For<IGenreRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly GenreService _service;

        public GenreServiceTests()
        {
            _service = new GenreService(_genres, _unitOfWork);
        }

        [Fact]
        public async Task Create_WithNewName_AddsAndSaves()
        {
            _genres.ExistsByNameAsync("Ficção", null, Arg.Any<CancellationToken>()).Returns(false);

            var result = await _service.CreateAsync(new GenreRequest(" Ficção "));

            Assert.Equal("Ficção", result.Name);
            _genres.Received(1).Add(Arg.Is<Genre>(g => g.Name == "Ficção"));
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Create_WithExistingName_ThrowsConflictAndDoesNotSave()
        {
            _genres.ExistsByNameAsync("Ficção", null, Arg.Any<CancellationToken>()).Returns(true);

            await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(new GenreRequest("Ficção")));

            _genres.DidNotReceive().Add(Arg.Any<Genre>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Create_WithEmptyName_ThrowsDomainException()
        {
            await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(new GenreRequest(" ")));

            _genres.DidNotReceive().Add(Arg.Any<Genre>());
        }

        [Fact]
        public async Task GetById_WhenMissing_ThrowsNotFound()
        {
            _genres.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Genre?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsResponse()
        {
            var genre = new Genre("Terror");
            _genres.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);

            var result = await _service.GetByIdAsync(genre.Id);

            Assert.Equal(new GenreResponse(genre.Id, "Terror"), result);
        }

        [Fact]
        public async Task Update_WhenMissing_ThrowsNotFound()
        {
            _genres.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Genre?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new GenreRequest("X")));
        }

        [Fact]
        public async Task Update_WithNameUsedByAnotherGenre_ThrowsConflictAndDoesNotSave()
        {
            var genre = new Genre("Terror");
            _genres.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);
            _genres.ExistsByNameAsync("Ficção", genre.Id, Arg.Any<CancellationToken>()).Returns(true);

            await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(genre.Id, new GenreRequest("Ficção")));

            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Update_WithValidName_ChangesAndSaves()
        {
            var genre = new Genre("Terror");
            _genres.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);
            _genres.ExistsByNameAsync("Suspense", genre.Id, Arg.Any<CancellationToken>()).Returns(false);

            var result = await _service.UpdateAsync(genre.Id, new GenreRequest("Suspense"));

            Assert.Equal("Suspense", result.Name);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Delete_WhenMissing_ThrowsNotFound()
        {
            _genres.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Genre?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Delete_WhenGenreHasBooks_ThrowsConflictAndDoesNotRemove()
        {
            var genre = new Genre("Terror");
            _genres.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);
            _genres.HasBooksAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(true);

            await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(genre.Id));

            _genres.DidNotReceive().Remove(Arg.Any<Genre>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Delete_WhenGenreHasNoBooks_RemovesAndSaves()
        {
            var genre = new Genre("Terror");
            _genres.GetByIdAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(genre);
            _genres.HasBooksAsync(genre.Id, Arg.Any<CancellationToken>()).Returns(false);

            await _service.DeleteAsync(genre.Id);

            _genres.Received(1).Remove(genre);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetPaged_NormalizesQueryBeforeCallingRepository()
        {
            _genres.SearchAsync(Arg.Any<ListQuery>(), Arg.Any<CancellationToken>())
                .Returns(new PagedResult<Genre>(new List<Genre> { new("Terror") }, 1, 100, 1));

            var result = await _service.GetPagedAsync(new ListQuery("  ter ", Page: 0, PageSize: 5000));

            await _genres.Received(1).SearchAsync(
                Arg.Is<ListQuery>(q => q.Search == "ter" && q.Page == 1 && q.PageSize == Paging.MaxPageSize),
                Arg.Any<CancellationToken>());
            Assert.Single(result.Items);
            Assert.Equal("Terror", result.Items[0].Name);
        }
    }
}
