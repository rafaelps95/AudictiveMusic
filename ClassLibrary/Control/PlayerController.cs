using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ClassLibrary.Control
{
    public static class PlayerController
    {
        public static event RoutedEventHandler FullPlayerRequested;

        public static void OpenPlayer(object sender) => FullPlayerRequested?.Invoke(sender, new RoutedEventArgs());

        public static async void Play(MediaItem mediaItem)
        {
            List<string> list = await Collection.FetchSongs(mediaItem);
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static async void Play(List<MediaItem> mediaItems)
        {
            List<string> list = new List<string>();
            foreach (MediaItem mediaItem in mediaItems)
            {
                List<string> tempList = await Collection.FetchSongs(mediaItem);
                list.AddRange(tempList);
            }

            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static void Play(List<string> list)
        {
            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static async void AddToQueue(MediaItem mediaItem, bool asNext = false)
        {
            List<string> list = await Collection.FetchSongs(mediaItem);
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static async void AddToQueue(List<MediaItem> mediaItems, bool asNext = false)
        {
            List<string> list = new List<string>();
            foreach (MediaItem mediaItem in mediaItems)
            {
                List<string> tempList = await Collection.FetchSongs(mediaItem);
                list.AddRange(tempList);
            }

            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static void AddToQueue(List<string> list, bool asNext = false)
        {
            MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }
    }
}
