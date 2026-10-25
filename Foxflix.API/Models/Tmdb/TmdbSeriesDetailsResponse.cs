
namespace Foxflix.API.Models.Tmdb
{
    public class TmdbSeriesDetailsResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string? FirstAirDate { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public double VoteAverage { get; set; }

        public int NumberOfSeasons { get; set; }

        public int NumberOfEpisodes { get; set; }

        public string? Status { get; set; }

        public string? OriginalLanguage { get; set; }

        public string? Tagline { get; set; }
    }
}