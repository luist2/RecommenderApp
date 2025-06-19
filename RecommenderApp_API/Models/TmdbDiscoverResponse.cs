namespace RecommenderApp_API.Models
{
    public class TmdbDiscoverResponse
    {
        public List<TmdbMovieResult> results { get; set; } = new();
    }

    public class TmdbMovieResult
    {
        public int id { get; set; }
        public string title { get; set; } = null!;
        public string overview { get; set; } = null!;
        public string poster_path { get; set; } = null!;
        public string release_date { get; set; } = null!;
    }

}
