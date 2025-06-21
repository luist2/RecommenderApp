using RecommenderApp_API.Entities;

namespace RecommenderApp_API.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task MarkMovieAsync(Guid userId, int tmdbId, MovieStatus status, CancellationToken cancellationToken);
    }
}
