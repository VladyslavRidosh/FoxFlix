namespace Foxflix.Domain.Entities;

public class Series
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string PosterUrl { get; set; } = string.Empty;

    public string BackdropUrl { get; set; } = string.Empty;

    public string? TrailerUrl { get; set; }

    public double Rating { get; set; }

    public string Genre { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public int AgeRating { get; set; }

    public int SeasonsCount { get; set; }

    public string? PosterPath { get; set; }

    public ICollection<Season> Seasons { get; set; } = new List<Season>();
}