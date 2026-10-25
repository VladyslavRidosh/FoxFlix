
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Foxflix.Web.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FoxflixAPI");
    }

    public async Task<IActionResult> Index()
    {
        var movies = new List<MovieDto>();

        try
        {
            movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>(
                "api/Movies") ?? new List<MovieDto>();
        }
        catch (HttpRequestException)
        {
            ViewBag.HomeError = "Не вдалося підключитися до Foxflix API.";
        }
        catch (JsonException)
        {
            ViewBag.HomeError = "Не вдалося прочитати список фільмів.";
        }

        var continueWatching = new List<ContinueWatchingDto>();

        if (User.Identity?.IsAuthenticated == true)
        {
            var token = User.FindFirst("access_token")?.Value;

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    using var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        "api/watch-history");

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    using var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var history = await response.Content
                            .ReadFromJsonAsync<List<WatchHistoryDto>>();

                        if (history is not null)
                        {
                            continueWatching = history
                                .Where(x => !x.Completed && x.Movie is not null)
                                .Select(x => new ContinueWatchingDto
                                {
                                    MovieId = x.MovieId,
                                    Title = x.Movie!.Title,
                                    PosterUrl = x.Movie.PosterUrl,
                                    PosterPath = x.Movie.PosterPath,
                                    PositionSeconds = x.PositionSeconds,
                                    DurationSeconds = x.DurationSeconds,
                                    UpdatedAt = x.UpdatedAt
                                })
                                .ToList();
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    continueWatching = new List<ContinueWatchingDto>();
                }
                catch (JsonException)
                {
                    continueWatching = new List<ContinueWatchingDto>();
                }
            }
        }

        var popularTmdb = new List<TmdbHomeMovieDto>();
        var newTmdb = new List<TmdbHomeMovieDto>();
        var upcomingTmdb = new List<TmdbHomeMovieDto>();

        try
        {
            var response = await _httpClient.GetFromJsonAsync<TmdbHomeResponseDto>(
                "api/tmdb/popular?page=1");

            popularTmdb = response?.Results ?? new List<TmdbHomeMovieDto>();
        }
        catch (HttpRequestException)
        {
            popularTmdb = new List<TmdbHomeMovieDto>();
        }
        catch (JsonException)
        {
            popularTmdb = new List<TmdbHomeMovieDto>();
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<TmdbHomeResponseDto>(
                "api/tmdb/now-playing?page=1");

            newTmdb = response?.Results ?? new List<TmdbHomeMovieDto>();
        }
        catch (HttpRequestException)
        {
            newTmdb = new List<TmdbHomeMovieDto>();
        }
        catch (JsonException)
        {
            newTmdb = new List<TmdbHomeMovieDto>();
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<TmdbHomeResponseDto>(
                "api/tmdb/upcoming?page=1");

            upcomingTmdb = response?.Results ?? new List<TmdbHomeMovieDto>();
        }
        catch (HttpRequestException)
        {
            upcomingTmdb = new List<TmdbHomeMovieDto>();
        }
        catch (JsonException)
        {
            upcomingTmdb = new List<TmdbHomeMovieDto>();
        }

        ViewBag.TmdbPopular = popularTmdb;
        ViewBag.TmdbNew = newTmdb;
        ViewBag.TmdbUpcoming = upcomingTmdb;
        ViewBag.ContinueWatching = continueWatching;

        return View(movies);
    }

    public async Task<IActionResult> Catalog(
        string? query,
        int? genreId,
        int? year,
        double? minimumRating,
        string sortBy = "popularity.desc",
        int page = 1)
    {
        page = Math.Clamp(page, 1, 500);

        var parameters = new List<string>
        {
            $"page={page}",
            $"sortBy={Uri.EscapeDataString(sortBy)}"
        };

        var isSearch = !string.IsNullOrWhiteSpace(query);

        if (isSearch)
        {
            parameters.Add($"query={Uri.EscapeDataString(query!.Trim())}");
        }
        else
        {
            if (genreId.HasValue)
            {
                parameters.Add($"genreId={genreId.Value}");
            }

            if (year.HasValue)
            {
                parameters.Add($"year={year.Value}");
            }

            if (minimumRating.HasValue)
            {
                parameters.Add(
                    "minimumRating=" +
                    minimumRating.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        var endpoint = isSearch
            ? "api/tmdb/search?"
            : "api/tmdb/discover?";

        var url = endpoint + string.Join("&", parameters);
        var model = new TmdbCatalogResponseDto();

        try
        {
            using var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                model = await response.Content
                    .ReadFromJsonAsync<TmdbCatalogResponseDto>()
                    ?? new TmdbCatalogResponseDto();
            }
            else
            {
                ViewBag.CatalogError =
                    $"TMDB повернув помилку: {(int)response.StatusCode}";
            }
        }
        catch (HttpRequestException)
        {
            ViewBag.CatalogError = "Не вдалося підключитися до Foxflix API.";
        }
        catch (JsonException)
        {
            ViewBag.CatalogError = "Не вдалося прочитати відповідь TMDB.";
        }

        ViewBag.Query = query;
        ViewBag.GenreId = genreId;
        ViewBag.Year = year;
        ViewBag.MinimumRating = minimumRating;
        ViewBag.SortBy = sortBy;

        return View(model);
    }

    public async Task<IActionResult> MyList()
    {
        var favorites = new List<FavoriteDto>();

        if (User.Identity?.IsAuthenticated != true)
        {
            return View(favorites);
        }

        var token = User.FindFirst("access_token")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            ViewBag.MyListError =
                "Сесія авторизації не знайдена. Увійди в акаунт повторно.";

            return View(favorites);
        }

        try
        {
            using var localRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/favorites");

            localRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var localResponse = await _httpClient.SendAsync(localRequest);

            if (localResponse.IsSuccessStatusCode)
            {
                var localFavorites = await localResponse.Content
                    .ReadFromJsonAsync<List<FavoriteResponseDto>>();

                if (localFavorites is not null)
                {
                    favorites.AddRange(
                        localFavorites
                            .Where(x => x.Movie is not null)
                            .Select(x => new FavoriteDto
                            {
                                MovieId = x.MovieId,
                                Title = x.Movie!.Title,
                                PosterUrl = x.Movie.PosterUrl,
                                PosterPath = x.Movie.PosterPath,
                                ReleaseYear = x.Movie.ReleaseYear,
                                Rating = x.Movie.Rating,
                                IsTmdb = false
                            }));
                }
            }
            else
            {
                ViewBag.LocalFavoritesError =
                    $"Не вдалося завантажити локальне обране: {(int)localResponse.StatusCode}.";
            }

            using var tmdbRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/tmdb-favorites");

            tmdbRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var tmdbResponse = await _httpClient.SendAsync(tmdbRequest);

            if (tmdbResponse.IsSuccessStatusCode)
            {
                var tmdbFavorites = await tmdbResponse.Content
                    .ReadFromJsonAsync<List<TmdbFavoriteDto>>();

                if (tmdbFavorites is not null)
                {
                    favorites.AddRange(
                        tmdbFavorites.Select(x => new FavoriteDto
                        {
                            MovieId = x.TmdbMovieId,
                            Title = x.Title,
                            PosterUrl = x.PosterUrl ?? string.Empty,
                            PosterPath = null,
                            ReleaseYear = ParseReleaseYear(x.ReleaseDate),
                            Rating = x.Rating,
                            IsTmdb = true
                        }));
                }
            }
            else
            {
                ViewBag.TmdbFavoritesError =
                    $"Не вдалося завантажити TMDB-обране: {(int)tmdbResponse.StatusCode}.";
            }
        }
        catch (HttpRequestException)
        {
            ViewBag.MyListError =
                "Не вдалося підключитися до Foxflix API.";
        }
        catch (JsonException)
        {
            ViewBag.MyListError =
                "Не вдалося прочитати список обраного.";
        }

        return View(favorites);
    }

    private static int ParseReleaseYear(string? releaseDate)
    {
        if (DateTime.TryParse(
            releaseDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            return date.Year;
        }

        return 0;
    }

    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string PosterUrl { get; set; } = string.Empty;
        public string BackdropUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int DurationMinutes { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int AgeRating { get; set; }
        public string? PosterPath { get; set; }
        public string? VideoPath { get; set; }
    }

    public class WatchHistoryDto
    {
        public int MovieId { get; set; }
        public double PositionSeconds { get; set; }
        public double DurationSeconds { get; set; }
        public bool Completed { get; set; }
        public DateTime UpdatedAt { get; set; }
        public MovieDto Movie { get; set; } = null!;
    }

    public class ContinueWatchingDto
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public double PositionSeconds { get; set; }
        public double DurationSeconds { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FavoriteResponseDto
    {
        public int MovieId { get; set; }
        public MovieDto Movie { get; set; } = null!;
    }

    public class FavoriteDto
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public int ReleaseYear { get; set; }
        public double Rating { get; set; }
        public bool IsTmdb { get; set; }
    }

    public class TmdbFavoriteDto
    {
        public int TmdbMovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? ReleaseDate { get; set; }
        public string? PosterUrl { get; set; }
        public double Rating { get; set; }
    }

    public class TmdbCatalogResponseDto
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
        public List<TmdbCatalogMovieDto> Results { get; set; } = [];
    }

    public class TmdbCatalogMovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? ReleaseDate { get; set; }
        public string? FullPosterUrl { get; set; }
        public string? FullBackdropUrl { get; set; }
        public double Rating { get; set; }
        public int VoteCount { get; set; }
        public List<int> GenreIds { get; set; } = [];
        public string OriginalLanguage { get; set; } = string.Empty;
        public double Popularity { get; set; }
    }

    public class TmdbHomeResponseDto
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
        public List<TmdbHomeMovieDto> Results { get; set; } = [];
    }

    public class TmdbHomeMovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? ReleaseDate { get; set; }
        public string? FullPosterUrl { get; set; }
        public string? FullBackdropUrl { get; set; }
        public double VoteAverage { get; set; }
        public double Rating => VoteAverage;
        public int VoteCount { get; set; }
        public List<int> GenreIds { get; set; } = [];
        public string OriginalLanguage { get; set; } = string.Empty;
        public double Popularity { get; set; }
    }
}
