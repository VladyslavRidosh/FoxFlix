namespace Foxflix.API.Models.Tmdb
{
    public class TmdbMovieDetailsResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string? ReleaseDate { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public double VoteAverage { get; set; }

        public int VoteCount { get; set; }

        public int Runtime { get; set; }

        public string? OriginalLanguage { get; set; }

        public List<TmdbGenreDto> Genres { get; set; } = [];

        public string? Tagline { get; set; }

        public string FullPosterUrl =>
            string.IsNullOrWhiteSpace(PosterPath)
                ? string.Empty
                : $"https://image.tmdb.org/t/p/w780{PosterPath}";

        public string FullBackdropUrl =>
            string.IsNullOrWhiteSpace(BackdropPath)
                ? string.Empty
                : $"https://image.tmdb.org/t/p/original{BackdropPath}";
    }

    public class TmdbGenreDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}