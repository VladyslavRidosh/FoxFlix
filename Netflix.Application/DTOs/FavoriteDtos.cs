namespace Foxflix.Application.DTOs;

public record FavoriteDto(
    int MovieId,
    string Title,
    string PosterUrl,
    string? PosterPath,
    int ReleaseYear,
    double Rating);

public record AddFavoriteDto(
    int MovieId);