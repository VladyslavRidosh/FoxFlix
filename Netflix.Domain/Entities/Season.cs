namespace Foxflix.Domain.Entities;

public class Season
{
    public int Id { get; set; }

    public int SeriesId { get; set; }

    public int SeasonNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? PosterPath { get; set; }

    public Series Series { get; set; } = null!;
}