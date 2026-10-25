
using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Identity;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/tmdb-favorites")]
[Authorize]
public class TmdbFavoritesController : ControllerBase
{
    private readonly FoxflixDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TmdbFavoritesController(
        FoxflixDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetFavorites(
        CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var favorites = await _context.TmdbFavorites
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return Ok(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite(
        [FromBody] TmdbFavorite request,
        CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        if (request.TmdbMovieId <= 0 ||
            string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Некоректні дані фільму.");
        }

        var exists = await _context.TmdbFavorites
            .AnyAsync(x =>
                x.UserId == user.Id &&
                x.TmdbMovieId == request.TmdbMovieId, ct);

        if (exists)
        {
            return Conflict("Фільм уже є у вашому списку.");
        }

        var favorite = new TmdbFavorite
        {
            UserId = user.Id,
            TmdbMovieId = request.TmdbMovieId,
            Title = request.Title,
            Overview = request.Overview ?? string.Empty,
            ReleaseDate = request.ReleaseDate,
            PosterUrl = request.PosterUrl,
            Rating = request.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.TmdbFavorites.Add(favorite);
        await _context.SaveChangesAsync(ct);

        return Ok(favorite);
    }

    [HttpDelete("{tmdbMovieId:int}")]
    public async Task<IActionResult> RemoveFavorite(
        int tmdbMovieId,
        CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var favorite = await _context.TmdbFavorites
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id &&
                x.TmdbMovieId == tmdbMovieId, ct);

        if (favorite is null)
        {
            return NotFound();
        }

        _context.TmdbFavorites.Remove(favorite);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }
}
