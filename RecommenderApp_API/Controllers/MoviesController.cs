using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecommenderApp_API.DTOs;
using RecommenderApp_API.Services;
using System.Security.Claims;

namespace RecommenderApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }


        [Authorize]
        [HttpGet("recommend")]
        public async Task<IActionResult> GetRecommendations(CancellationToken cancellationToken = default)
        {
            // Asegurarse que el user está autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(userIdClaim == null) return Unauthorized();

            // Checkear que el claim no sea nulo y convertirlo a Guid
            var userId = Guid.Parse(userIdClaim.Value);

            var movies = await _movieService.GetRecommendationsAsync(userId, cancellationToken);

            return Ok(movies);
        }

        [Authorize]
        [HttpPost("{tmdbId}/mark")]
        public async Task<IActionResult> MarkMovie(int tmdbId, [FromBody] UserMovieCreateDTO dto, CancellationToken cancellationToken)
        {
            // Asegurarse que el user está autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            // Checkear que el claim no sea nulo y convertirlo a Guid
            var userId = Guid.Parse(userIdClaim.Value);

            await _movieService.MarkMovieAsync(userId, tmdbId, dto.Status, cancellationToken);

            return Ok(new { message = "Movie marked successfully." });

        }
    }
}
