namespace Foxflix.Infrastructure.Services;

public interface IFileStorageService
{
    Task<(string RelativePath, long SizeBytes, string ContentType)> SaveVideoAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<(string RelativePath, long SizeBytes, string ContentType)> SavePosterAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    string GetVideoPath(string relativePath);

    string GetPosterPath(string relativePath);

    string GetAbsolutePath(string relativePath);

    void DeleteIfExists(string? relativePath);
}