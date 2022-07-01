using AudictiveMusicUWP.Gui.Pages;
using AudictiveMusicUWP.Gui.Pages.LFM;
using BackgroundAudioShared.Messages;
using ClassLibrary;
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
using ClassLibrary.Helpers.Enumerators;

namespace AudictiveMusicUWP.Gui.Util
{
    public class PopupHelper
    {
        private static readonly object _lock = new object();

        private static PopupHelper _instance;

        private object _sender;

        public static PopupHelper GetInstance(object sender)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new PopupHelper();
                }
            }

            _instance._sender = sender;
            return _instance;
        }

        private PopupHelper()
        {

        }

        public void ShowPopupMenu(MediaItem mediaItem, bool showAtPoint = false, Point point = default(Point))
        {
            MenuFlyout menu = new MenuFlyout();
            menu.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Right;

            MenuFlyoutItem item1 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("Play"),
                Tag = "\uE102",
            };
            item1.Click += (s, a) =>
            {
                PlayerController.Play(mediaItem);
            };

            menu.Items.Add(item1);

            MenuFlyoutItem item2 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                Tag = "\uE846",
            };
            item2.Click += (s, a) =>
            {
                PlayerController.AddToQueue(mediaItem, true);
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
                PlayerController.AddToQueue(mediaItem);
            };

            menu.Items.Add(item3);

            MenuFlyoutItem item4 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                Tag = "\uE7AC",
            };
            item4.Click += async (s, a) =>
            {
                List<string> list = await Collection.FetchSongs(mediaItem);

                PlaylistHelper.RequestPlaylistPicker(_sender, list);
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
                if (await ShareHelper.Instance.Share(mediaItem) == false)
                {
                    MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                    await md.ShowAsync();
                }
            };

            menu.Items.Add(item5);

            if (mediaItem.GetType() == typeof(Playlist))
            {
                Playlist playlist = mediaItem as Playlist;
                MenuFlyoutItem item = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Delete"),
                    Tag = "\uE107"
                };
                item.Click += (s, a) =>
                {
                    PlaylistHelper.DeletePlaylist(_sender, playlist);
                };

                menu.Items.Add(item);
            }


            if (mediaItem.GetType() == typeof(Song))
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

            if (mediaItem.GetType() == typeof(Song) || mediaItem.GetType() == typeof(Album))
            {
                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item7 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("GoToArtistString"),
                    Tag = "\uE13D",
                };
                item7.Click += (s, a) =>
                {
                    string artistName;
                    if (mediaItem.GetType() == typeof(Song))
                    {
                        Song song = mediaItem as Song;
                        artistName = song.Artist;
                    }
                    else
                    {
                        Album album = mediaItem as Album;
                        artistName = album.Artist;
                    }

                    Artist artist = new Artist();
                    artist.Name = artistName;

                    NavigationService.Navigate(_sender, typeof(ArtistPage), artist);
                };

                menu.Items.Add(item7);

                if (mediaItem.GetType() == typeof(Song))
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
                            ID = song.AlbumID,
                            Year = Convert.ToInt32(song.Year),
                            Genre = song.Genre,
                            HexColor = song.HexColor
                        };

                        NavigationService.Navigate(_sender, typeof(AlbumPage), album);
                    };

                    menu.Items.Add(item8);
                }
            }

            if (mediaItem.GetType() == typeof(Artist))
            {
                if (ApplicationInfo.Current.HasInternetConnection)
                {
                    menu.Items.Add(new MenuFlyoutSeparator());

                    MenuFlyoutItem item9 = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("ArtistOnLastfm"),
                        Tag = "\uE8A7",
                    };
                    item9.Click += async (s, a) =>
                    {
                        var result = await LastFm.Current.Client.Artist.GetInfoAsync(((Artist)mediaItem).Name, ApplicationInfo.Current.Language, true);
                        if (result.Success)
                        {
                            LastArtist artist = result.Content;

                            NavigationService.Navigate(this, typeof(LastFmProfilePage), artist);
                        }
                    };

                    menu.Items.Add(item9);

                }
            }

            menu.ShowAt(_sender as FrameworkElement);

            if (showAtPoint)
            {
                menu.ShowAt(_sender as FrameworkElement, point);
            }
        }

        public void ShowLastFmPopupMenu(LastUser user)
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

                    LastFm.Current.RequestLogin(_sender);
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
                        NavigationService.Navigate(_sender, typeof(LastFmProfilePage), user);
                    };

                    mf.Items.Add(mfi);

                    MenuFlyoutItem mfi2 = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("ScrobbleSettings"),
                    };

                    mfi2.Click += (s, a) =>
                    {
                        NavigationService.Navigate(_sender, typeof(Settings), "path=scrobble");
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
                        NavigationService.Navigate(_sender, typeof(LastFmProfilePage), user);
                    };

                    mf.Items.Add(mfi);
                }
            }

            mf.ShowAt(_sender as FrameworkElement);
        }
    }
}
