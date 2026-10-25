using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/seasons/{seasonId:int}/episodes/{episodeNumber:int}/stream")]
public class EpisodeStreamingController : ControllerBase
{
    private readonly FoxflixDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public EpisodeStreamingController(
        FoxflixDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Stream(
        int seasonId,
        int episodeNumber)
    {
        var episode = await _context.Episodes
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.SeasonId == seasonId &&
                e.EpisodeNumber == episodeNumber);

        if (episode is null)
        {
            return NotFound("Episode not found.");
        }

        if (string.IsNullOrWhiteSpace(episode.VideoPath))
        {
            return NotFound("Episode video not found.");
        }

        var absolutePath = _fileStorage.GetAbsolutePath(
            episode.VideoPath);

        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("Video file not found.");
        }

        var contentType =
            string.IsNullOrWhiteSpace(episode.VideoContentType)
                ? "video/mp4"
                : episode.VideoContentType;

        return PhysicalFile(
            absolutePath,
            contentType,
            enableRangeProcessing: true);
    }
}