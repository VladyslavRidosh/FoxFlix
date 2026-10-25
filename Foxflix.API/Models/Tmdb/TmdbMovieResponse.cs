namespace Foxflix.API.Models.Tmdb
{
    public class TmdbMovieResponse
    {
        public int Page { get; set; }

        public int TotalPages { get; set; }

        public int TotalResults { get; set; }

        public List<TmdbMovieDto> Results { get; set; } = [];
    }

    public class TmdbMovieDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string? ReleaseDate { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public double VoteAverage { get; set; }

        public int VoteCount { get; set; }

        public List<int> GenreIds { get; set; } = [];

        public string OriginalLanguage { get; set; } = string.Empty;

        public double Popularity { get; set; }

        public bool Adult { get; set; }

        public string FullPosterUrl =>
            string.IsNullOrWhiteSpace(PosterPath)
                ? string.Empty
                : $"https://image.tmdb.org/t/p/w500{PosterPath}";

        public string FullBackdropUrl =>
            string.IsNullOrWhiteSpace(BackdropPath)
                ? string.Empty
                : $"https://image.tmdb.org/t/p/w1280{BackdropPath}";
    }
}