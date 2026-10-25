
using Foxflix.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foxflix.API.Controllers
{
    [ApiController]
    [Route("api/tmdb")]
    public class TmdbController : ControllerBase
    {
        private readonly TmdbService _tmdbService;

        public TmdbController(TmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular(
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetPopularMoviesAsync(
                    page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати популярні фільми від TMDB." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("now-playing")]
        public async Task<IActionResult> GetNowPlaying(
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetNowPlayingMoviesAsync(
                    page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати фільми з TMDB." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming(
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetUpcomingMoviesAsync(
                    page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати майбутні прем'єри TMDB." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.SearchMoviesAsync(
                    query, page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося виконати пошук у TMDB." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("discover")]
        public async Task<IActionResult> Discover(
            [FromQuery] int page = 1,
            [FromQuery] int? genreId = null,
            [FromQuery] int? year = null,
            [FromQuery] double? minimumRating = null,
            [FromQuery] string sortBy = "popularity.desc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.DiscoverMoviesAsync(
                    page, genreId, year, minimumRating, sortBy,
                    cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося відфільтрувати фільми TMDB." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("movie/{movieId:int}")]
        public async Task<IActionResult> GetMovieDetails(
            int movieId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetMovieDetailsAsync(
                    movieId, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати інформацію про фільм." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("movie/{movieId:int}/videos")]
        public async Task<IActionResult> GetMovieVideos(
            int movieId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetMovieVideosAsync(
                    movieId, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати трейлери фільму." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("series/popular")]
        public async Task<IActionResult> GetPopularSeries(
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetPopularSeriesAsync(
                    page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати популярні серіали від TMDB." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("series/on-the-air")]
        public async Task<IActionResult> GetOnTheAirSeries(
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetOnTheAirSeriesAsync(
                    page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати серіали, що зараз виходять." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("series/top-rated")]
        public async Task<IActionResult> GetTopRatedSeries(
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok(await _tmdbService.GetTopRatedSeriesAsync(
                    page, cancellationToken: cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "Не вдалося отримати серіали з найвищим рейтингом." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Перевір налаштування TMDB у User Secrets." });
            }
        }

        [HttpGet("series/{seriesId:int}")]
        public async Task<IActionResult> GetSeriesDetails(
            int seriesId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var series = await _tmdbService.GetSeriesDetailsAsync(
                    seriesId,
                    cancellationToken: cancellationToken);

                return Ok(series);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    message = "Не вдалося отримати інформацію про серіал."
                });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new
                {
                    message = "Перевір налаштування TMDB у User Secrets."
                });
            }
        }
    }
}
