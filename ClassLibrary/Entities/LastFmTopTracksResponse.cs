using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Entities.LastFmHelper
{

    public class LastFmTopTracksResponse
    {
        public Toptracks toptracks { get; set; }
    }

    public class Toptracks
    {
        public Track[] track { get; set; }
        public Attr attr { get; set; }
    }

    public class Track
    {
        public Streamable streamable { get; set; }
        public string mbid { get; set; }
        public string name { get; set; }
        public Image[] image { get; set; }
        public Artist artist { get; set; }
        public string url { get; set; }
        public string duration { get; set; }
        public Attr1 attr { get; set; }
        public string playcount { get; set; }
    }

    public class Streamable
    {
        public string fulltrack { get; set; }
        public string text { get; set; }
    }

    public class Artist
    {
        public string url { get; set; }
        public string name { get; set; }
        public string mbid { get; set; }
    }

    public class Attr1
    {
        public string rank { get; set; }
    }
}
