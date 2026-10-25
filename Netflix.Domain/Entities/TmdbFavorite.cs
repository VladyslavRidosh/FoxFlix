
namespace Foxflix.Domain.Entities;

public class TmdbFavorite
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int TmdbMovieId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Overview { get; set; } = string.Empty;

    public string? ReleaseDate { get; set; }

    public string? PosterUrl { get; set; }

    public double Rating { get; set; }

    public DateTime CreatedAt { get; set; }
}

