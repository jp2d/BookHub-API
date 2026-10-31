using BookHub.Application.Common;
using BookHub.Application.Genres;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genres;

        public GenresController(IGenreService genres)
        {
            _genres = genres;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<GenreResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<GenreResponse>>> GetAll([FromQuery] ListQuery query, CancellationToken cancellationToken) =>
            Ok(await _genres.GetPagedAsync(query, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenreResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
            Ok(await _genres.GetByIdAsync(id, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<GenreResponse>> Create(GenreRequest request, CancellationToken cancellationToken)
        {
            var created = await _genres.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<GenreResponse>> Update(Guid id, GenreRequest request, CancellationToken cancellationToken) =>
            Ok(await _genres.UpdateAsync(id, request, cancellationToken));

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _genres.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
