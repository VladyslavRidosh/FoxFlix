namespace Foxflix.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _videosPath;
    private readonly string _postersPath;

    private static readonly string[] AllowedVideoExtensions =
    {
        ".mp4",
        ".webm",
        ".mov",
        ".mkv"
    };

    private static readonly string[] AllowedImageExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public FileStorageService()
    {
        var storagePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "MediaStorage");

        _videosPath = Path.Combine(storagePath, "videos");
        _postersPath = Path.Combine(storagePath, "posters");

        Directory.CreateDirectory(_videosPath);
        Directory.CreateDirectory(_postersPath);
    }

    public async Task<(string RelativePath, long SizeBytes, string ContentType)> SaveVideoAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await SaveFileAsync(
            stream,
            fileName,
            _videosPath,
            "video",
            cancellationToken);
    }

    public async Task<(string RelativePath, long SizeBytes, string ContentType)> SavePosterAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await SaveFileAsync(
            stream,
            fileName,
            _postersPath,
            "image",
            cancellationToken);
    }

    public string GetVideoPath(string relativePath)
    {
        return GetAbsolutePath(relativePath);
    }

    public string GetPosterPath(string relativePath)
    {
        return GetAbsolutePath(relativePath);
    }

    public string GetAbsolutePath(string relativePath)
    {
        var normalizedPath = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        return Path.GetFullPath(normalizedPath);
    }

    public void DeleteIfExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var absolutePath = GetAbsolutePath(relativePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    private static async Task<(string RelativePath, long SizeBytes, string ContentType)> SaveFileAsync(
        Stream stream,
        string fileName,
        string directory,
        string type,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (type == "video" && !AllowedVideoExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Непідтримуваний формат відео.");
        }

        if (type == "image" && !AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Непідтримуваний формат зображення.");
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            directory,
            uniqueFileName);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await stream.CopyToAsync(
            fileStream,
            cancellationToken);

        var relativeFolder = type == "video"
            ? "MediaStorage/videos"
            : "MediaStorage/posters";

        var relativePath = $"{relativeFolder}/{uniqueFileName}";

        var contentType = type == "video"
            ? GetVideoContentType(extension)
            : GetImageContentType(extension);

        return (
            relativePath,
            new FileInfo(filePath).Length,
            contentType);
    }

    private static string GetVideoContentType(string extension)
    {
        return extension switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            _ => "application/octet-stream"
        };
    }

    private static string GetImageContentType(string extension)
    {
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}