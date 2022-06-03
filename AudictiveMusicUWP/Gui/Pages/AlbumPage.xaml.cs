using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        private ResourceLoader res;

        private List<Song> listOfSongs
        {
            get;
            set;
        }

        private NavigationMode NavMode
        {
            get;
            set;
        }

        private Album Album
        {
            get;
            set;
        }

        private double _selectionBarOffset;
        private bool _selectionBarOnTop = false;

        public AlbumPage()
        {

            this.SizeChanged += AlbumPage_SizeChanged;
            this.InitializeComponent();


            res = new ResourceLoader();            
        }

        private void AlbumPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _selectionBarOffset = header.ActualHeight;
            selectionItemsBar.Width = e.NewSize.Width;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            header.ClearContext();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;
            Album = e.Parameter as Album;
            header.SetContext(Album);

            LoadSongs();
        }

        private async void LoadSongs()
        {
            listOfSongs = Ctr_Song.Current.GetSongsByAlbum(Album);

            await Task.Delay(10);
            listView.ItemsSource = listOfSongs.OrderBy(s => s.TrackNumber);

            header.UpdateNumberOfItems(listOfSongs.Count);

            progress.IsActive = false;
        }

        private void artistName_Click(object sender, RoutedEventArgs e)
        {
            Artist artist = new Artist()
            {
                Name = Album.Artist,
            };

            Frame.Navigate(typeof(ArtistPage), artist);
        }

        private void playAlbum_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.Play(this.Album);
        }

        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AddToNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.AddToQueue(this.Album);
        }

        private void SongItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
            {
                if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    Song song = (sender as FrameworkElement).DataContext as Song;
                    CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
                }
            }
        }

        private void SongItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
            {
                if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && e.HoldingState == HoldingState.Started)
                {
                    Song song = (sender as FrameworkElement).DataContext as Song;
                    CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
                }
            }
        }

        private void CreateSongPopup(Song song, object sender, Point point)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(song, true, point);
        }

        private void moreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(Album, true, new Point(0, 0));
        }

        private void SongsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
            {
                Song clickedSong = e.ClickedItem as Song;

                List<string> list = new List<string>();

                listOfSongs.Shuffle();
                list.Add(clickedSong.SongURI);

                foreach (Song s in listOfSongs)
                {
                    if (s.SongURI != clickedSong.SongURI)
                        list.Add(s.SongURI);
                }

                PlayerController.Play(list);
            }
        }


        private void EnableSelectionMode()
        {
            listView.SelectionMode = ListViewSelectionMode.Multiple;
            listView.SelectionChanged += listView_SelectionChanged;
            listView.Padding = new Thickness(0,60,0,110);
            selectionItemsBar.SelectionModeChanged -= SelectionItemsBar_SelectionModeChanged;
            selectionItemsBar.SelectionMode = SelectedItemsBar.BarMode.Enabled;
            selectionItemsBar.SelectionModeChanged += SelectionItemsBar_SelectionModeChanged;
        }

        private void DisableSelectionMode()
        {
            listView.SelectedItem = null;
            listView.SelectionChanged -= listView_SelectionChanged;
            listView.SelectionMode = ListViewSelectionMode.None;
            listView.Padding = new Thickness(0, 0, 0, 110);
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

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset > _selectionBarOffset)
            {
                if (_selectionBarOnTop)
                    return;

                _selectionBarOnTop = true;
                scrollContent.Children.Remove(selectionItemsBar);
                layoutRoot.Children.Add(selectionItemsBar);
            }
            else
            {
                if (!_selectionBarOnTop)
                    return;

                _selectionBarOnTop = false;
                layoutRoot.Children.Remove(selectionItemsBar);
                scrollContent.Children.Add(selectionItemsBar);
            }
        }
    }
}
