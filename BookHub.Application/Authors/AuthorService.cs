using BookHub.Application.Abstractions;
using BookHub.Application.Common;
using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;

namespace BookHub.Application.Authors
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authors;
        private readonly IUnitOfWork _unitOfWork;

        public AuthorService(IAuthorRepository authors, IUnitOfWork unitOfWork)
        {
            _authors = authors;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<AuthorResponse>> GetPagedAsync(ListQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _authors.SearchAsync(query.Normalize(), cancellationToken);
            return result.Map(a => a.ToResponse());
        }

        public async Task<AuthorResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var author = await _authors.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Autor não encontrado.");

            return author.ToResponse();
        }

        public async Task<AuthorResponse> CreateAsync(AuthorRequest request, CancellationToken cancellationToken = default)
        {
            var author = new Author(request.Name);

            _authors.Add(author);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return author.ToResponse();
        }

        public async Task<AuthorResponse> UpdateAsync(Guid id, AuthorRequest request, CancellationToken cancellationToken = default)
        {
            var author = await _authors.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Autor não encontrado.");

            author.Update(request.Name);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return author.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var author = await _authors.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Autor não encontrado.");

            if (await _authors.HasBooksAsync(id, cancellationToken))
                throw new ConflictException("Não é possível excluir um autor que possui livros.");

            _authors.Remove(author);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
