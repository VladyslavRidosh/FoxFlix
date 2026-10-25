
namespace Foxflix.API.Models.Tmdb
{
    public class TmdbSeriesResponse
    {
        public int Page { get; set; }

        public List<TmdbSeriesDto> Results { get; set; } = new();

        public int TotalPages { get; set; }

        public int TotalResults { get; set; }
    }

    public class TmdbSeriesDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string OriginalName { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string? FirstAirDate { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public double VoteAverage { get; set; }

        public int VoteCount { get; set; }

        public List<int> GenreIds { get; set; } = new();

        public string OriginalLanguage { get; set; } = string.Empty;

        public double Popularity { get; set; }

        public string? PosterUrl =>
            string.IsNullOrWhiteSpace(PosterPath)
                ? null
                : $"https://image.tmdb.org/t/p/w500{PosterPath}";

        public string? BackdropUrl =>
            string.IsNullOrWhiteSpace(BackdropPath)
                ? null
                : $"https://image.tmdb.org/t/p/original{BackdropPath}";
    }
}