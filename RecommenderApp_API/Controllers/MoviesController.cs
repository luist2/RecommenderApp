using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecommenderApp_API.DTOs;
using RecommenderApp_API.Entities;
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
            if (userIdClaim == null) return Unauthorized();

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

            if (!Enum.IsDefined(typeof(MovieStatus), dto.Status))
            {
                return BadRequest("Invalid movie status.");
            }

            await _movieService.MarkMovieAsync(userId, tmdbId, dto.Status, cancellationToken);

            return Ok(new { message = "Movie marked successfully." });

        }

        [Authorize]
        [HttpGet("user/movies")]
        public async Task<IActionResult> GetUserMovies(
            [FromQuery] MovieStatus? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // Asegurarse que el user está autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            // Checkear que el claim no sea nulo y convertirlo a Guid
            var userId = Guid.Parse(userIdClaim.Value);

            var userMovies = await _movieService.GetUserMoviesAsync(userId, status, pageNumber, pageSize, cancellationToken);

            return Ok(userMovies);
        }
    }
}

