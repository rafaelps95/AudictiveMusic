using AudictiveMusicUWP.Gui.Pages;
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

        public static async void ShowPopupMenu(this FrameworkElement fwe, object mediaItem, object sender, Point point, MediaItemType type)
        {
            page = fwe;

            MenuFlyout menu = new MenuFlyout();
            menu.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Right;
            // SE O MENU FOR DO TIPO SONG
            if (type == MediaItemType.Song)
            {
                Song song = Ctr_Song.Current.GetSong(mediaItem as Song);

                MenuFlyoutItem item1 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Play"),
                    Tag = "",
                };
                item1.Click += (s, a) =>
                {
                    PlayerController.Play(song);
                };

                menu.Items.Add(item1);

                MenuFlyoutItem item2 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                    Tag = "",
                };
                item2.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(song);
                };

                menu.Items.Add(item2);

                MenuFlyoutItem item3 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                    Tag = "",
                };
                item3.Click += (s, a) =>
                {
                    List<string> songs = new List<string>();
                    songs.Add(song.SongURI);

                    if (PageHelper.MainPage != null)
                    {
                        PageHelper.MainPage.CreateAddToPlaylistPopup(songs);
                    }
                };

                menu.Items.Add(item3);


                MenuFlyoutItem item4 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                    Tag = "\uEA52",
                };
                item4.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(song, true);
                };

                menu.Items.Add(item4);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item5 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Share"),
                    Tag = "",
                };
                item5.Click += async (s, a) =>
                {
                    if (await page.ShareMediaItem(song, type) == false)
                    {
                        MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                        await md.ShowAsync();
                    }
                };

                menu.Items.Add(item5);

                MenuFlyoutItem item6 = new MenuFlyoutItem();

                if (song.IsFavorite)
                {
                    item6.Text = ApplicationInfo.Current.Resources.GetString("RemoveFromFavoritesString");
                    item6.Tag = "";
                }
                else
                {
                    item6.Text = ApplicationInfo.Current.Resources.GetString("AddToFavoritesString");
                    item6.Tag = "";
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

                MenuFlyoutItem item7 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("GoToArtistString"),
                    Tag = "",
                };
                item7.Click += (s, a) =>
                {
                    Artist artist = new Artist();
                    artist.Name = song.Artist;

                    PageHelper.MainPage?.Navigate(typeof(ArtistPage), artist);
                };

                menu.Items.Add(item7);

                MenuFlyoutItem item8 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("GoToAlbumString"),
                    Tag = "",
                };
                item8.Click += (s, a) =>
                {
                    Album album = new Album()
                    {
                        Name = song.Album,
                        Artist = song.Artist,
                        AlbumID = song.AlbumID,
                        Year = Convert.ToInt32(song.Year),
                        Genre = song.Genre,
                        HexColor = song.HexColor
                    };

                    PageHelper.MainPage?.Navigate(typeof(AlbumPage), album);
                };

                menu.Items.Add(item8);
            }
            // SE O MENU FOR DO TIPO ALBUM
            else if (type == MediaItemType.Album)
            {
                Album album = mediaItem as Album;

                MenuFlyoutItem item1 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Play"),
                    Tag = "",
                };
                item1.Click += (s, a) =>
                {
                    PlayerController.Play(album);
                };

                menu.Items.Add(item1);

                MenuFlyoutItem item2 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                    Tag = "",
                };
                item2.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(album);
                };

                menu.Items.Add(item2);

                MenuFlyoutItem item3 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                    Tag = "",
                };
                item3.Click += (s, a) =>
                {
                    if (PageHelper.MainPage != null)
                    {
                        PageHelper.MainPage.CreateAddToPlaylistPopup(album);
                    }
                };

                menu.Items.Add(item3);

                MenuFlyoutItem item4 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                    Tag = "\uEA52",
                    
                };
                item4.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(album, true);
                };

                menu.Items.Add(item4);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item5 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Share"),
                    Tag = "",
                    
                };
                item5.Click += async (s, a) =>
                {
                    if (await page.ShareMediaItem(album, type) == false)
                    {
                        MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                        await md.ShowAsync();
                    }
                };

                menu.Items.Add(item5);

                MenuFlyoutItem item6 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("GoToArtistString"),
                    Tag = "",
                    
                };
                item6.Click += (s, a) =>
                {
                    Artist artist = new Artist();
                    artist.Name = album.Artist;

                    PageHelper.MainPage?.Navigate(typeof(ArtistPage), artist);
                };

                menu.Items.Add(item6);
            }
            // SE O MENU FOR DO TIPO ARTIST
            else if (type == MediaItemType.Artist)
            {
                Artist artist = mediaItem as Artist;

                MenuFlyoutItem item1 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Play"),
                    Tag = "",
                    
                };
                item1.Click += (s, a) =>
                {
                    PlayerController.Play(artist);
                };

                menu.Items.Add(item1);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item2 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                    Tag = "",
                    
                };
                item2.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(artist);
                };

                menu.Items.Add(item2);

                MenuFlyoutItem item3 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                    Tag = "",
                    
                };
                item3.Click += (s, a) =>
                {
                    if (PageHelper.MainPage != null)
                    {
                        PageHelper.MainPage.CreateAddToPlaylistPopup(artist);
                    }
                };

                menu.Items.Add(item3);

                MenuFlyoutItem item4 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                    Tag = "\uEA52",
                    
                };
                item4.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(artist, true);
                };

                menu.Items.Add(item4);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item5 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Share"),
                    Tag = "",
                    
                };
                item5.Click += async (s, a) =>
                {
                    if (await page.ShareMediaItem(artist, type) == false)
                    {
                        MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                        await md.ShowAsync();
                    }
                };

                menu.Items.Add(item5);

            }
            //SE O MENU FOR DO TIPO FOLDER
            else if (type == MediaItemType.Folder)
            {
                FolderItem folder = mediaItem as FolderItem;

                MenuFlyoutItem item1 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Play"),
                    Tag = "",
                    
                };
                item1.Click += (s, a) =>
                {
                    PlayerController.Play(folder);
                };

                menu.Items.Add(item1);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item2 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                    Tag = "",
                    
                };
                item2.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(folder);
                };

                menu.Items.Add(item2);

                MenuFlyoutItem item3 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                    Tag = "",
                    
                };
                item3.Click += (s, a) =>
                {
                    if (PageHelper.MainPage != null)
                    {
                        PageHelper.MainPage.CreateAddToPlaylistPopup(folder);
                    }
                };

                menu.Items.Add(item3);

                MenuFlyoutItem item4 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                    Tag = "\uEA52",
                    
                };
                item4.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(folder, true);
                };

                menu.Items.Add(item4);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item5 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Share"),
                    Tag = "",
                    
                };
                item5.Click += async (s, a) =>
                {
                    if (await page.ShareMediaItem(folder, type) == false)
                    {
                        MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                        await md.ShowAsync();
                    }
                };

                menu.Items.Add(item5);
            }
            // SE O MENU FOR DO TIPO PLAYLIST
            else if (type == MediaItemType.Playlist)
            {
                Playlist playlist = mediaItem as Playlist;

                MenuFlyoutItem item1 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Play"),
                    Tag = "",
                    
                };
                item1.Click += (s, a) =>
                {
                    PlayerController.Play(playlist);
                };

                menu.Items.Add(item1);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item2 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                    Tag = "",
                    
                };
                item2.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(playlist);
                };

                menu.Items.Add(item2);


                MenuFlyoutItem item3 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                    Tag = "\uEA52",
                    
                };
                item3.Click += (s, a) =>
                {
                    PlayerController.AddToPlaylist(playlist, true);
                };

                menu.Items.Add(item3);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item4 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Share"),
                    Tag = "",
                    
                };
                item4.Click += async (s, a) =>
                {
                    if (await page.ShareMediaItem(playlist, type) == false)
                    {
                        MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                        await md.ShowAsync();
                    }
                };

                menu.Items.Add(item4);

                menu.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutItem item5 = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("Delete"),
                    Tag = "",
                    
                };
                item5.Click += async (s, a) =>
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
                                await playlistItem.DeleteAsync(StorageDeleteOption.PermanentDelete);

                                if (PageHelper.PlaylistPage != null)
                                {
                                    if (PageHelper.MainPage != null)
                                        PageHelper.MainPage.GoBack();
                                }
                                else if (PageHelper.Playlists != null)
                                {
                                    PageHelper.Playlists.LoadPlaylists();
                                }
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

                    
                };

                menu.Items.Add(item5);
            }

            try
            {
                menu.ShowAt(sender as FrameworkElement);
                menu.ShowAt(sender as FrameworkElement, point);
            }
            catch
            {
                menu.ShowAt(sender as FrameworkElement);
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
                    PageHelper.MainPage.CreateLastFmLogin();
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
                        PageHelper.MainPage.Navigate(typeof(LastFmProfilePage), user);
                    };

                    mf.Items.Add(mfi);

                    MenuFlyoutItem mfi2 = new MenuFlyoutItem()
                    {
                        Text = ApplicationInfo.Current.Resources.GetString("ScrobbleSettings"),
                    };

                    mfi2.Click += (s, a) =>
                    {
                        PageHelper.MainPage.Navigate(typeof(Settings), "path=scrobble");
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
                        PageHelper.MainPage.Navigate(typeof(LastFmProfilePage), user);
                    };

                    mf.Items.Add(mfi);
                }
            }

            mf.ShowAt(fwe);
        }
    }
}
