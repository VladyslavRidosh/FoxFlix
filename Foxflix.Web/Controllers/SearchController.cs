
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Foxflix.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly HttpClient _httpClient;

        public SearchController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("FoxflixAPI");
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            ViewBag.Query = query;
            ViewBag.Page = page;

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new TmdbSearchResponse());
            }

            try
            {
                string url =
                    $"api/tmdb/search?query={Uri.EscapeDataString(query.Trim())}&page={page}";

                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    url,
                    cancellationToken);

                return View(response ?? new TmdbSearchResponse());
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Не вдалося отримати результати пошуку. Спробуй пізніше.";
                return View(new TmdbSearchResponse());
            }
            catch (System.Text.Json.JsonException)
            {
                ViewBag.Error = "Не вдалося обробити результати пошуку.";
                return View(new TmdbSearchResponse());
            }
        }

        public class TmdbSearchResponse
        {
            public int Page { get; set; }
            public List<TmdbSearchMovieDto> Results { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalResults { get; set; }
        }

        public class TmdbSearchMovieDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Overview { get; set; } = string.Empty;
            public string? ReleaseDate { get; set; }
            public string? PosterPath { get; set; }
            public string? BackdropPath { get; set; }
            public double VoteAverage { get; set; }

            public string? PosterUrl =>
                string.IsNullOrWhiteSpace(PosterPath)
                    ? null
                    : $"https://image.tmdb.org/t/p/w500{PosterPath}";

            public string? BackdropUrl =>
                string.IsNullOrWhiteSpace(BackdropPath)
                    ? null
                    : $"https://image.tmdb.org/t/p/w1280{BackdropPath}";

            public string ReleaseYear =>
                DateTime.TryParse(ReleaseDate, out var date)
                    ? date.Year.ToString()
                    : "Рік невідомий";
        }
    }
}

