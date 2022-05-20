using ClassLibrary.Dao;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ClassLibrary.Control
{
    public class Ctr_PendingScrobble
    {
        public event RoutedEventHandler Updated;

        private static Ctr_PendingScrobble instance;

        public static Ctr_PendingScrobble Current
        {
            get
            {
                if (instance == null)
                    instance = new Ctr_PendingScrobble();

                return instance;
            }
        }

        public bool Add(PendingScrobble pendingScrobble)
        {
            bool result = PendingScrobbleDao.Add(pendingScrobble);

            if (result)
                Updated?.Invoke(null, new RoutedEventArgs());

            return result;
        }

        public async Task<bool[]> SendScrobbles(List<PendingScrobble> pendingScrobbles)
        {
            bool[] result = new bool[pendingScrobbles.Count];

            for(int i = 0; i < pendingScrobbles.Count; i++)
            {
                PendingScrobble ps = pendingScrobbles[i];
                try
                {
                    Scrobble scrobble = new Scrobble(ps.Song.Artist, ps.Song.Album, ps.Song.Name, ps.Time);
                    var response = await LastFm.Current.Client.Scrobbler.ScrobbleAsync(scrobble);

                    Remove(ps);
                    result[i] = true;
                }
                catch
                {
                    result[i] = false;
                }
                finally
                {

                }
            }


            return result;
        }

        public List<PendingScrobble> GetPendingScrobbles()
        {
            return PendingScrobbleDao.GetPendingScrobbles();
        }


        //public bool UpdateSong(Song song)
        //{
        //    return SongDao.UpdateSong(song);
        //}

        public bool Remove(PendingScrobble pendingScrobble)
        {
            bool result = PendingScrobbleDao.Remove(pendingScrobble);

            if (result)
            {
                Updated?.Invoke(null, new RoutedEventArgs());
            }

            return result;
        }

        public bool Clear()
        {
            bool result = PendingScrobbleDao.ClearList();

            if (result)
            {
                Updated?.Invoke(null, new RoutedEventArgs());
            }

            return result;
        }
    }
}
