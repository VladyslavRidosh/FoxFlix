using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/series/{seriesId:int}/seasons")]
public class SeasonController : ControllerBase
{
    private readonly FoxflixDbContext _context;

    public SeasonController(FoxflixDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSeasons(int seriesId)
    {
        var seriesExists =
            await _context.Series.AnyAsync(s => s.Id == seriesId);

        if (!seriesExists)
        {
            return NotFound($"Series {seriesId} not found.");
        }

        var seasons =
            await _context.Seasons
                .AsNoTracking()
                .Where(s => s.SeriesId == seriesId)
                .OrderBy(s => s.SeasonNumber)
                .ToListAsync();

        return Ok(seasons);
    }

    [HttpGet("{seasonNumber:int}")]
    public async Task<IActionResult> GetSeason(
        int seriesId,
        int seasonNumber)
    {
        var season =
            await _context.Seasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SeriesId == seriesId &&
                    s.SeasonNumber == seasonNumber);

        if (season is null)
        {
            return NotFound();
        }

        return Ok(season);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateSeason(
        int seriesId,
        [FromBody] CreateSeasonDto dto)
    {
        var seriesExists =
            await _context.Series.AnyAsync(s => s.Id == seriesId);

        if (!seriesExists)
        {
            return NotFound($"Series {seriesId} not found.");
        }

        if (dto.SeasonNumber <= 0)
        {
            return BadRequest(
                "Номер сезону повинен бути більшим за 0.");
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(
                "Назва сезону є обов'язковою.");
        }

        var existingSeason =
            await _context.Seasons.AnyAsync(s =>
                s.SeriesId == seriesId &&
                s.SeasonNumber == dto.SeasonNumber);

        if (existingSeason)
        {
            return Conflict(
                $"Season {dto.SeasonNumber} already exists.");
        }

        var season = new Season
        {
            SeriesId = seriesId,
            SeasonNumber = dto.SeasonNumber,
            Title = dto.Title,
            PosterPath = dto.PosterPath
        };

        _context.Seasons.Add(season);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSeason),
            new
            {
                seriesId,
                seasonNumber = season.SeasonNumber
            },
            season);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{seasonNumber:int}")]
    public async Task<IActionResult> UpdateSeason(
        int seriesId,
        int seasonNumber,
        [FromBody] UpdateSeasonDto dto)
    {
        var season =
            await _context.Seasons
                .FirstOrDefaultAsync(s =>
                    s.SeriesId == seriesId &&
                    s.SeasonNumber == seasonNumber);

        if (season is null)
        {
            return NotFound("Сезон не знайдено.");
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(
                "Назва сезону є обов'язковою.");
        }

        season.Title = dto.Title;
        season.PosterPath = dto.PosterPath;

        await _context.SaveChangesAsync();

        return Ok(season);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{seasonNumber:int}")]
    public async Task<IActionResult> DeleteSeason(
        int seriesId,
        int seasonNumber)
    {
        var season =
            await _context.Seasons
                .FirstOrDefaultAsync(s =>
                    s.SeriesId == seriesId &&
                    s.SeasonNumber == seasonNumber);

        if (season is null)
        {
            return NotFound("Сезон не знайдено.");
        }

        _context.Seasons.Remove(season);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Сезон видалено.",
            seriesId,
            seasonNumber
        });
    }

    public class CreateSeasonDto
    {
        public int SeasonNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
    }

    public class UpdateSeasonDto
    {
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
    }
}