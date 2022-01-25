using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Entities
{
    public class CustomLastArtist
    {
        public void SetLastArtist(LastArtist lastArtist)
        {
            Id = lastArtist.Id;
            Name = lastArtist.Name;
            Bio = lastArtist.Bio;
            Mbid = lastArtist.Mbid;
            Url = lastArtist.Url;
            OnTour = lastArtist.OnTour;
            PlayCount = lastArtist.PlayCount;
            Similar = lastArtist.Similar;
            Stats = lastArtist.Stats;
            Tags = lastArtist.Tags;
            MainImage = lastArtist.MainImage;
            Artist = new Artist() { Name = lastArtist.Name };
        }

        public CustomLastArtist()
        {

        }

        public Artist Artist { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public LastWiki Bio { get; set; }
        public string Mbid { get; set; }
        public Uri Url { get; set; }
        public bool OnTour { get; set; }
        public IEnumerable<LastTag> Tags { get; set; }
        public List<LastArtist> Similar { get; set; }
        public LastImageSet MainImage { get; set; }
        public int? PlayCount { get; set; }
        public LastStats Stats { get; set; }
    }
}
