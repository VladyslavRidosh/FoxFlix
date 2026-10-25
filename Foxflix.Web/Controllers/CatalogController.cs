using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Foxflix.Web.Controllers;

public class CatalogController : Controller
{
    private readonly HttpClient _httpClient;

    public CatalogController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FoxflixAPI");
    }

    public async Task<IActionResult> Index(
        string? genre,
        int? year,
        double? minRating,
        bool popular = false,
        bool newest = false,
        string? sort = null,
        string? query = null,
        int? genreId = null,
        double? minimumRating = null,
        string? sortBy = null,
        int page = 1,
        string source = "tmdb")
    {
        if (source == "tmdb")
        {
            return await TmdbCatalog(
                query,
                genreId,
                year,
                minimumRating,
                sortBy ?? "popularity.desc",
                page);
        }

        List<MovieDto> movies;

        var hasFilters =
            !string.IsNullOrWhiteSpace(genre) ||
            year.HasValue ||
            minRating.HasValue ||
            !string.IsNullOrWhiteSpace(sort);

        try
        {
            if (popular && hasFilters)
            {
                movies = await GetFilteredMovies(
                    genre, year, minRating, sort);
            }
            else if (popular)
            {
                movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>(
                    "api/Movies/popular") ?? [];
            }
            else if (newest && hasFilters)
            {
                movies = await GetFilteredMovies(
                    genre, year, minRating, sort);
            }
            else if (newest)
            {
                movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>(
                    "api/Movies/new") ?? [];
            }
            else if (hasFilters)
            {
                movies = await GetFilteredMovies(
                    genre, year, minRating, sort);
            }
            else
            {
                movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>(
                    "api/Movies") ?? [];
            }

            ViewBag.Genres =
                await _httpClient.GetFromJsonAsync<List<string>>(
                    "api/Movies/genres") ?? [];
        }
        catch (HttpRequestException)
        {
            movies = [];
            ViewBag.Genres = new List<string>();
            ViewBag.CatalogError =
                "Не вдалося підключитися до Foxflix API.";
        }
        catch (JsonException)
        {
            movies = [];
            ViewBag.Genres = new List<string>();
            ViewBag.CatalogError =
                "Не вдалося прочитати відповідь Foxflix API.";
        }

        ViewBag.Genre = genre;
        ViewBag.Year = year;
        ViewBag.MinRating = minRating;
        ViewBag.Popular = popular;
        ViewBag.Newest = newest;
        ViewBag.Sort = sort;
        ViewBag.Source = "local";

        return View(movies);
    }

    private async Task<IActionResult> TmdbCatalog(
        string? query,
        int? genreId,
        int? year,
        double? minimumRating,
        string sortBy,
        int page)
    {
        page = Math.Clamp(page, 1, 500);

        var parameters = new List<string>
        {
            $"page={page}",
            $"sortBy={Uri.EscapeDataString(sortBy)}"
        };

        bool isSearch = !string.IsNullOrWhiteSpace(query);

        if (isSearch)
        {
            parameters.Add(
                $"query={Uri.EscapeDataString(query!.Trim())}");
        }
        else
        {
            if (genreId.HasValue)
                parameters.Add($"genreId={genreId.Value}");

            if (year.HasValue)
                parameters.Add($"year={year.Value}");

            if (minimumRating.HasValue)
            {
                parameters.Add(
                    "minimumRating=" +
                    minimumRating.Value.ToString(
                        CultureInfo.InvariantCulture));
            }
        }

        string endpoint = isSearch
            ? "api/tmdb/search?"
            : "api/tmdb/discover?";

        TmdbCatalogResponseDto model = new();

        try
        {
            var response = await _httpClient.GetAsync(
                endpoint + string.Join("&", parameters));

            if (response.IsSuccessStatusCode)
            {
                model = await response.Content
                    .ReadFromJsonAsync<TmdbCatalogResponseDto>()
                    ?? new TmdbCatalogResponseDto();
            }
            else
            {
                ViewBag.CatalogError =
                    $"TMDB API повернув помилку: {(int)response.StatusCode}";
            }
        }
        catch (HttpRequestException)
        {
            ViewBag.CatalogError =
                "Не вдалося підключитися до Foxflix API.";
        }
        catch (JsonException)
        {
            ViewBag.CatalogError =
                "Не вдалося прочитати відповідь TMDB.";
        }

        ViewBag.Query = query;
        ViewBag.GenreId = genreId;
        ViewBag.Year = year;
        ViewBag.MinimumRating = minimumRating;
        ViewBag.SortBy = sortBy;
        ViewBag.Source = "tmdb";

        return View("Tmdb", model);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
            return NotFound();

        TmdbMovieDetailsDto? movie;

        try
        {
            movie = await _httpClient.GetFromJsonAsync<TmdbMovieDetailsDto>(
                $"api/tmdb/movie/{id}");
        }
        catch (HttpRequestException)
        {
            return View("DetailsError");
        }
        catch (JsonException)
        {
            return View("DetailsError");
        }

        if (movie is null)
            return NotFound();

        try
        {
            var videos = await _httpClient
                .GetFromJsonAsync<TmdbVideosResponseDto>(
                    $"api/tmdb/movie/{id}/videos");

            var trailer = videos?.Results
                .Where(v =>
                    string.Equals(
                        v.Site,
                        "YouTube",
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        v.Type,
                        "Trailer",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(v.Key))
                .OrderByDescending(v => v.Official)
                .FirstOrDefault();

            ViewBag.TrailerKey = trailer?.Key;
        }
        catch (HttpRequestException)
        {
            ViewBag.TrailerKey = null;
        }
        catch (JsonException)
        {
            ViewBag.TrailerKey = null;
        }

        return View(movie);
    }

    private async Task<List<MovieDto>> GetFilteredMovies(
        string? genre,
        int? year,
        double? minRating,
        string? sort)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(genre))
            parameters.Add($"genre={Uri.EscapeDataString(genre)}");

        if (year.HasValue)
            parameters.Add($"year={year.Value}");

        if (minRating.HasValue)
        {
            parameters.Add(
                "minRating=" +
                minRating.Value.ToString(
                    CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrWhiteSpace(sort))
            parameters.Add($"sort={Uri.EscapeDataString(sort)}");

        var url = "api/Movies/filter?" + string.Join("&", parameters);

        return await _httpClient.GetFromJsonAsync<List<MovieDto>>(url)
            ?? [];
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

    public class TmdbMovieDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? ReleaseDate { get; set; }
        public string? FullPosterUrl { get; set; }
        public string? FullBackdropUrl { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public int Runtime { get; set; }
        public string? OriginalLanguage { get; set; }
        public List<TmdbGenreDto> Genres { get; set; } = [];
        public string? Tagline { get; set; }
    }

    public class TmdbGenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TmdbVideosResponseDto
    {
        public int Id { get; set; }
        public List<TmdbVideoDto> Results { get; set; } = [];
    }

    public class TmdbVideoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Official { get; set; }
    }

    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string PosterUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int DurationMinutes { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int AgeRating { get; set; }
        public string? PosterPath { get; set; }
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddToMyList(
    int movieId,
    string? title,
    string? overview,
    string? releaseDate,
    string? posterUrl,
    double rating)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        var token = User.FindFirst("access_token")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["MyListMessage"] =
                "Сесія авторизації не знайдена. Увійди в акаунт повторно.";

            return RedirectToAction(nameof(Index));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/tmdb-favorites");

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content = JsonContent.Create(new
        {
            TmdbMovieId = movieId,
            Title = title ?? string.Empty,
            Overview = overview ?? string.Empty,
            ReleaseDate = releaseDate,
            PosterUrl = posterUrl,
            Rating = rating
        });

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["MyListMessage"] =
                    "Фільм додано до твого списку.";
            }
            else if (response.StatusCode ==
                     System.Net.HttpStatusCode.Conflict)
            {
                TempData["MyListMessage"] =
                    "Цей фільм уже є у твоєму списку.";
            }
            else
            {
                TempData["MyListMessage"] =
                    $"Не вдалося додати фільм. Код API: {(int)response.StatusCode}.";
            }
        }
        catch (HttpRequestException)
        {
            TempData["MyListMessage"] =
                "Не вдалося підключитися до Foxflix API.";
        }

        return RedirectToAction(nameof(Index), new { source = "tmdb" });
    }


}