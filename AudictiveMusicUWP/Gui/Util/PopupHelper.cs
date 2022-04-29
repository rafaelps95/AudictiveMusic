﻿using AudictiveMusicUWP.Gui.Pages;
using AudictiveMusicUWP.Gui.Pages.LFM;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static ClassLibrary.Helpers.Enumerators;

namespace AudictiveMusicUWP.Gui.Util
{
    public static class PopupHelper
    {
        private static ResourceLoader res = new ResourceLoader();
        private static object page;

        public static void ShowPopupMenu(this FrameworkElement fwe, object mediaItem, object sender, MediaItemType mediaItemType, bool showAtPoint = false, Point point = default(Point))
        {
            page = fwe;

            MenuFlyout menu = new MenuFlyout();
            menu.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Right;

            MenuFlyoutItem item1 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("Play"),
                Tag = "\uE102",
            };
            item1.Click += (s, a) =>
            {
                PlayerController.Play(mediaItem, mediaItemType);
            };

            menu.Items.Add(item1);

            MenuFlyoutItem item2 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                Tag = "\uE846",
            };
            item2.Click += (s, a) =>
            {
                PlayerController.AddToQueue(mediaItem, mediaItemType, true);
            };

            menu.Items.Add(item2);


            menu.Items.Add(new MenuFlyoutSeparator());


            MenuFlyoutItem item3 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                Tag = "\uE109",
            };
            item3.Click += (s, a) =>
            {
                PlayerController.AddToQueue(mediaItem, mediaItemType);
            };

            menu.Items.Add(item3);

            MenuFlyoutItem item4 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                Tag = "\uE7AC",
            };
            item4.Click += async (s, a) =>
            {
                List<string> list = await PlayerController.FetchSongs(mediaItem, mediaItemType);

                PlaylistHelper.RequestPlaylistPicker(sender, list);
            };

            menu.Items.Add(item4);


            menu.Items.Add(new MenuFlyoutSeparator());


            MenuFlyoutItem item5 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("Share"),
                Tag = "\uE72D",
            };
            item5.Click += async (s, a) =>
            {
                if (await page.ShareMediaItem(mediaItem, mediaItemType) == false)
                {
                    MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                    await md.ShowAsync();
                }
            };

            menu.Items.Add(item5);

            if (mediaItemType == MediaItemType.Playlist)
            {
                Playlist playlist = mediaItem as Playlist;
                MenuFlyoutItem item = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Delete"),
                    Tag = "\uE107"
                };
                item.Click += (s, a) =>
                {
                    PlaylistHelper.DeletePlaylist(sender, playlist);
                };

                menu.Items.Add(item);
            }


            if (mediaItemType == MediaItemType.Song)
            {
                menu.Items.Add(new MenuFlyoutSeparator());

                Song song = Ctr_Song.Current.GetSong(mediaItem as Song);
                MenuFlyoutItem item6 = new MenuFlyoutItem();

                if (song.IsFavorite)
                {
                    item6.Text = ApplicationInfo.Current.Resources.GetString("RemoveFromFavoritesString");
                    item6.Tag = "\uE00C";
                }
                else
                {
                    item6.Text = ApplicationInfo.Current.Resources.GetString("AddToFavoritesString");
                    item6.Tag = "\uE00B";
                }

                item6.Click += (s, a) =>
                {
                    //remove favorite mark
                    if (song.IsFavorite)
                    {
                        Ctr_Song.Current.SetFavoriteState(song, false);
                    }
                    //add favorite mark
                    else
                    {
                        Ctr_Song.Current.SetFavoriteState(song, true);
                    }
                };

                menu.Items.Add(item6);
            }

            if (mediaItemType == MediaItemType.Song || mediaItemType == MediaItemType.Album)
            {
                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item7 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("GoToArtistString"),
                    Tag = "\uE13D",
                };
                item7.Click += (s, a) =>
                {
                    Artist artist = new Artist();
                    if (mediaItemType == MediaItemType.Album)
                    {
                        Album album = mediaItem as Album;
                        artist.Name = album.Artist;
                    }
                    else
                    {
                        Song song = mediaItem as Song;
                        artist.Name = song.Artist;
                    }

                    NavigationHelper.Navigate(sender, typeof(ArtistPage), artist);
                };

                menu.Items.Add(item7);

                if (mediaItemType != MediaItemType.Album)
                {

                    MenuFlyoutItem item8 = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("GoToAlbumString"),
                        Tag = "\uE958",
                    };
                    item8.Click += (s, a) =>
                    {
                        Song song = mediaItem as Song;
                        Album album = new Album()
                        {
                            Name = song.Album,
                            Artist = song.Artist,
                            AlbumID = song.AlbumID,
                            Year = Convert.ToInt32(song.Year),
                            Genre = song.Genre,
                            HexColor = song.HexColor
                        };

                        NavigationHelper.Navigate(sender, typeof(AlbumPage), album);
                    };

                    menu.Items.Add(item8);
                }
            }

            menu.ShowAt(sender as FrameworkElement);

            if (showAtPoint)
            {
                menu.ShowAt(sender as FrameworkElement, point);
            }
        }

        public static void ShowLastFmPopupMenu(this FrameworkElement fwe, LastUser user)
        {
            MenuFlyout mf = new MenuFlyout();

            if (LastFm.Current.IsAuthenticated == false)
            {
                MenuFlyoutItem mfi = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("SignInLastFm"),
                };

                mfi.Click += (s, a) =>
                {

                    LastFm.Current.RequestLogin(fwe);
                };

                mf.Items.Add(mfi);
            }
            else
            {
                if (user.Name.ToLower() == ApplicationSettings.LastFmSessionUsername.ToLower())
                {
                    MenuFlyoutItem mfi = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("OpenProfile"),
                    };

                    mfi.Click += (s, a) =>
                    {
                        NavigationHelper.Navigate(fwe, typeof(LastFmProfilePage), user);
                    };

                    mf.Items.Add(mfi);

                    MenuFlyoutItem mfi2 = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("ScrobbleSettings"),
                    };

                    mfi2.Click += (s, a) =>
                    {
                        NavigationHelper.Navigate(fwe, typeof(Settings), "path=scrobble");
                    };

                    mf.Items.Add(mfi2);

                    MenuFlyoutItem mfi3 = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("SignOut"),
                    };

                    mfi3.Click += (s, a) =>
                    {
                        LastFm.Current.Logout();
                    };

                    mf.Items.Add(mfi3);
                }
                else
                {
                    MenuFlyoutItem mfi = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("VisitProfile"),
                    };

                    mfi.Click += (s, a) =>
                    {
                        NavigationHelper.Navigate(fwe, typeof(LastFmProfilePage), user);
                    };

                    mf.Items.Add(mfi);
                }
            }

            mf.ShowAt(fwe);
        }
    }
}
