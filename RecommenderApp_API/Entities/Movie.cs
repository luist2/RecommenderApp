namespace RecommenderApp_API.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = null!;
        public string? Overview { get; set; }
        public string? PosterUrl { get; set; }
        public string? Genres { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public ICollection<UserMovie> UserMovies { get; set; } = new List<UserMovie>();
    }
}
