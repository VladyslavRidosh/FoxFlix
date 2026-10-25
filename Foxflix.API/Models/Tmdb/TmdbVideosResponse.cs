namespace Foxflix.API.Models.Tmdb
{
    public class TmdbVideosResponse
    {
        public int Id { get; set; }

        public List<TmdbVideoDto> Results { get; set; } = [];
    }

    public class TmdbVideoDto
    {
        public string Id { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Site { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool Official { get; set; }

        public string? PublishedAt { get; set; }
    }
}