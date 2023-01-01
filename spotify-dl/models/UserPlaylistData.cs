namespace spotify_dl.models
{
    internal class UserPlaylistData
    {
        public string? Href { get; set; }
        public List<Item>? Items { get; set; }
        public int Total { get; set; }
    }
    internal class Item
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public Tracks? Tracks { get; set; }
    }
    internal class Tracks
    {
        public string? Href { get; set; }
        public int Total { get; set; }
    }
    internal class ExternalUrls
    {
        public string? Spotify { get; set; }
    }
}
