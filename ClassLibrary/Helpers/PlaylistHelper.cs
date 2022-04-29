using ClassLibrary.Control;
using ClassLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace ClassLibrary.Helpers
{
    public static class PlaylistHelper
    {
        public static event RoutedEventHandler PlaylistChanged;
        public delegate void PlaylistPickerRequestedHandler(object sender, List<string> list);
        public static event PlaylistPickerRequestedHandler PlaylistPickerRequested;

        public static void RequestPlaylistPicker(object sender, List<string> list) => PlaylistPickerRequested?.Invoke(sender, list);

        public static string TrackByIndex(int index)
        {
            if (NowPlaying.Current.Songs.Count > 0)
                return NowPlaying.Current.Songs[index];
            else
                return "LISTA VAZIA!!";
        }

        public async static Task SaveCurrentPlaylist()
        {
            if (NowPlaying.Current.Songs.Count == 0)
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("LastPlayback.xml", CreationCollisionOption.OpenIfExists);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);

                return;
            }

            XmlDocument DOC = new XmlDocument();

            XmlElement mainElem = DOC.DocumentElement;
            XmlElement ELE = DOC.CreateElement("Playlist");
            ELE.SetAttribute("Name", "LastPlayback");
            DOC.AppendChild(ELE);

            foreach (string file in NowPlaying.Current.Songs)
            {
                XmlElement x = DOC.CreateElement("Song");
                x.InnerText = file;

                DOC.FirstChild.AppendChild(x);
            }

            StorageFile st = await ApplicationData.Current.LocalFolder.CreateFileAsync("LastPlayback.xml", CreationCollisionOption.ReplaceExisting);

            await DOC.SaveToFileAsync(st);

            ApplicationData.Current.LocalSettings.Values["ExistsLastPlayback"] = true;
        }

        public static async void DeletePlaylist(object sender, Playlist playlist)
        {
            MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("DeletePlaylistMessage"), ApplicationInfo.Current.Resources.GetString("DeletePlaylistMessageTitle"));
            md.Commands.Add(new UICommand(ApplicationInfo.Current.Resources.GetString("Yes"), async (t) =>
            {
                StorageFolder playlistsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);

                IStorageItem playlistItem = await playlistsFolder.TryGetItemAsync(playlist.PlaylistFileName);

                if (playlistItem != null)
                {
                    try
                    {
                        await playlistItem.DeleteAsync(StorageDeleteOption.Default);

                        PlaylistChanged?.Invoke(sender, new RoutedEventArgs());
                    }
                    catch
                    {

                    }
                }
            }));

            md.Commands.Add(new UICommand(ApplicationInfo.Current.Resources.GetString("No")));

            md.CancelCommandIndex = 1;
            md.DefaultCommandIndex = 1;

            await md.ShowAsync();
        }

    }
}
