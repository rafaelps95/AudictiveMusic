using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudictiveMusicUWP.Gui.Util
{
    public static class PlayerController
    {

        public static void Play(Artist artist)
        {
            List<Song> songs = Ctr_Song.Current.GetSongsByArtist(artist);
            List<string> list = new List<string>();
            foreach (Song s in songs)
                list.Add(s.SongURI);

            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static void Play(Album album)
        {
            List<Song> songs = Ctr_Song.Current.GetSongsByAlbum(album);
            List<string> list = new List<string>();
            foreach (Song s in songs)
                list.Add(s.SongURI);

            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static void Play(Song song)
        {
            List<string> list = new List<string>();
            list.Add(song.SongURI);
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static void Play(Playlist playlist)
        {
            MessageService.SendMessageToBackground(new SetPlaylistMessage(playlist.Songs));
        }

        public static async void Play(FolderItem folder)
        {
            List<string> list = await Ctr_FolderItem.GetSongs(folder);
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static void Play(List<string> list)
        {
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static void AddToPlaylist(Artist artist, bool asNext = false)
        {
            List<Song> songs = Ctr_Song.Current.GetSongsByArtist(artist);
            List<string> list = new List<string>();
            foreach (Song s in songs)
                list.Add(s.SongURI);

            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static void AddToPlaylist(Album album, bool asNext = false)
        {
            List<Song> songs = Ctr_Song.Current.GetSongsByAlbum(album);
            List<string> list = new List<string>();
            foreach (Song s in songs)
                list.Add(s.SongURI);

            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static void AddToPlaylist(Song song, bool asNext = false)
        {
            List<string> list = new List<string>();
            list.Add(song.SongURI);

            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static void AddToPlaylist(Playlist playlist, bool asNext = false)
        {
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(playlist.Songs, asNext));
        }

        public static async void AddToPlaylist(FolderItem folder, bool asNext = false)
        {
            List<string> list = await Ctr_FolderItem.GetSongs(folder);
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static void AddToPlaylist(List<string> list, bool asNext = false)
        {
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }
    }
}
