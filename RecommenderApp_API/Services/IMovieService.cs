using RecommenderApp_API.Entities;

namespace RecommenderApp_API.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
