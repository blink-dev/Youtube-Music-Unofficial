using System;

namespace Youtube_Music.Models
{
    public class YoutubeVideoInfo
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public string VideoId { get; set; }
        public Uri Thumbnail { get; set; }
        public string PlaylistId { get; set; }
        public string PlayerParams { get; set; }
        public string Params { get; set; }
    }
}
