using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/seasons/{seasonId:int}/episodes/{episodeNumber:int}/media")]
public class EpisodeMediaController : ControllerBase
{
    private readonly FoxflixDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public EpisodeMediaController(
        FoxflixDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    [HttpPost("video")]
    [RequestSizeLimit(5L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> UploadVideo(
        int seasonId,
        int episodeNumber,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Video file is required.");
        }

        var episode = await _context.Episodes
            .FirstOrDefaultAsync(e =>
                e.SeasonId == seasonId &&
                e.EpisodeNumber == episodeNumber);

        if (episode is null)
        {
            return NotFound("Episode not found.");
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        var allowedExtensions = new[]
        {
            ".mp4",
            ".webm",
            ".mov",
            ".mkv"
        };

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(
                "Unsupported video format.");
        }

        await using var stream = file.OpenReadStream();

        var result = await _fileStorage.SaveVideoAsync(
            stream,
            file.FileName,
            HttpContext.RequestAborted);

        episode.VideoPath = result.RelativePath;
        episode.VideoSizeBytes = result.SizeBytes;
        episode.VideoContentType = result.ContentType;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            episode.Id,
            episode.SeasonId,
            episode.EpisodeNumber,
            episode.VideoPath,
            episode.VideoSizeBytes,
            episode.VideoContentType
        });
    }
}