using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeriesController : ControllerBase
{
    private readonly FoxflixDbContext _context;

    public SeriesController(FoxflixDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSeries()
    {
        var series = await _context.Series
            .AsNoTracking()
            .ToListAsync();

        return Ok(series);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSeries(int id)
    {
        var series = await _context.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (series is null)
        {
            return NotFound();
        }

        return Ok(series);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSeries(Series series)
    {
        _context.Series.Add(series);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSeries),
            new { id = series.Id },
            series);
    }
}