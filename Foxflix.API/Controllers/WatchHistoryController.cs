using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Identity;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/watch-history")]
[Authorize]
public class WatchHistoryController : ControllerBase
{
    private readonly FoxflixDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public WatchHistoryController(
        FoxflixDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var history = await _context.WatchHistories
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.MovieId,
                x.PositionSeconds,
                x.DurationSeconds,
                x.Completed,
                x.UpdatedAt,
                Movie = x.Movie == null ? null : new
                {
                    x.Movie.Id,
                    x.Movie.Title,
                    x.Movie.PosterUrl
                }
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetMovieHistory(int movieId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var history = await _context.WatchHistories
            .AsNoTracking()
            .Where(x =>
                x.UserId == user.Id &&
                x.MovieId == movieId)
            .Select(x => new
            {
                x.MovieId,
                x.PositionSeconds,
                x.DurationSeconds,
                x.Completed,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (history is null)
        {
            return NotFound(new
            {
                message = "Історію перегляду ще не створено."
            });
        }

        return Ok(history);
    }

    [HttpPost("{movieId:int}")]
    public async Task<IActionResult> SaveHistory(
        int movieId,
        [FromBody] WatchHistoryDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        if (movieId <= 0)
        {
            return BadRequest(new
            {
                message = "Некоректний ідентифікатор фільму."
            });
        }

        if (!double.IsFinite(dto.PositionSeconds) ||
            !double.IsFinite(dto.DurationSeconds) ||
            dto.PositionSeconds < 0 ||
            dto.DurationSeconds < 0)
        {
            return BadRequest(new
            {
                message = "Некоректна позиція або тривалість відео."
            });
        }

        var movieExists = await _context.Movies
            .AnyAsync(m => m.Id == movieId);

        if (!movieExists)
        {
            return NotFound(new
            {
                message = "Фільм не знайдено."
            });
        }

        var position = dto.DurationSeconds > 0
            ? Math.Min(dto.PositionSeconds, dto.DurationSeconds)
            : dto.PositionSeconds;

        var history = await _context.WatchHistories
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id &&
                x.MovieId == movieId);

        if (history is null)
        {
            history = new WatchHistory
            {
                UserId = user.Id,
                MovieId = movieId,
                PositionSeconds = position,
                DurationSeconds = dto.DurationSeconds,
                Completed = dto.Completed,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WatchHistories.Add(history);
        }
        else
        {
            history.PositionSeconds = position;
            history.DurationSeconds = dto.DurationSeconds;
            history.Completed = dto.Completed;
            history.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            history.MovieId,
            history.PositionSeconds,
            history.DurationSeconds,
            history.Completed,
            history.UpdatedAt
        });
    }

    [HttpDelete("{movieId:int}")]
    public async Task<IActionResult> DeleteHistory(int movieId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var history = await _context.WatchHistories
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id &&
                x.MovieId == movieId);

        if (history is null)
        {
            return NotFound(new
            {
                message = "Історію перегляду не знайдено."
            });
        }

        _context.WatchHistories.Remove(history);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Історію перегляду видалено.",
            movieId
        });
    }

    public class WatchHistoryDto
    {
        public double PositionSeconds { get; set; }
        public double DurationSeconds { get; set; }
        public bool Completed { get; set; }
    }
}