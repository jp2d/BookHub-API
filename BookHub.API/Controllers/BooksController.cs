using BookHub.Application.Books;
using BookHub.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _books;

        public BooksController(IBookService books)
        {
            _books = books;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<BookResponse>>> GetAll([FromQuery] BookListQuery query, CancellationToken cancellationToken) =>
            Ok(await _books.GetPagedAsync(query, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
            Ok(await _books.GetByIdAsync(id, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookResponse>> Create(BookRequest request, CancellationToken cancellationToken)
        {
            var created = await _books.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookResponse>> Update(Guid id, BookRequest request, CancellationToken cancellationToken) =>
            Ok(await _books.UpdateAsync(id, request, cancellationToken));

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _books.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
