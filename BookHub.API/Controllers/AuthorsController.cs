using BookHub.Application.Authors;
using BookHub.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authors;

        public AuthorsController(IAuthorService authors)
        {
            _authors = authors;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AuthorResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<AuthorResponse>>> GetAll([FromQuery] ListQuery query, CancellationToken cancellationToken) =>
            Ok(await _authors.GetPagedAsync(query, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
            Ok(await _authors.GetByIdAsync(id, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthorResponse>> Create(AuthorRequest request, CancellationToken cancellationToken)
        {
            var created = await _authors.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorResponse>> Update(Guid id, AuthorRequest request, CancellationToken cancellationToken) =>
            Ok(await _authors.UpdateAsync(id, request, cancellationToken));

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _authors.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
