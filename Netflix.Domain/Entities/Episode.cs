namespace Foxflix.Domain.Entities;

public class Episode
{
    public int Id { get; set; }

    public int SeasonId { get; set; }

    public int EpisodeNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public string? VideoPath { get; set; }

    public long? VideoSizeBytes { get; set; }

    public string? VideoContentType { get; set; }

    public string? PosterPath { get; set; }

    public Season Season { get; set; } = null!;
}