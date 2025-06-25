using RecommenderApp_API.Entities;

namespace RecommenderApp_API.DTOs
{
    public class UserMovieResponseDTO
    {
        public int TmdbId { get; set; }
        public string Title { get; set; } = null!;
        public string? PosterUrl { get; set; }
        public string? Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public MovieStatus Status { get; set; }
    }
}
