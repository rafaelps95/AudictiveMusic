using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Dao;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    public sealed partial class Favorites : Page
    {
        private bool updated;

        private NavigationMode NavMode
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

        public Favorites()
        {
            this.SizeChanged += Favorites_SizeChanged;
            CollectionHasBeenUpdated = false;
            this.InitializeComponent();

            SongDao.FavoritesChanged += Collection_FavoritesChanged;
        }

        private void Collection_FavoritesChanged()
        {
            LoadSongs();
        }

        private void Favorites_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            LoadSongs();
        }

        private void LoadSongs()
        {

            List<Song> songs = Ctr_Song.Current.GetFavoriteSongs();

            listView.ItemsSource = songs;

            progress.IsActive = false;

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
            this.ShowPopupMenu(song, sender, Enumerators.MediaItemType.Song, true, point);
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            List<Song> songs = Ctr_Song.Current.GetFavoriteSongs();

            List<string> list = new List<string>();

            foreach (Song s in songs)
                list.Add(s.SongURI);

            Random rng = new Random();
            int n = songs.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
            {
                Song clickedSong = e.ClickedItem as Song;

                MessageService.SendMessageToBackground(new SetPlaylistMessage(new List<string>() { clickedSong.SongURI }));
            }
        }

        private void ThisDeviceButton_Checked(object sender, RoutedEventArgs e)
        {
            //lastFmFavoritesButton.IsChecked = false;
        }

        private void ThisDeviceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LastFmFavoritesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LastFmFavoritesButton_Checked(object sender, RoutedEventArgs e)
        {
            //thisDeviceButton.IsChecked = false;
        }







        private void EnableSelectionMode()
        {
            Thickness padding = new Thickness(listView.Padding.Left, 70, listView.Padding.Right, listView.Padding.Bottom);
            listView.Padding = padding;

            listView.SelectionMode = ListViewSelectionMode.Multiple;
            listView.SelectionChanged += listView_SelectionChanged;
            selectionItemsBar.SelectionModeChanged -= SelectionItemsBar_SelectionModeChanged;
            selectionItemsBar.SelectionMode = SelectedItemsBar.BarMode.Enabled;
            selectionItemsBar.SelectionModeChanged += SelectionItemsBar_SelectionModeChanged;
        }

        private void DisableSelectionMode()
        {
            Thickness padding = new Thickness(listView.Padding.Left, 0, listView.Padding.Right, listView.Padding.Bottom);
            listView.Padding = padding;

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
                PlayerController.Play(list, Enumerators.MediaItemType.ListOfStrings);
            else
                PlayerController.AddToQueue(list, Enumerators.MediaItemType.ListOfStrings, true);

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
                PlayerController.AddToQueue(list, Enumerators.MediaItemType.ListOfStrings);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_ShareSelected(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (Song song in listView.SelectedItems)
            {
                list.Add(song.SongURI);
            }

            await this.ShareMediaItem(list, Enumerators.MediaItemType.ListOfStrings);

            DisableSelectionMode();
        }

        private void SelectionItemsBar_SelectionModeChanged(object sender, SelectedItemsBar.BarMode barMode)
        {
            if (barMode == SelectedItemsBar.BarMode.Disabled)
                DisableSelectionMode();
            else
                EnableSelectionMode();
        }

    }
}
