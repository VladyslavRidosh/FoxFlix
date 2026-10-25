namespace Foxflix.Application.DTOs;

public record UploadResultDto(
    string RelativePath,
    long SizeBytes,
    string ContentType);

public record UpdateWatchProgressDto(
    int MovieId,
    double PositionSeconds,
    double DurationSeconds);

public record WatchProgressDto(
    int MovieId,
    double PositionSeconds,
    double DurationSeconds,
    bool Completed,
    DateTime UpdatedAt);

public record ContinueWatchingDto(
    int MovieId,
    string Title,
    string PosterUrl,
    string? PosterPath,
    double PositionSeconds,
    double DurationSeconds,
    DateTime UpdatedAt);