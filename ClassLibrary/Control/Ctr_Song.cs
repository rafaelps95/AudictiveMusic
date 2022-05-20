using ClassLibrary.Dao;
using ClassLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ClassLibrary.Control
{
    public class Ctr_Song
    {
        public delegate void FavoritesChangedEventHandler(Song updatedSong);
        public static event FavoritesChangedEventHandler FavoritesChanged;

        private static Ctr_Song instance;

        public static Ctr_Song Current
        {
            get
            {
                if (instance == null)
                    instance = new Ctr_Song();

                return instance;
            }
        }

        public List<Song> GetSongs(bool sort)
        {
            return SongDao.GetSongs(sort);
        }

        public List<string> GetAllSongsPaths()
        {
            return SongDao.GetAllSongsPaths();
        }

        public Song GetSong(Song song)
        {
            return SongDao.GetSong(song);
        }

        public Song GetRandomSong()
        {
            List<Song> songs = Current.GetSongs(false);

            if (songs.Count == 0)
                return null;

            Random rdn = new Random();

            return songs[rdn.Next(songs.Count - 1)];
        }

        public List<Song> GetSongsByPath(string uri)
        {
            return SongDao.GetSongsByPath(uri);
        }

        public List<Song> GetSongsByAlbum(Album album)
        {
            return SongDao.GetSongsByAlbum(album);
        }

        public List<Song> GetSongsByArtist(Artist artist)
        {
            return SongDao.GetSongsByArtist(artist);
        }

        public List<Song> GetSongsFromPlaylist(List<string> list)
        {
            return SongDao.GetSongsFromPlaylist(list);
        }

        public bool SetFavoriteState(Song song, bool state)
        {
            bool result = SongDao.SetSongFavoriteState(song, state);

            if (result)
                FavoritesChanged?.Invoke(song);

            return result;
        }

        public List<Song> GetFavoriteSongs()
        {
            return SongDao.GetFavoriteSongs();
        }

        public bool UpdateSong(Song song)
        {
            return SongDao.UpdateSong(song);
        }

        public bool SongExists(Song song)
        {
            return SongDao.SongExists(song);
        }

        public bool RemoveSong(Song song)
        {
            return SongDao.RemoveSong(song);
        }
    }
}
