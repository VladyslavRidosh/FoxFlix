using System.Net.Http.Headers;
using System.Text.Json;
using Foxflix.API.Models.Tmdb;

namespace Foxflix.API.Services
{
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions =
    new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

        public TmdbService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress =
                new Uri("https://api.themoviedb.org/3/");

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));
        }

        public async Task<TmdbMovieResponse> GetPopularMoviesAsync(
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string url =
                $"movie/popular?language={Uri.EscapeDataString(language)}&page={page}";

            return await GetAsync<TmdbMovieResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbMovieResponse> GetNowPlayingMoviesAsync(
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string url =
                $"movie/now_playing?language={Uri.EscapeDataString(language)}&page={page}";

            return await GetAsync<TmdbMovieResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbMovieResponse> GetUpcomingMoviesAsync(
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string url =
                $"movie/upcoming?language={Uri.EscapeDataString(language)}&page={page}";

            return await GetAsync<TmdbMovieResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbSeriesResponse> GetPopularSeriesAsync(
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string url =
                $"tv/popular?language={Uri.EscapeDataString(language)}&page={page}";

            return await GetAsync<TmdbSeriesResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbSeriesResponse> GetOnTheAirSeriesAsync(
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string url =
                $"tv/on_the_air?language={Uri.EscapeDataString(language)}&page={page}";

            return await GetAsync<TmdbSeriesResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbSeriesResponse> GetTopRatedSeriesAsync(
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string url =
                $"tv/top_rated?language={Uri.EscapeDataString(language)}&page={page}";

            return await GetAsync<TmdbSeriesResponse>(
                url,
                cancellationToken);
        }
        public async Task<TmdbMovieDetailsResponse> GetMovieDetailsAsync(
    int movieId,
    string language = "uk-UA",
    CancellationToken cancellationToken = default)
        {
            if (movieId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(movieId),
                    "ID фільму має бути більшим за нуль.");
            }

            string url =
                $"movie/{movieId}?language={Uri.EscapeDataString(language)}";

            return await GetAsync<TmdbMovieDetailsResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbSeriesDetailsResponse> GetSeriesDetailsAsync(
            int seriesId,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            if (seriesId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(seriesId),
                    "ID серіалу має бути більшим за нуль.");
            }

            string url =
                $"tv/{seriesId}?language={Uri.EscapeDataString(language)}";

            return await GetAsync<TmdbSeriesDetailsResponse>(
                url,
                cancellationToken);
        }
        public async Task<TmdbVideosResponse> GetMovieVideosAsync(
            int movieId,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            if (movieId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(movieId),
                    "ID фільму має бути більшим за нуль.");
            }

            string url =
                $"movie/{movieId}/videos?language={Uri.EscapeDataString(language)}";

            return await GetAsync<TmdbVideosResponse>(
                url,
                cancellationToken);
        }
        public async Task<TmdbMovieResponse> SearchMoviesAsync(
            string query,
            int page = 1,
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(
                    "Пошуковий запит не може бути порожнім.",
                    nameof(query));
            }

            ValidatePage(page);

            string url =
                $"search/movie?query={Uri.EscapeDataString(query)}" +
                $"&language={Uri.EscapeDataString(language)}" +
                $"&page={page}&include_adult=false";

            return await GetAsync<TmdbMovieResponse>(
                url,
                cancellationToken);
        }

        public async Task<TmdbMovieResponse> DiscoverMoviesAsync(
            int page = 1,
            int? genreId = null,
            int? year = null,
            double? minimumRating = null,
            string sortBy = "popularity.desc",
            string language = "uk-UA",
            CancellationToken cancellationToken = default)
        {
            ValidatePage(page);

            string[] allowedSortOptions =
            [
                "popularity.desc",
                "popularity.asc",
                "vote_average.desc",
                "vote_average.asc",
                "primary_release_date.desc",
                "primary_release_date.asc",
                "original_title.asc",
                "original_title.desc"
            ];

            if (!allowedSortOptions.Contains(sortBy))
            {
                throw new ArgumentException(
                    "Непідтримуване сортування.",
                    nameof(sortBy));
            }

            if (year.HasValue &&
                (year.Value < 1888 || year.Value > 2100))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(year),
                    "Рік має бути в діапазоні від 1888 до 2100.");
            }

            if (minimumRating.HasValue &&
                (minimumRating.Value < 0 ||
                 minimumRating.Value > 10))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumRating),
                    "Рейтинг має бути в діапазоні від 0 до 10.");
            }

            var parameters = new List<string>
            {
                $"page={page}",
                $"language={Uri.EscapeDataString(language)}",
                $"sort_by={Uri.EscapeDataString(sortBy)}",
                "include_adult=false"
            };

            if (genreId.HasValue)
            {
                parameters.Add(
                    $"with_genres={genreId.Value}");
            }

            if (year.HasValue)
            {
                parameters.Add(
                    $"primary_release_year={year.Value}");
            }

            if (minimumRating.HasValue)
            {
                parameters.Add(
                    $"vote_average.gte={minimumRating.Value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)}");

                parameters.Add("vote_count.gte=50");
            }

            string url =
                "discover/movie?" +
                string.Join("&", parameters);

            return await GetAsync<TmdbMovieResponse>(
                url,
                cancellationToken);
        }

        private async Task<T> GetAsync<T>(
            string url,
            CancellationToken cancellationToken)
        {
            string? token =
                _configuration["TMDB:ReadAccessToken"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "TMDB Read Access Token не налаштований.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"TMDB повернув помилку HTTP {(int)response.StatusCode}.");
            }

            await using Stream stream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            T? result =
                await JsonSerializer.DeserializeAsync<T>(
                    stream,
                    JsonOptions,
                    cancellationToken);

            return result ??
                throw new InvalidOperationException(
                    "TMDB повернув порожню відповідь.");
        }

        private static void ValidatePage(int page)
        {
            if (page < 1 || page > 500)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(page),
                    "Сторінка має бути в діапазоні від 1 до 500.");
            }
        }
    }
}