using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly FoxflixDbContext _context;

    public MoviesController(FoxflixDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMovies()
    {
        var movies = await _context.Movies
            .AsNoTracking()
            .ToListAsync();

        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMovie(int id)
    {
        var movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            return NotFound();
        }

        return Ok(movie);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovie(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMovie),
            new { id = movie.Id },
            movie);
    }
    [HttpGet("search")]
    public async Task<IActionResult> SearchMovies(
    [FromQuery] string query,
    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<Movie>());
        }

        var normalizedQuery = query.Trim().ToLower();

        var movies = await _context.Movies
            .AsNoTracking()
            .Where(m => m.Title.ToLower().Contains(normalizedQuery))
            .OrderBy(m => m.Title)
            .ToListAsync(ct);

        return Ok(movies);
    }
  
[HttpGet("filter")]
public async Task<IActionResult> FilterMovies(
    [FromQuery] string? genre,
    [FromQuery] int? year,
    [FromQuery] double? minRating,
    [FromQuery] string? sort,
    CancellationToken ct)
    {
        var query = _context.Movies
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var normalizedGenre = genre.Trim().ToLower();

            query = query.Where(m =>
                m.Genre.ToLower().Contains(normalizedGenre));
        }

        if (year.HasValue)
        {
            query = query.Where(m =>
                m.ReleaseYear == year.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(m =>
                m.Rating >= minRating.Value);
        }

        switch (sort?.ToLower())
        {
            case "newest":
                query = query
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenByDescending(m => m.Rating);
                break;

            case "title":
                query = query
                    .OrderBy(m => m.Title);
                break;

            default:
                query = query
                    .OrderByDescending(m => m.Rating)
                    .ThenByDescending(m => m.ReleaseYear);
                break;
        }

        var movies = await query.ToListAsync(ct);

        return Ok(movies);
    }


    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularMovies(
    CancellationToken ct)
    {
        var movies = await _context.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.Rating)
            .ThenByDescending(m => m.ReleaseYear)
            .ToListAsync(ct);

        return Ok(movies);
    }
    [HttpGet("new")]
    public async Task<IActionResult> GetNewMovies(
    CancellationToken ct)
    {
        var movies = await _context.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.ReleaseYear)
            .ThenByDescending(m => m.Rating)
            .ToListAsync(ct);

        return Ok(movies);
    }
    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres(
    CancellationToken ct)
    {
        var genres = await _context.Movies
            .AsNoTracking()
            .Where(m => !string.IsNullOrWhiteSpace(m.Genre))
            .Select(m => m.Genre)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(ct);

        return Ok(genres);
    }
    [HttpGet("sort")]
    public async Task<IActionResult> SortMovies(
    [FromQuery] string by = "rating",
    CancellationToken ct = default)
    {
        var query = _context.Movies
            .AsNoTracking()
            .AsQueryable();

        switch (by.ToLower())
        {
            case "newest":
                query = query
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenByDescending(m => m.Rating);
                break;

            case "title":
                query = query
                    .OrderBy(m => m.Title);
                break;

            default:
                query = query
                    .OrderByDescending(m => m.Rating)
                    .ThenByDescending(m => m.ReleaseYear);
                break;
        }

        var movies = await query.ToListAsync(ct);

        return Ok(movies);
    }
}