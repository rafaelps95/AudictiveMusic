using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using static ClassLibrary.Helpers.Enumerators;

namespace AudictiveMusicUWP.Gui.Util
{
    public static class PlayerController
    {
        public static event RoutedEventHandler FullPlayerRequested;

        public static void OpenPlayer(object sender) => FullPlayerRequested?.Invoke(sender, new RoutedEventArgs());

        public static async void Play(object mediaItem, MediaItemType mediaItemType)
        {
            if (mediaItemType == MediaItemType.ListOfStrings)
            {
                Play(mediaItem as List<string>);
                return;
            }

            List<string> list = await FetchSongs(mediaItem, mediaItemType);
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        private static void Play(List<string> list)
        {
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static async void AddToQueue(object mediaItem, MediaItemType mediaItemType, bool asNext = false)
        {
            if (mediaItemType == MediaItemType.ListOfStrings)
            {
                AddToQueue(mediaItem as List<string>, asNext);
                return;
            }

            List<string> list = await FetchSongs(mediaItem, mediaItemType);
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        private static void AddToQueue(List<string> list, bool asNext = false)
        {
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static async Task<List<string>> FetchSongs(object mediaItem, MediaItemType mediaItemType)
        {
            List<string> list = new List<string>();
            List<Song> songs = new List<Song>();

            switch (mediaItemType)
            {
                case MediaItemType.Album:
                    Album album = mediaItem as Album;
                    songs = Ctr_Song.Current.GetSongsByAlbum(album);
                    break;
                case MediaItemType.Artist:
                    Artist artist = mediaItem as Artist;
                    songs = Ctr_Song.Current.GetSongsByArtist(artist);
                    break;
                case MediaItemType.Folder:
                    FolderItem folderItem = mediaItem as FolderItem;
                    list = await Ctr_FolderItem.GetSongs(folderItem);
                    break;
                case MediaItemType.Playlist:
                    Playlist playlist = mediaItem as Playlist;
                    list = playlist.Songs;
                    break;
                case MediaItemType.Song:
                    Song song = mediaItem as Song;
                    songs.Add(song);
                    break;
            }

            if (songs.Count > 0)
                foreach (Song s in songs)
                    list.Add(s.SongURI);

            return list;
        }

        //public static void AddToPlaylist(Artist artist, bool asNext = false)
        //{
        //    List<Song> songs = Ctr_Song.Current.GetSongsByArtist(artist);
        //    List<string> list = new List<string>();
        //    foreach (Song s in songs)
        //        list.Add(s.SongURI);

        //    MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        //}

        //public static void AddToPlaylist(Album album, bool asNext = false)
        //{
        //    List<Song> songs = Ctr_Song.Current.GetSongsByAlbum(album);
        //    List<string> list = new List<string>();
        //    foreach (Song s in songs)
        //        list.Add(s.SongURI);

        //    MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        //}

        //public static void AddToPlaylist(Song song, bool asNext = false)
        //{
        //    List<string> list = new List<string>();
        //    list.Add(song.SongURI);

        //    MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        //}

        //public static void AddToPlaylist(Playlist playlist, bool asNext = false)
        //{
        //    MessageService.SendMessageToBackground(new AddSongsToPlaylist(playlist.Songs, asNext));
        //}

        //public static async void AddToPlaylist(FolderItem folder, bool asNext = false)
        //{
        //    List<string> list = await Ctr_FolderItem.GetSongs(folder);
        //    MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        //}

        //public static void Play(Artist artist)
        //{
        //    List<Song> songs = Ctr_Song.Current.GetSongsByArtist(artist);
        //    List<string> list = new List<string>();
        //    foreach (Song s in songs)
        //        list.Add(s.SongURI);

        //    MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        //}

        //public static void Play(Album album)
        //{
        //    List<Song> songs = Ctr_Song.Current.GetSongsByAlbum(album);
        //    List<string> list = new List<string>();
        //    foreach (Song s in songs)
        //        list.Add(s.SongURI);

        //    MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        //}

        //public static void Play(Song song)
        //{
        //    List<string> list = new List<string>();
        //    list.Add(song.SongURI);
        //    MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        //}

        //public static void Play(Playlist playlist)
        //{
        //    MessageService.SendMessageToBackground(new SetPlaylistMessage(playlist.Songs));
        //}

        //public static async void Play(FolderItem folder)
        //{
        //    List<string> list = await Ctr_FolderItem.GetSongs(folder);
        //    MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        //}
    }
}
