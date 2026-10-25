namespace Foxflix.Domain.Entities;

public class WatchHistory
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int MovieId { get; set; }

    public double PositionSeconds { get; set; }

    public double DurationSeconds { get; set; }

    public bool Completed { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Movie Movie { get; set; } = null!;
}