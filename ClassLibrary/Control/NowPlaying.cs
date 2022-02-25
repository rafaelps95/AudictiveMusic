using System.Collections.Generic;

namespace ClassLibrary.Control
{
    public class NowPlaying
    {
        public enum PlaylistType
        {
            Custom = 0,
            Artist = 1,
            Album = 2,
            Playlist = 3,
            Favorites = 4
        }

        private static NowPlaying nowPlaying;

        public static NowPlaying Current
        {
            get
            {
                if (nowPlaying == null)
                    nowPlaying = new NowPlaying();

                return nowPlaying;
            }
        }

        private NowPlaying()
        {
            PlaylistContextType = PlaylistType.Custom;
            Index = "";
        }

        public PlaylistType PlaylistContextType;
        public int PlaylistContextID;
        public string Index;
        public double TrackPosition;
        public List<string> Songs = new List<string>();
    }
}
