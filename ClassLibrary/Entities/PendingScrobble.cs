using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Entities
{
    public class PendingScrobble
    {
        public int ID
        {
            get;
            set;
        }

        public Song Song
        {
            get;
            set;
        }

        public DateTimeOffset Time
        {
            get;
            set;
        }

        public bool Sent
        {
            get;
            set;
        }


        public PendingScrobble(Song song, DateTimeOffset time)
        {
            this.Song = song;
            this.Time = time;
            this.Sent = false;
        }
    }
}
