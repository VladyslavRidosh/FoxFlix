namespace Foxflix.Domain.Entities;

public class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string PosterUrl { get; set; } = string.Empty;

    public string BackdropUrl { get; set; } = string.Empty;

    public string? TrailerUrl { get; set; }

    public double Rating { get; set; }

    public int DurationMinutes { get; set; }

    public string Genre { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public int AgeRating { get; set; }

    public string? VideoPath { get; set; }

    public string? PosterPath { get; set; }

    public long? VideoSizeBytes { get; set; }

    public string? VideoContentType { get; set; }

    public ICollection<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
}