using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ClassLibrary.Helpers.Enumerators;

namespace AudictiveMusicUWP.Gui.Util
{
    public class ShareHelper
    {
        private static ShareHelper _instance;

        public static ShareHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ShareHelper();

                return _instance;
            }
        }

        private ShareHelper()
        {

        }

        private DataTransferManager dataTransferManager;
        private List<IStorageItem> filesToShare;
        private string _title;

        public async Task<bool> Share(List<string> list)
        {
            filesToShare = new List<IStorageItem>();
            dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareHandler;

            return await PrepareFiles(list);
        }

        public async Task<bool> Share(MediaItem mediaItem)
        {
            filesToShare = new List<IStorageItem>();
            dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareHandler;

            return await PrepareFiles(mediaItem);
        }

        private async Task<bool> PrepareFiles(MediaItem mediaItem)
        {
            if (mediaItem.GetType() == typeof(Album))
            {
                Album album = mediaItem as Album;

                var songs = Ctr_Song.Current.GetSongsByAlbum(album);
                foreach (Song song in songs)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(song.SongURI);
                    if (file != null)
                    {
                        filesToShare.Add(file);
                    }
                }

                _title = album.Name + " " + ApplicationInfo.Current.Resources.GetString("By") + album.Artist;
            }
            else if (mediaItem.GetType() == typeof(Artist))
            {
                Artist artist = mediaItem as Artist;

                var songs = Ctr_Song.Current.GetSongsByArtist(artist);
                foreach (Song song in songs)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(song.SongURI);
                    if (file != null)
                    {
                        filesToShare.Add(file);
                    }
                }

                _title = artist.Name;
            }
            else if (mediaItem.GetType() == typeof(FolderItem))
            {
                FolderItem item = mediaItem as FolderItem;
                if (item.IsFolder)
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                    var files = await StorageHelper.ScanFolder(folder);
                    filesToShare.AddRange(files);
                    _title = files.Count + " " + ApplicationInfo.Current.Resources.GetString("SongPlural").ToLower();
                }
                else
                {
                    if (StorageHelper.IsMusicFile(item.Path))
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(item.Path);
                        filesToShare.Add(file);
                    }

                    _title = 1 + " " + ApplicationInfo.Current.Resources.GetString("SongPlural").ToLower();
                }
            }
            else if (mediaItem.GetType() == typeof(Playlist))
            {
                Playlist playlist = mediaItem as Playlist;

                foreach (string song in playlist.Songs)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(song);
                    if (file != null)
                    {
                        filesToShare.Add(file);
                    }
                }

                _title = playlist.Name;
            }
            else if (mediaItem.GetType() == typeof(Song))
            {
                Song song = mediaItem as Song;
                StorageFile file = await StorageFile.GetFileFromPathAsync(song.SongURI);
                if (file == null)
                {
                    filesToShare = null;
                    return false;
                }

                _title = song.Name + " " + ApplicationInfo.Current.Resources.GetString("By") + song.Artist;

                filesToShare.Add(file);
            }

            if (filesToShare.Count == 0)
                return false;

            DataTransferManager.ShowShareUI();

            return true;
        }

        private async Task<bool> PrepareFiles(List<string> list)
        {
            foreach (string path in list)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                if (file != null)
                    filesToShare.Add(file);
            }

            if (filesToShare.Count == 0)
                return false;

            _title = list.Count + " " + ApplicationInfo.Current.Resources.GetString("SongPlural").ToLower();

            DataTransferManager.ShowShareUI();

            return true;
        }

        private void ShareHandler(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            DataRequest request = args.Request;
            request.Data.Properties.Title = _title;
            request.Data.SetStorageItems(filesToShare);

            deferral.Complete();
        }

    }
}
