using Foxflix.Domain.Entities;
using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxflix.API.Controllers;

[ApiController]
[Route("api/seasons/{seasonId:int}/episodes")]
public class EpisodeController : ControllerBase
{
    private readonly FoxflixDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public EpisodeController(
        FoxflixDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }


    /*
     * GET:
     * /api/seasons/1/episodes
     */

    [HttpGet]
    public async Task<IActionResult> GetEpisodes(
        int seasonId)
    {
        var seasonExists =
            await _context.Seasons
                .AnyAsync(s => s.Id == seasonId);

        if (!seasonExists)
        {
            return NotFound(
                $"Season {seasonId} not found.");
        }

        var episodes =
            await _context.Episodes
                .AsNoTracking()
                .Where(e =>
                    e.SeasonId == seasonId)
                .OrderBy(e =>
                    e.EpisodeNumber)
                .ToListAsync();

        return Ok(episodes);
    }


    /*
     * GET:
     * /api/seasons/1/episodes/1
     */

    [HttpGet("{episodeNumber:int}")]
    public async Task<IActionResult> GetEpisode(
        int seasonId,
        int episodeNumber)
    {
        var episode =
            await _context.Episodes
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.SeasonId == seasonId &&
                    e.EpisodeNumber == episodeNumber);

        if (episode is null)
        {
            return NotFound();
        }

        return Ok(episode);
    }


    /*
     * POST:
     * /api/seasons/1/episodes
     */
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateEpisode(
        int seasonId,
        [FromBody] CreateEpisodeDto dto)
    {
        var seasonExists =
            await _context.Seasons
                .AnyAsync(s => s.Id == seasonId);

        if (!seasonExists)
        {
            return NotFound(
                $"Season {seasonId} not found.");
        }

        var episodeExists =
            await _context.Episodes
                .AnyAsync(e =>
                    e.SeasonId == seasonId &&
                    e.EpisodeNumber ==
                    dto.EpisodeNumber);

        if (episodeExists)
        {
            return Conflict(
                $"Episode {dto.EpisodeNumber} already exists.");
        }

        var episode =
            new Episode
            {
                SeasonId =
                    seasonId,

                EpisodeNumber =
                    dto.EpisodeNumber,

                Title =
                    dto.Title,

                Description =
                    dto.Description,

                DurationMinutes =
                    dto.DurationMinutes,

                PosterPath =
                    dto.PosterPath
            };

        _context.Episodes.Add(
            episode);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEpisode),
            new
            {
                seasonId,
                episodeNumber =
                    episode.EpisodeNumber
            },
            episode);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{episodeNumber:int}")]
    public async Task<IActionResult> UpdateEpisode(
    int seasonId,
    int episodeNumber,
    [FromBody] UpdateEpisodeDto dto)
    {
        var episode =
            await _context.Episodes
                .FirstOrDefaultAsync(e =>
                    e.SeasonId == seasonId &&
                    e.EpisodeNumber == episodeNumber);

        if (episode is null)
        {
            return NotFound("Епізод не знайдено.");
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(
                "Назва епізоду є обов'язковою.");
        }

        if (dto.DurationMinutes <= 0)
        {
            return BadRequest(
                "Тривалість повинна бути більшою за 0.");
        }

        episode.Title = dto.Title;
        episode.Description = dto.Description;
        episode.DurationMinutes = dto.DurationMinutes;
        episode.PosterPath = dto.PosterPath;

        await _context.SaveChangesAsync();

        return Ok(episode);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{episodeNumber:int}")]
    public async Task<IActionResult> DeleteEpisode(
        int seasonId,
        int episodeNumber)
    {
        var episode =
            await _context.Episodes
                .FirstOrDefaultAsync(e =>
                    e.SeasonId == seasonId &&
                    e.EpisodeNumber == episodeNumber);

        if (episode is null)
        {
            return NotFound("Епізод не знайдено.");
        }

        _context.Episodes.Remove(episode);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Епізод видалено.",
            seasonId,
            episodeNumber
        });
    }

    public class UpdateEpisodeDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string? PosterPath { get; set; }
    }

    /*
     * POST:
     * /api/seasons/1/episodes/1/video
     *
     * Upload video.
     */

    [Authorize(Roles = "Admin")]
    [HttpPost("{episodeNumber:int}/video")]
    [RequestSizeLimit(2_000_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadVideo(
        int seasonId,
        int episodeNumber,
        [FromForm] VideoUploadDto dto,
        CancellationToken cancellationToken)
    {
        var episode =
            await _context.Episodes
                .FirstOrDefaultAsync(e =>
                    e.SeasonId == seasonId &&
                    e.EpisodeNumber == episodeNumber,
                    cancellationToken);

        if (episode is null)
        {
            return NotFound(
                $"Episode {episodeNumber} not found.");
        }

        if (dto.Video is null ||
            dto.Video.Length == 0)
        {
            return BadRequest(
                "Video file is required.");
        }

        try
        {
            await using var stream =
                dto.Video.OpenReadStream();

            var result =
                await _fileStorage.SaveVideoAsync(
                    stream,
                    dto.Video.FileName,
                    cancellationToken);

            episode.VideoPath =
                result.RelativePath;

            episode.VideoSizeBytes =
                result.SizeBytes;

            episode.VideoContentType =
                result.ContentType;

            await _context.SaveChangesAsync(
                cancellationToken);

            return Ok(
                new
                {
                    episode.Id,
                    episode.EpisodeNumber,
                    episode.Title,
                    episode.VideoPath,
                    episode.VideoSizeBytes,
                    episode.VideoContentType
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /*
     * DTO:
     * Create episode
     */

    public class CreateEpisodeDto
    {
        public int EpisodeNumber { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public int DurationMinutes { get; set; }

        public string? PosterPath { get; set; }
    }


    /*
     * DTO:
     * Upload video
     */

    public class VideoUploadDto
    {
        public IFormFile Video { get; set; } = null!;
    }
}