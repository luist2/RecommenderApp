using RecommenderApp_API.DTOs;
using RecommenderApp_API.Entities;

namespace RecommenderApp_API.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task MarkMovieAsync(Guid userId, int tmdbId, MovieStatus status, CancellationToken cancellationToken);
        Task<List<UserMovieResponseDTO>> GetUserMoviesAsync(Guid userId, MovieStatus? status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    }
}
