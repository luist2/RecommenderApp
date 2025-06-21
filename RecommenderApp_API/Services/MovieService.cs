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
        private readonly string _apikey;


        public MovieService(AppDbContext context, HttpClient httpClient, IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _apikey = config["TMDB:ApiKey"];
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
            var url = $"https://api.themoviedb.org/3/discover/movie?api_key={_apikey}&sort_by=popularity.desc";

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

        public async Task MarkMovieAsync(Guid userId, int tmdbId, MovieStatus status, CancellationToken cancellationToken)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId, cancellationToken);

            if (movie is null)
            {
                // traer de TMDB si no existe
                var tmdbMovie = await GetMovieByIdAsync(tmdbId, cancellationToken);
                if (tmdbMovie is null) throw new Exception("Movie not found in TMDB");

                movie = new Movie
                {
                    TmdbId = tmdbMovie.id,
                    Title = tmdbMovie.title,
                    Overview = tmdbMovie.overview,
                    PosterUrl = $"https://image.tmdb.org/t/p/w500{tmdbMovie.poster_path}",
                    ReleaseDate = DateTime.TryParse(tmdbMovie.release_date, out var date) ? date : null
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var userMovie = await _context.UserMovies
                .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movie.Id, cancellationToken);

            if (userMovie is null)
            {
                userMovie = new UserMovie
                {
                    UserId = userId,
                    MovieId = movie.Id,
                    Status = status
                };

                _context.UserMovies.Add(userMovie);
            }
            else
            {
                userMovie.Status = status;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }


        private async Task<TmdbMovieResult?> GetMovieByIdAsync(int tmdbId, CancellationToken cancellationToken)
        {
            var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apikey}";

            return await _httpClient.GetFromJsonAsync<TmdbMovieResult>(url, cancellationToken);
        }

    }
}
