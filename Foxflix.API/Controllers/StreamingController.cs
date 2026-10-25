using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/movies/{movieId:int}")]
public class StreamingController : ControllerBase
{
    private readonly FoxflixDbContext _db;
    private readonly IFileStorageService _storage;

    public StreamingController(
        FoxflixDbContext db,
        IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet("stream")]
    public async Task<IActionResult> Stream(
        int movieId,
        CancellationToken ct)
    {
        Movie? movie;

        try
        {
            movie = await _db.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == movieId, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            throw new TimeoutException(
                "Запрос к базе данных был отменён или превысил допустимое время ожидания.",
                ex);
        }

        if (movie is null)
        {
            return NotFound();
        }

        if (movie?.VideoPath is null)
        {
            return NotFound();
        }

        var path = _storage.GetAbsolutePath(movie.VideoPath);

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var contentType = string.IsNullOrEmpty(movie.VideoContentType)
            ? "video/mp4"
            : movie.VideoContentType;

        return PhysicalFile(
            path,
            contentType,
            enableRangeProcessing: true);
    }

    [HttpGet("poster")]
    public async Task<IActionResult> Poster(
        int movieId,
        CancellationToken ct)
    {
        var movie = await _db.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == movieId, ct);

        if (movie?.PosterPath is null)
        {
            return NotFound();
        }

        var path = _storage.GetAbsolutePath(movie.PosterPath);

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return PhysicalFile(
            path,
            "image/jpeg",
            enableRangeProcessing: true);
    }
}