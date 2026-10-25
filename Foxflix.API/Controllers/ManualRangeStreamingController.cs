using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Foxflix.Infrastructure.Persistence.Context;
using Foxflix.Infrastructure.Services;
using Foxflix.Domain.Entities;

namespace StreamingApp.Controllers;

[ApiController]
[Route("api/movies/{movieId:int}/stream-manual")]
public class ManualRangeStreamingController : ControllerBase
{
    private const int BufferSize = 64 * 1024;
    private readonly FoxflixDbContext _db;
    private readonly IFileStorageService _storage;

    public ManualRangeStreamingController(FoxflixDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> Stream(int movieId, CancellationToken ct)
    {
        var movie = await _db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == movieId, ct);
        if (movie?.VideoPath is null) return NotFound();

        var path = _storage.GetAbsolutePath(movie.VideoPath);
        if (!System.IO.File.Exists(path)) return NotFound();

        var fileInfo = new FileInfo(path);
        var totalLength = fileInfo.Length;
        var contentType = string.IsNullOrEmpty(movie.VideoContentType) ? "video/mp4" : movie.VideoContentType;

        Response.Headers.AcceptRanges = "bytes";

        long start = 0;
        long end = totalLength - 1;
        var isRangeRequest = false;

        var rangeHeader = Request.Headers.Range.ToString();

        if (RangeHeaderValue.TryParse(rangeHeader, out var range) && range.Ranges.Count > 0)
        {
            var requested = range.Ranges.First();

            if (requested.From.HasValue)
            {
                start = requested.From.Value;
                end = requested.To ?? totalLength - 1;
            }
            else if (requested.To.HasValue)
            {
                start = totalLength - requested.To.Value;
                end = totalLength - 1;
            }

            if (start >= totalLength || end >= totalLength || start > end)
            {
                Response.Headers.ContentRange = $"bytes */{totalLength}";
                return StatusCode(StatusCodes.Status416RangeNotSatisfiable);
            }

            isRangeRequest = true;
        }

        var chunkLength = end - start + 1;

        Response.ContentType = contentType;
        Response.ContentLength = chunkLength;

        if (isRangeRequest)
        {
            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.Headers.ContentRange = $"bytes {start}-{end}/{totalLength}";
        }
        else
        {
            Response.StatusCode = StatusCodes.Status200OK;
        }

        await using var fileStream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);

        fileStream.Seek(start, SeekOrigin.Begin);

        var buffer = new byte[BufferSize];
        long remaining = chunkLength;

        while (remaining > 0)
        {
            var readSize = (int)Math.Min(buffer.Length, remaining);
            var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, readSize), ct);
            if (bytesRead == 0) break;

            await Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            remaining -= bytesRead;
        }

        return new EmptyResult();
    }
}
