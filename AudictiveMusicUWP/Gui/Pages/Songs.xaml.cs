﻿using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class Songs : Page
    {
        private bool updated;

        private NavigationMode NavMode
        {
            get;
            set;
        }

        private List<Song> listOfSongs
        {
            get;
            set;
        }

        public bool CollectionHasBeenUpdated
        {
            get
            {
                return updated;
            }
            set
            {
                updated = value;
                if (value == true)
                {
                    LoadSongs();
                }
            }
        }

        public Songs()
        {
            CollectionHasBeenUpdated = false;

            this.SizeChanged += Songs_SizeChanged;

            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Songs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            selectionGrid.Margin = new Thickness(20, 20, 20, ApplicationInfo.Current.FooterHeight + 20);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            progress.IsActive = true;
            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            PageHelper.Songs = this;

            if (((CollectionViewSource)Resources["ListOfSongs"]).Source == null || CollectionHasBeenUpdated)
            {
                CollectionHasBeenUpdated = false;
                LoadSongs();
            }
            else
            {
                OpenPage(NavMode == NavigationMode.Back);
            }
        }

        public async void LoadSongs()
        {
            ((CollectionViewSource)Resources["ListOfSongs"]).Source = null;

            List<AlphaKeyGroup<Song>> itemSource;
            Song aux = null;

            listOfSongs = Ctr_Song.Current.GetSongs(false);

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SongsSortBy"))
            {
                if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByTitle)
                {
                    listOfSongs.OrderBy(s => s.Title);
                    itemSource = AlphaKeyGroup<Song>.CreateGroups(listOfSongs,
    CultureInfo.InvariantCulture,
    a => a.Title, true);
                }
                else if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByArtist)
                {
                    listOfSongs.OrderBy(s => s.Artist);
                    itemSource = AlphaKeyGroup<Song>.CreateGroups(listOfSongs,
    CultureInfo.InvariantCulture,
    a => a.Artist, true);
                }
                else if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByAlbum)
                {
                    listOfSongs.OrderBy(s => s.Album);
                    itemSource = AlphaKeyGroup<Song>.CreateGroups(listOfSongs,
    CultureInfo.InvariantCulture,
    a => a.Album, true);
                }
                else
                {
                    listOfSongs.OrderBy(s => s.Title);
                    itemSource = AlphaKeyGroup<Song>.CreateGroups(listOfSongs,
    CultureInfo.InvariantCulture,
    a => a.Artist, true);
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["SongsSortBy"] = Sorting.SortByArtist;

                listOfSongs.OrderBy(s => s.Artist);
                itemSource = AlphaKeyGroup<Song>.CreateGroups(listOfSongs,
    CultureInfo.InvariantCulture,
    a => a.Artist, true);
            }

            ((CollectionViewSource)Resources["ListOfSongs"]).Source = itemSource;

            OpenPage(NavMode == NavigationMode.Back);
        }

        private void OpenPage(bool reload)
        {
            progress.IsActive = false;
            Storyboard sb = this.Resources["OpenPageTransition"] as Storyboard;

            if (reload)
            {
                layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;
            }

            sb.Begin();
        }

        private void SongItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                Song song = (sender as FrameworkElement).DataContext as Song;
                CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void SongItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && e.HoldingState == HoldingState.Started)
            {
                Song song = (sender as FrameworkElement).DataContext as Song;
                CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void CreateSongPopup(Song song, object sender, Point point)
        {
            this.ShowPopupMenu(song, sender, point, Enumerators.MediaItemType.Song);
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> songs = Ctr_Song.Current.GetAllSongsPaths();

            Random rng = new Random();
            int n = songs.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = songs[k];
                songs[k] = songs[n];
                songs[n] = value;
            }

            MessageService.SendMessageToBackground(new SetPlaylistMessage(songs));
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SongsList.SelectionMode == ListViewSelectionMode.None)
                EnableSelectionMode();
            else
                DisableSelectionMode();            
        }

        private void EnableSelectionMode()
        {
            selectButton.Content = "";
            SongsList.SelectionMode = ListViewSelectionMode.Multiple;
            SongsList.SelectionChanged += SongsList_SelectionChanged;
            topAppBar.Visibility = Visibility.Visible;
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            DisableSelectionMode();
        }


        private void DisableSelectionMode()
        {
            selectButton.Content = "";
            SongsList.SelectedItem = null;
            SongsList.SelectionChanged -= SongsList_SelectionChanged;
            SongsList.SelectionMode = ListViewSelectionMode.None;
            topAppBar.Visibility = Visibility.Collapsed;
        }


        private void SongsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongsList.SelectedItems.Count > 0)
            {
                topPlay.IsEnabled = topAdd.IsEnabled = topMore.IsEnabled = true;
            }
            else
            {
                topPlay.IsEnabled = topAdd.IsEnabled = topMore.IsEnabled = false;
                selectedItemsLabel.Text = string.Empty;
                selectedItemsLabel.Visibility = Visibility.Collapsed;
                return;
            }

            int i = SongsList.SelectedItems.Count;

            string s = i + " " + ApplicationInfo.Current.GetSingularPlural(i, "ItemSelected");

            selectedItemsLabel.Text = s;
            selectedItemsLabel.Visibility = Visibility.Visible;
        }

        private async void SongsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SongsList.SelectionMode == ListViewSelectionMode.None)
            {
                Song clickedSong = e.ClickedItem as Song;

                await listOfSongs.Shuffle();
                List<string> list = new List<string>();
                list.Add(clickedSong.SongURI);

                foreach (Song s in listOfSongs)
                {
                    if (s.SongURI != clickedSong.SongURI)
                        list.Add(s.SongURI);
                }

                MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
            }
        }

        private void topPlay_Click(object sender, RoutedEventArgs e)
        {
            List<string> listSongs = new List<string>();

            foreach (Song song in SongsList.SelectedItems)
            {
                listSongs.Add(song.SongURI);
            }

            MessageService.SendMessageToBackground(new SetPlaylistMessage(listSongs));

            DisableSelectionMode();
        }

        private void topAdd_Click(object sender, RoutedEventArgs e)
        {
            List<string> listSongs = new List<string>();

            foreach (Song song in SongsList.SelectedItems)
            {
                listSongs.Add(song.SongURI);
            }

            PageHelper.MainPage.CreateAddToPlaylistPopup(listSongs);

            DisableSelectionMode();

        }

        private void topMore_Click(object sender, RoutedEventArgs e)
        {

            List<string> list = new List<string>();

            foreach (Song song in SongsList.SelectedItems)
            {
                list.Add(song.SongURI);
            }

            MenuFlyout menu = new MenuFlyout()
            {
                MenuFlyoutPresenterStyle = Application.Current.Resources["MenuFlyoutModernStyle"] as Style,
            };

            MenuFlyoutItem item1 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("Play"),
                Tag = "",
                Style = Application.Current.Resources["ModernMenuFlyoutItem"] as Style,
            };
            item1.Click += (s, a) =>
            {
                MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
            };

            menu.Items.Add(item1);

            menu.Items.Add(new MenuFlyoutSeparator());

            MenuFlyoutItem item2 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylist"),
                Tag = "",
                Style = Application.Current.Resources["ModernMenuFlyoutItem"] as Style,
            };
            item2.Click += (s, a) =>
            {
                MessageService.SendMessageToBackground(new AddSongsToPlaylist(list));
            };

            menu.Items.Add(item2);

            MenuFlyoutItem item3 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("AddToPlaylistFile"),
                Tag = "",
                Style = Application.Current.Resources["ModernMenuFlyoutItem"] as Style,
            };
            item3.Click += (s, a) =>
            {
                if (PageHelper.MainPage != null)
                {
                    PageHelper.MainPage.CreateAddToPlaylistPopup(list);
                }
            };

            menu.Items.Add(item3);

            MenuFlyoutItem item4 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("PlayNext"),
                Tag = "\uEA52",
                Style = Application.Current.Resources["ModernMenuFlyoutItem"] as Style,
            };
            item4.Click += (s, a) =>
            {
                MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, true));
            };

            menu.Items.Add(item4);

            menu.Items.Add(new MenuFlyoutSeparator());

            MenuFlyoutItem item5 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("Share"),
                Tag = "",
                Style = Application.Current.Resources["ModernMenuFlyoutItem"] as Style,
            };
            item5.Click += async (s, a) =>
            {
                if (await this.ShareMediaItem(list, Enumerators.MediaItemType.Song) == false)
                {
                    MessageDialog md = new MessageDialog(ApplicationInfo.Current.Resources.GetString("ShareErrorMessage"));
                    await md.ShowAsync();
                }
            };

            menu.Items.Add(item5);

            menu.ShowAt(sender as FrameworkElement);


        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (((SemanticZoom)sender).IsZoomedInViewActive)
            {
                selectionGrid.Visibility = Visibility.Visible;
            }
            else
            {
                selectionGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void selection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisableSelectionMode();
        }
    }
}
