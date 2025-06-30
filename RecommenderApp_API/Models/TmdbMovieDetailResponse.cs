namespace RecommenderApp_API.Models
{
    public class TmdbMovieDetailResponse
    {
        public List<TmdbGenre> genres { get; set; } = new();
    }

    public class TmdbGenre
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
    }
}
