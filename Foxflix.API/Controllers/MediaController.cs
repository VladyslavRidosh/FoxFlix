using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Foxflix.Application.DTOs;
namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/movies/{movieId:int}/media")]
public class MediaController : ControllerBase
{
    private readonly FoxflixDbContext _db;
    private readonly IFileStorageService _storage;

    public MediaController(
        FoxflixDbContext db,
        IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpPost("video")]
    [RequestSizeLimit(5L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> UploadVideo(
        int movieId,
        IFormFile file,
        CancellationToken ct)
    {
        var movie = await _db.Movies
            .FirstOrDefaultAsync(m => m.Id == movieId, ct);

        if (movie is null)
        {
            return NotFound($"Movie {movieId} not found.");
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("Video file is required.");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var result = await _storage.SaveVideoAsync(
                stream,
                file.FileName,
                ct);

            _storage.DeleteIfExists(movie.VideoPath);

            movie.VideoPath = result.RelativePath;
            movie.VideoSizeBytes = result.SizeBytes;
            movie.VideoContentType = result.ContentType;

            await _db.SaveChangesAsync(ct);

            return Ok(new UploadResultDto(
    result.RelativePath,
    result.SizeBytes,
    result.ContentType));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("poster")]
    [RequestSizeLimit(10L * 1024 * 1024)]
    public async Task<IActionResult> UploadPoster(
        int movieId,
        IFormFile file,
        CancellationToken ct)
    {
        var movie = await _db.Movies
            .FirstOrDefaultAsync(m => m.Id == movieId, ct);

        if (movie is null)
        {
            return NotFound($"Movie {movieId} not found.");
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("Poster file is required.");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var result = await _storage.SavePosterAsync(
                stream,
                file.FileName,
                ct);

            _storage.DeleteIfExists(movie.PosterPath);

            movie.PosterPath = result.RelativePath;

            await _db.SaveChangesAsync(ct);

            return Ok(new UploadResultDto(
    result.RelativePath,
    result.SizeBytes,
    result.ContentType));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}