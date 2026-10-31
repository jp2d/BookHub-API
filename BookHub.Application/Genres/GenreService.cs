using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;

namespace BookHub.Application.Genres
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genres;
        private readonly IUnitOfWork _unitOfWork;

        public GenreService(IGenreRepository genres, IUnitOfWork unitOfWork)
        {
            _genres = genres;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GenreResponse>> GetPagedAsync(ListQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _genres.SearchAsync(query.Normalize(), cancellationToken);
            return result.Map(g => g.ToResponse());
        }

        public async Task<GenreResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var genre = await _genres.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Gênero não encontrado.");

            return genre.ToResponse();
        }

        public async Task<GenreResponse> CreateAsync(GenreRequest request, CancellationToken cancellationToken = default)
        {
            var genre = new Genre(request.Name);

            if (await _genres.ExistsByNameAsync(genre.Name, null, cancellationToken))
                throw new ConflictException($"Já existe um gênero com o nome '{genre.Name}'.");

            _genres.Add(genre);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return genre.ToResponse();
        }

        public async Task<GenreResponse> UpdateAsync(Guid id, GenreRequest request, CancellationToken cancellationToken = default)
        {
            var genre = await _genres.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Gênero não encontrado.");

            genre.Update(request.Name);

            if (await _genres.ExistsByNameAsync(genre.Name, genre.Id, cancellationToken))
                throw new ConflictException($"Já existe um gênero com o nome '{genre.Name}'.");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return genre.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var genre = await _genres.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Gênero não encontrado.");

            if (await _genres.HasBooksAsync(id, cancellationToken))
                throw new ConflictException("Não é possível excluir um gênero que possui livros.");

            _genres.Remove(genre);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
