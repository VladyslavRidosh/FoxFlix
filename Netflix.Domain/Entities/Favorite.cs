namespace Foxflix.Domain.Entities;

public class Favorite
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int MovieId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Movie Movie { get; set; } = null!;
}