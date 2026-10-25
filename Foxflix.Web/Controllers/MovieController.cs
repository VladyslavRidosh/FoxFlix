using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foxflix.Web.Controllers;

public class MovieController : Controller
{
    private readonly HttpClient _httpClient;

    public MovieController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FoxflixAPI");
    }

    public async Task<IActionResult> Index(int id = 1)
    {
        MovieDto? movie;
        try
        {
            using var movieResponse = await _httpClient.GetAsync($"api/Movies/{id}");
            if (movieResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!movieResponse.IsSuccessStatusCode)
            {
                return StatusCode(502, "Foxflix API не змогла завантажити фільм.");
            }

            movie = await movieResponse.Content.ReadFromJsonAsync<MovieDto>();
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Foxflix API тимчасово недоступна.");
        }
        catch (JsonException)
        {
            return StatusCode(502, "Foxflix API повернула некоректні дані фільму.");
        }

        if (movie is null)
        {
            return NotFound();
        }

        movie.IsFavorite = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var token = GetAccessToken();

            if (!string.IsNullOrWhiteSpace(token))
            {
                using var request = CreateRequest(
                    HttpMethod.Get,
                    "api/favorites",
                    token);

                using var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var favorites =
                        await response.Content.ReadFromJsonAsync<
                            List<FavoriteDto>>();

                    movie.IsFavorite =
                        favorites?.Any(f => f.MovieId == movie.Id) == true;
                }
            }
        }

        return View(movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToFavorites(int movieId)
    {
        var token = GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Account");
        }

        using var request = CreateRequest(
            HttpMethod.Post,
            $"api/favorites/{movieId}",
            token);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode &&
            response.StatusCode != HttpStatusCode.Conflict)
        {
            return StatusCode((int)response.StatusCode);
        }

        return RedirectToAction(nameof(Index), new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromFavorites(int movieId)
    {
        var token = GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Account");
        }

        using var request = CreateRequest(
            HttpMethod.Delete,
            $"api/favorites/{movieId}",
            token);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode);
        }

        return RedirectToAction(nameof(Index), new { id = movieId });
    }

    [HttpGet]
    public async Task<IActionResult> GetWatchHistory(int movieId)
    {
        if (movieId <= 0)
        {
            return BadRequest();
        }

        var token = GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        using var request = CreateRequest(
            HttpMethod.Get,
            $"api/watch-history/{movieId}",
            token);

        using var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"Watch history GET failed: {(int)response.StatusCode} " +
                $"{response.StatusCode}. Body: {errorBody}");

            return StatusCode((int)response.StatusCode);
        }

        var history = await response.Content
            .ReadFromJsonAsync<WatchHistoryDto>();

        if (history is null)
        {
            return StatusCode(502, "API повернув порожню відповідь.");
        }

        return Ok(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWatchHistory(
        int movieId,
        [FromBody] WatchHistoryDto model)
    {
        if (movieId <= 0)
        {
            return BadRequest();
        }

        var token = GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        if (!double.IsFinite(model.PositionSeconds) ||
            !double.IsFinite(model.DurationSeconds) ||
            model.PositionSeconds < 0 ||
            model.DurationSeconds < 0)
        {
            return BadRequest("Некоректна позиція або тривалість відео.");
        }

        using var request = CreateRequest(
            HttpMethod.Post,
            $"api/watch-history/{movieId}",
            token);

        request.Content = JsonContent.Create(new
        {
            positionSeconds = model.PositionSeconds,
            durationSeconds = model.DurationSeconds,
            completed = model.Completed
        });

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"Watch history POST failed: {(int)response.StatusCode} " +
                $"{response.StatusCode}. Body: {errorBody}");

            return StatusCode((int)response.StatusCode);
        }

        var history = await response.Content
            .ReadFromJsonAsync<WatchHistoryDto>();

        if (history is null)
        {
            return StatusCode(502, "API повернув порожню відповідь.");
        }

        return Ok(history);
    }

    private string? GetAccessToken()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return User.FindFirst("access_token")?.Value;
    }

    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url,
        string token)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    public class WatchHistoryDto
    {
        public int MovieId { get; set; }

        public double PositionSeconds { get; set; }

        public double DurationSeconds { get; set; }

        public bool Completed { get; set; }

        public DateTime UpdatedAt { get; set; }
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

        public string? VideoPath { get; set; }

        public string? PosterPath { get; set; }

        public long? VideoSizeBytes { get; set; }

        public string? VideoContentType { get; set; }

        public bool IsFavorite { get; set; }
    }

    public class FavoriteDto
    {
        public int MovieId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PosterUrl { get; set; } = string.Empty;

        public int ReleaseYear { get; set; }

        public double Rating { get; set; }
    }
}