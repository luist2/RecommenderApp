using Microsoft.EntityFrameworkCore;
using RecommenderApp_API.Data;
using RecommenderApp_API.DTOs;
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
            var excludeIds = await _context.UserMovies
                .Where(um => um.UserId == userId &&
                             (um.Status == MovieStatus.Watched || um.Status == MovieStatus.Ignored))
                .Select(um => um.Movie.TmdbId)
                .ToListAsync(cancellationToken);

            // 1. Obtener películas favoritas del usuario
            var favoriteTmdbIds = await _context.UserMovies
                .Include(um => um.Movie)
                .Where(um => um.UserId == userId && um.Status == MovieStatus.Favorite)
                .Select(um => um.Movie.TmdbId)
                .ToListAsync(cancellationToken);

            List<int> topGenres = [];

            if (favoriteTmdbIds.Any())
            {
                // 2. Obtener los géneros de esas pelis desde TMDB
                var genreCount = new Dictionary<int, int>();

                foreach (var tmdbId in favoriteTmdbIds)
                {
                    var urlDetails = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apikey}";
                    var movieDetails = await _httpClient.GetFromJsonAsync<TmdbMovieDetailResponse>(urlDetails, cancellationToken);
                    if (movieDetails?.genres != null)
                    {
                        foreach (var genre in movieDetails.genres)
                        {
                            if (genreCount.ContainsKey(genre.id))
                                genreCount[genre.id]++;
                            else
                                genreCount[genre.id] = 1;
                        }
                    }
                }

                // 3. Obtener los géneros más frecuentes (top 3 por ejemplo)
                topGenres = genreCount
                    .OrderByDescending(g => g.Value)
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList();
            }

            // 4. Construir URL de recomendación personalizada
            string url;
            if (topGenres.Any())
            {
                var genreParam = string.Join(",", topGenres);
                url = $"https://api.themoviedb.org/3/discover/movie?api_key={_apikey}&sort_by=popularity.desc&with_genres={genreParam}";
            }
            else
            {
                url = $"https://api.themoviedb.org/3/discover/movie?api_key={_apikey}&sort_by=popularity.desc";
            }

            // 5. Llamar a la API
            var response = await _httpClient.GetFromJsonAsync<TmdbDiscoverResponse>(url, cancellationToken);
            if (response == null || response.results == null) return new List<Movie>();

            // 6. Filtrar, mapear y guardar
            var movies = response.results
                .Where(m => !excludeIds.Contains(m.id))
                .Take(10)
                .Select(m => new Movie
                {
                    TmdbId = m.id,
                    Title = m.title,
                    Overview = m.overview,
                    PosterUrl = string.IsNullOrEmpty(m.poster_path) ? null : $"https://image.tmdb.org/t/p/w500{m.poster_path}",
                    ReleaseDate = DateTime.TryParse(m.release_date, out var date) ? date : null
                })
                .ToList();

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


        public async Task<List<UserMovieResponseDTO>> GetUserMoviesAsync(Guid userId, MovieStatus? status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            // Construir la consulta para obtener las películas del usuario
            var query = _context.UserMovies
                .Include(um => um.Movie)
                .Where(um => um.UserId == userId);

            // Filtrar por estado si se proporciona
            if (status.HasValue)
            {
                query = query.Where(um => um.Status == status.Value);
            }

            // Paginación: calcular el número de registros a omitir
            var skip = (pageNumber - 1) * pageSize;

            // Ejecutar la consulta y mapear los resultados a UserMovieResponseDTO
            var result = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(um => new UserMovieResponseDTO
                {
                    TmdbId = um.Movie.TmdbId,
                    Title = um.Movie.Title,
                    PosterUrl = um.Movie.PosterUrl,
                    Overview = um.Movie.Overview,
                    ReleaseDate = um.Movie.ReleaseDate,
                    Status = um.Status
                })
                .ToListAsync(cancellationToken);

            return result;
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
