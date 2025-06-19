using Microsoft.EntityFrameworkCore;
using RecommenderApp_API.Data;
using RecommenderApp_API.Entities;
using RecommenderApp_API.Models;

namespace RecommenderApp_API.Services
{
    public class MovieService : IMovieService
    {

        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;


        public MovieService(AppDbContext context, HttpClient httpClient, IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
        }
        public async Task<List<Movie>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Obtener IDs de pelicuas ignoradas por el usuario
            var excludeIds = await _context.UserMovies
            .Where(um => um.UserId == userId &&
                        (um.Status == MovieStatus.Watched || um.Status == MovieStatus.Ignored))
            .Select(um => um.Movie.TmdbId)
            .ToListAsync(cancellationToken);

            // Llamar API de TMDB
            var tmdbApiKey = _config["TMDB:ApiKey"];
            var url = $"https://api.themoviedb.org/3/discover/movie?api_key={tmdbApiKey}&sort_by=popularity.desc";

            var response = await _httpClient.GetFromJsonAsync<TmdbDiscoverResponse>(url, cancellationToken);
            if (response == null || response.results == null) return new List<Movie>();

            // Filtrar y mapear los resultados a la entidad Movie
            var movies = response.results
                .Where(m => !excludeIds.Contains(m.id)) // Excluir películas ignoradas
                .Take(10) // Limitar a 10 resultados
                .Select(m => new Movie
                {
                    TmdbId = m.id,
                    Title = m.title,
                    Overview = m.overview,
                    PosterUrl = string.IsNullOrEmpty(m.poster_path) ? null : $"https://image.tmdb.org/t/p/w500{m.poster_path}",
                    ReleaseDate = DateTime.Parse(m.release_date)
                })
                .ToList();

            // Guardar las películas en la base de datos si no existen
            foreach (var movie in movies)
            {
                if (!await _context.Movies.AnyAsync(m => m.TmdbId == movie.TmdbId, cancellationToken))
                {
                    _context.Movies.Add(movie);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return movies;


        }
    }
}
