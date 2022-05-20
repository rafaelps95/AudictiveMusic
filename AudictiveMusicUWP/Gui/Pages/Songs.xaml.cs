using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
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

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            progress.IsActive = true;
            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
            Collection.SongsChanged -= Collection_SongsChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            if (((CollectionViewSource)Resources["ListOfSongs"]).Source == null || CollectionHasBeenUpdated)
            {
                CollectionHasBeenUpdated = false;
                LoadSongs();
            }
            else
            {
                OpenPage(NavMode == NavigationMode.Back);
            }

            Collection.SongsChanged += Collection_SongsChanged;
        }

        private void Collection_SongsChanged(object sender, RoutedEventArgs e)
        {
            LoadSongs();
        }

        private void LoadSongs()
        {
            ((CollectionViewSource)Resources["ListOfSongs"]).Source = null;

            List<AlphaKeyGroup<Song>> itemSource;
            Song aux = null;

            listOfSongs = Ctr_Song.Current.GetSongs(false);

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SongsSortBy"))
            {
                if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByTitle)
                {
                    listOfSongs.OrderBy(s => s.Name);
                    itemSource = AlphaKeyGroup<Song>.CreateGroups(listOfSongs,
    CultureInfo.InvariantCulture,
    a => a.Name, true);
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
                    listOfSongs.OrderBy(s => s.Name);
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
            PopupHelper.GetInstance(sender).ShowPopupMenu(song, true, point);
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

        private async void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
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

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (((SemanticZoom)sender).IsZoomedInViewActive)
            {
                selectionItemsBar.Visibility = Visibility.Visible;
            }
            else
            {
                selectionItemsBar.Visibility = Visibility.Collapsed;
            }
        }

        private void EnableSelectionMode()
        {
            listView.SelectionMode = ListViewSelectionMode.Multiple;
            listView.SelectionChanged += listView_SelectionChanged;
            selectionItemsBar.SelectionModeChanged -= SelectionItemsBar_SelectionModeChanged;
            selectionItemsBar.SelectionMode = SelectedItemsBar.BarMode.Enabled;
            selectionItemsBar.SelectionModeChanged += SelectionItemsBar_SelectionModeChanged;
        }

        private void DisableSelectionMode()
        {
            listView.SelectedItem = null;
            listView.SelectionChanged -= listView_SelectionChanged;
            listView.SelectionMode = ListViewSelectionMode.None;
            selectionItemsBar.SelectionModeChanged -= SelectionItemsBar_SelectionModeChanged;
            selectionItemsBar.SelectionMode = SelectedItemsBar.BarMode.Disabled;
            selectionItemsBar.SelectionModeChanged += SelectionItemsBar_SelectionModeChanged;
        }

        private void ActivateSelecionMode(Album album)
        {
            if (listView.SelectedItems.Contains(album))
                return;

            ApplicationInfo.Current.VibrateDevice(25);
            EnableSelectionMode();
            listView.SelectedItems.Add(album);
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = listView.SelectedItems.Count;
            selectionItemsBar.SelectedItemsCount = i;

            if (i == 0)
            {
                DisableSelectionMode();
            }
        }

        private void SelectionItemsBar_ClearRequest(object sender, RoutedEventArgs e)
        {
            DisableSelectionMode();
        }

        private void SelectionItemsBar_SelectAllRequest(object sender, RoutedEventArgs e)
        {
            listView.SelectAll();
        }

        private void SelectionItemsBar_PlaySelected(object sender, SelectedItemsBar.PlayMode playMode)
        {
            List<string> list = new List<string>();

            foreach (Song song in listView.SelectedItems)
            {
                list.Add(song.SongURI);
            }

            if (playMode == SelectedItemsBar.PlayMode.Play)
                PlayerController.Play(list);
            else
                PlayerController.AddToQueue(list, true);

            DisableSelectionMode();
        }

        private void SelectionItemsBar_AddSelected(object sender, SelectedItemsBar.AddMode addMode)
        {
            List<string> list = new List<string>();

            foreach (Song song in listView.SelectedItems)
            {
                list.Add(song.SongURI);
            }

            if (addMode == SelectedItemsBar.AddMode.AddToPlaylist)
                PlaylistHelper.RequestPlaylistPicker(this, list);
            else
                PlayerController.AddToQueue(list);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_ShareSelected(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (Song song in listView.SelectedItems)
            {
                list.Add(song.SongURI);
            }

            await ShareHelper.Instance.Share(list);

            DisableSelectionMode();
        }

        private void SelectionItemsBar_SelectionModeChanged(object sender, SelectedItemsBar.BarMode barMode)
        {
            if (barMode == SelectedItemsBar.BarMode.Disabled)
                DisableSelectionMode();
            else
                EnableSelectionMode();
        }

        private void SelectionItemsBar_SelectionModeChanged(object sender, object barMode)
        {

        }
    }
}
