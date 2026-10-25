using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace Foxflix.Web.Controllers;

public class SeriesController : Controller
{
    private readonly HttpClient _httpClient;

    public SeriesController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FoxflixAPI");
    }

    private async Task<T?> TryGetFromJsonAsync<T>(string url)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(url);
        }
        catch (HttpRequestException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task<IActionResult> Index()
    {
        var series =
            await TryGetFromJsonAsync<List<SeriesDto>>("api/Series")
            ?? new List<SeriesDto>();

        var popularSeries =
            await TryGetFromJsonAsync<TmdbSeriesResponseDto>(
                "api/tmdb/series/popular?page=1");

        var onTheAirSeries =
            await TryGetFromJsonAsync<TmdbSeriesResponseDto>(
                "api/tmdb/series/on-the-air?page=1");

        var topRatedSeries =
            await TryGetFromJsonAsync<TmdbSeriesResponseDto>(
                "api/tmdb/series/top-rated?page=1");

        ViewBag.TmdbPopularSeries =
            popularSeries?.Results ?? new List<TmdbSeriesDto>();

        ViewBag.TmdbOnTheAirSeries =
            onTheAirSeries?.Results ?? new List<TmdbSeriesDto>();

        ViewBag.TmdbTopRatedSeries =
            topRatedSeries?.Results ?? new List<TmdbSeriesDto>();

        return View(series);
    }

    public async Task<IActionResult> Details(int id)
    {
        var series =
            await TryGetFromJsonAsync<SeriesDto>(
                $"api/Series/{id}");

        if (series is null)
        {
            return NotFound();
        }

        var seasons =
            await TryGetFromJsonAsync<List<SeasonDto>>(
                $"api/series/{id}/seasons")
            ?? new List<SeasonDto>();

        ViewBag.Seasons = seasons;

        return View(series);
    }
   
public async Task<IActionResult> TmdbDetails(int id)
    {
        var series = await TryGetFromJsonAsync<TmdbSeriesDetailsDto>(
            $"api/tmdb/series/{id}");

        if (series is null)
        {
            return NotFound();
        }

        return View(series);
    }


    public class TmdbSeriesDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string? FirstAirDate { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public double VoteAverage { get; set; }

        public int NumberOfSeasons { get; set; }

        public int NumberOfEpisodes { get; set; }

        public string? Status { get; set; }

        public string? OriginalLanguage { get; set; }

        public string? Tagline { get; set; }

        public string? PosterUrl =>
            string.IsNullOrWhiteSpace(PosterPath)
                ? null
                : $"https://image.tmdb.org/t/p/w500{PosterPath}";

        public string? BackdropUrl =>
            string.IsNullOrWhiteSpace(BackdropPath)
                ? PosterUrl
                : $"https://image.tmdb.org/t/p/original{BackdropPath}";
    }


    public async Task<IActionResult> Episodes(
    int seriesId,
    int seasonId)
    {
        var series =
            await TryGetFromJsonAsync<SeriesDto>(
                $"api/Series/{seriesId}");

        if (series is null)
        {
            return NotFound();
        }

        var episodes =
            await TryGetFromJsonAsync<List<EpisodeDto>>(
                $"api/seasons/{seasonId}/episodes")
            ?? new List<EpisodeDto>();

        ViewBag.SeriesId = seriesId;
        ViewBag.SeasonId = seasonId;
        ViewBag.SeriesTitle = series.Title;

        return View(episodes);
    }
    public async Task<IActionResult> Watch(
    int seriesId,
    int seasonId,
    int episodeNumber)
    {
        var series =
            await TryGetFromJsonAsync<SeriesDto>(
                $"api/Series/{seriesId}");

        if (series is null)
        {
            return NotFound();
        }

        var episode =
            await TryGetFromJsonAsync<EpisodeDto>(
                $"api/seasons/{seasonId}/episodes/{episodeNumber}");

        if (episode is null)
        {
            return NotFound();
        }

        var episodes =
            await TryGetFromJsonAsync<List<EpisodeDto>>(
                $"api/seasons/{seasonId}/episodes")
            ?? new List<EpisodeDto>();

        var currentIndex = episodes.FindIndex(
            e => e.EpisodeNumber == episodeNumber);

        EpisodeDto? previousEpisode = null;
        EpisodeDto? nextEpisode = null;

        if (currentIndex > 0)
        {
            previousEpisode = episodes[currentIndex - 1];
        }

        if (currentIndex >= 0 &&
            currentIndex < episodes.Count - 1)
        {
            nextEpisode = episodes[currentIndex + 1];
        }

        ViewBag.SeriesId = seriesId;
        ViewBag.SeasonId = seasonId;
        ViewBag.SeriesTitle = series.Title;

        ViewBag.PreviousEpisode = previousEpisode;
        ViewBag.NextEpisode = nextEpisode;

        return View(episode);
    }
    public class SeriesDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ReleaseYear { get; set; }

        public string PosterUrl { get; set; } = string.Empty;

        public string BackdropUrl { get; set; } = string.Empty;

        public string? TrailerUrl { get; set; }

        public double Rating { get; set; }

        public string Genre { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public int AgeRating { get; set; }

        public int SeasonsCount { get; set; }

        public string? PosterPath { get; set; }
    }
    public class EpisodeDto
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string? VideoPath { get; set; }
        public string? PosterPath { get; set; }
    }
    public class SeasonDto
    {
        public int Id { get; set; }

        public int SeriesId { get; set; }

        public int SeasonNumber { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? PosterPath { get; set; }
    }

    public class TmdbSeriesResponseDto
    {
        public List<TmdbSeriesDto> Results { get; set; } = new();
    }

    public class TmdbSeriesDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string? FirstAirDate { get; set; }

        public string? PosterUrl { get; set; }

        public string? BackdropUrl { get; set; }

        public double VoteAverage { get; set; }
    }
}