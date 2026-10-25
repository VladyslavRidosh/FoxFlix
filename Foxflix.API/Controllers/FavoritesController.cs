using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Identity;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly FoxflixDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(
        FoxflixDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var userId = user.Id;

        var favorites =
            await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Movie)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

        return Ok(favorites);
    }

    [HttpPost("{movieId:int}")]
    public async Task<IActionResult> AddFavorite(int movieId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var userId = user.Id;

        var movieExists =
            await _context.Movies
                .AnyAsync(m => m.Id == movieId);

        if (!movieExists)
        {
            return NotFound("Фільм не знайдено.");
        }

        var alreadyExists =
            await _context.Favorites
                .AnyAsync(f =>
                    f.UserId == userId &&
                    f.MovieId == movieId);

        if (alreadyExists)
        {
            return Conflict(
                "Фільм уже знаходиться у списку.");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            MovieId = movieId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favorites.Add(favorite);

        await _context.SaveChangesAsync();

        return Ok(favorite);
    }

    [HttpDelete("{movieId:int}")]
    public async Task<IActionResult> RemoveFavorite(int movieId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var userId = user.Id;

        var favorite =
            await _context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.MovieId == movieId);

        if (favorite is null)
        {
            return NotFound(
                "Фільм не знаходиться у списку.");
        }

        _context.Favorites.Remove(favorite);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Фільм видалено зі списку.",
            movieId
        });
    }
}