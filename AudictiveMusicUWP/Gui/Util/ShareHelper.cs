﻿using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using static ClassLibrary.Helpers.Enumerators;

namespace AudictiveMusicUWP.Gui.Util
{
    public static class ShareHelper
    {
        private static DataTransferManager dataTransferManager;
        private static List<IStorageItem> filesToShare;
        private static MediaItemType MediaType;
        private static object MediaItem;

        public static async Task<bool> ShareMediaItem(this object page, object mediaItem, MediaItemType type)
        {
            MediaType = type;
            MediaItem = mediaItem;
            filesToShare = new List<IStorageItem>();
            dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareHandler;

            return await PrepareFiles(mediaItem, type);
        }

        private static async Task<bool> PrepareFiles(object mediaItem, MediaItemType type)
        {
            if (type == MediaItemType.Song)
            {
                if (mediaItem.GetType() == typeof(Song))
                {
                    Song song = mediaItem as Song;
                    StorageFile file = await StorageFile.GetFileFromPathAsync(song.SongURI);
                    if (file == null)
                    {
                        filesToShare = null;
                        return false;
                    }

                    filesToShare.Add(file);
                }
                else
                {
                    // É LISTA DE STRING!!
                    List<string> list = mediaItem as List<string>;

                    foreach (string path in list)
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                        if (file != null)
                            filesToShare.Add(file);
                    }
                }
            }
            else if (type == MediaItemType.Album)
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

            }
            else if (type == MediaItemType.Artist)
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
            }
            else if (type == MediaItemType.Folder)
            {
                FolderItem item = mediaItem as FolderItem;
                if (item.IsFolder)
                {
                    StorageFolder subFolder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                    var subFolderItems = await StorageHelper.ReadFolder(subFolder);

                    foreach (StorageFile f in subFolderItems)
                    {
                        if (StorageHelper.IsMusicFile(f.Path))
                            filesToShare.Add(f);
                    }
                }
            }
            else if (type == MediaItemType.Playlist)
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
            }

            if (filesToShare.Count == 0)
                return false;

            DataTransferManager.ShowShareUI();

            return true;
        }

        private static void ShareHandler(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            DataRequest request = args.Request;

            if (MediaType == MediaItemType.Song)
            {
                if (MediaItem.GetType() == typeof(Song))
                {
                    Song song = MediaItem as Song;
                    request.Data.Properties.Title = song.Title + " " + ApplicationInfo.Current.Resources.GetString("By") + song.Artist;
                }
                else
                {
                    request.Data.Properties.Title = (MediaItem as List<string>).Count + " músicas";
                }
                //request.Data.Properties.Description = "Sharing a music file";
            }
            else if (MediaType == MediaItemType.Album)
            {
                Album album = MediaItem as Album;
                request.Data.Properties.Title = album.Name + " " + ApplicationInfo.Current.Resources.GetString("By") + album.Artist;
            }
            else if (MediaType == MediaItemType.Artist)
            {
                Artist artist = MediaItem as Artist;
                request.Data.Properties.Title = artist.Name;
            }
            else if (MediaType == MediaItemType.Playlist)
            {
                Playlist playlist = MediaItem as Playlist;
                request.Data.Properties.Title = playlist.Name;
            }

            request.Data.SetStorageItems(filesToShare);

            deferral.Complete();
        }

    }
}
