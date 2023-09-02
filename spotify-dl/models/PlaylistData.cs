namespace spotify_dl.models
{
    public class PlaylistData
    {
        public List<Item>? Items { get; set; }
        public int Total { get; set; }
    }
    public class Item
    {
        public Track Track { get; set; }
    }
    public class Track
    {
        public string? Uri { get; set; }
    }
}