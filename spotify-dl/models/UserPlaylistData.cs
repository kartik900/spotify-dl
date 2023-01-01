using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotify_dl.models
{
    public class ExternalUrls
    {
        public string? Spotify { get; set; }
    }


    public class Item
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public Tracks? Tracks { get; set; }
    }

    public class UserPlaylistData
    {
        public string? Href { get; set; }
        public List<Item>? Items { get; set; }
        public int Total { get; set; }
    }

    public class Tracks
    {
        public string? Href { get; set; }
        public int Total { get; set; }
    }


}
