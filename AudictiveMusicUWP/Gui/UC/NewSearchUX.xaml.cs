using AudictiveMusicUWP.Gui.Pages;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class NewSearchUX : UserControl
    {
        public delegate void UIDismissedEventArgs(object sender);
        public event UIDismissedEventArgs UIDismissed;
        public event SizeChangedEventHandler SearchBarSizeChanged;

        //private bool stretchBar = false;
        private bool anyResult = false;

        private bool iscompact;

        public bool IsCompact
        {
            get
            {
                return iscompact;
            }
            set
            {
                iscompact = value;
                SetCompactMode(value);
            }
        }

        private void SetCompactMode(bool value)
        {
            if (value)
            {
                if (SearchMode == SearchPaneMode.Closed)
                {
                    searchButton.Visibility = Visibility.Visible;
                    searchBox.Visibility = Visibility.Collapsed;
                    searchBox.Opacity = 0;
                }
            }
            else
            {
                searchButton.Visibility = Visibility.Collapsed;
                searchBox.Visibility = Visibility.Visible;
                searchBox.Opacity = 1;
            }
        }

        public double SearchBoxWidth
        {
            get { return (double)GetValue(SearchBoxWidthProperty); }
            set { SetValue(SearchBoxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchBoxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchBoxWidthProperty =
            DependencyProperty.Register("SearchBoxWidth", typeof(double), typeof(NewSearchUX), new PropertyMetadata((double)300));




        public double AlbumItemLength
        {
            get
            {
                return (double)GetValue(AlbumItemLengthProperty);
            }
            set
            {
                SetValue(AlbumItemLengthProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlbumItemLengthProperty =
            DependencyProperty.Register("AlbumItemLength", typeof(double), typeof(NewSearchUX), new PropertyMetadata(150));


        private List<Song> listOfSongs;
        private string CurrentSearchTerm;
        private int keyPressTimer;
        private DispatcherTimer timer;

        public enum OpenBehavior
        {
            MoveToCenter,
            StayInPosition
        }

        public enum SearchPlacement
        {
            Left,
            Center,
            Right
        }

        private OpenBehavior behavior;

        public OpenBehavior Behavior
        {
            get
            {
                return behavior;
            }
            set
            {
                if (behavior == OpenBehavior.MoveToCenter)
                {

                }

                behavior = value;
            }
        }

        private SearchPlacement placement = SearchPlacement.Right;

        public enum SearchPaneMode
        {
            Closed,
            Open
        }

        private double boxOffset
        {
            get;set;
        }
        private double PaneWidth;

        private SearchPaneMode mode;
        public SearchPaneMode SearchMode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                UpdateSearchMode();
            }
        }

        private void UpdateSearchMode()
        {
            if (SearchMode == SearchPaneMode.Closed)
            {
                LoseFocus(searchBox);
                CloseSearchPane();
            }
            else
            {
                OpenSearchPane();
                searchBox.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Makes virtual keyboard disappear
        /// </summary>
        /// <param name="sender"></param>
        private void LoseFocus(Control control)
        {
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        public NewSearchUX()
        {
            this.SizeChanged += NewSearchUX_SizeChanged;
            this.InitializeComponent();
            this.keyPressTimer = 0;
        }

        /// <summary>
        /// Sets the horizontal offset (left or right) based on content.
        /// Used to display the search bar/button correctly when the search UI is minimized.
        /// The offset is NOT applied when the search UI is open or centralized.
        /// </summary>
        /// <param name="offset"></param>
        public void SetOffset(double offset)
        {
            boxOffset = offset;

            if (placement == SearchPlacement.Left)
            {
                if (SearchMode == SearchPaneMode.Closed)
                    boxTranslate.X = offset;
                searchButton.Margin = new Thickness(offset, 9, 0, 4);
            }
            else if (placement == SearchPlacement.Right)
            {
                if (SearchMode == SearchPaneMode.Closed)
                    boxTranslate.X = offset *-1;
                searchButton.Margin = new Thickness(0, 9, offset, 4);
            }
            else
            {
                boxTranslate.X = 0;
                searchButton.Margin = new Thickness(0, 9, 0, 4);
            }
        }

        /// <summary>
        /// Sets the horizontal alignment of the control when the search UI is minimized.
        /// The UI is always centralized when its SearchMode is set to Open.
        /// </summary>
        /// <param name="sbp">Horizontal alignment of the control while minimized</param>
        public void SetSearchBarPlacement(SearchPlacement sbp)
        {
            placement = sbp;

            if (placement == SearchPlacement.Left)
            {
                box.HorizontalAlignment = searchButton.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (placement == SearchPlacement.Right)
            {
                box.HorizontalAlignment = searchButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                box.HorizontalAlignment = searchButton.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        public SearchPlacement GetSearchBarPlacement()
        {
            return placement;
        }

        private void NewSearchUX_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.PaneWidth = e.NewSize.Width;

            if (this.SearchMode == SearchPaneMode.Open)
                boxTranslate.X = GetTranslateXToCenter();
            else
                SetSearchBarPlacement(placement);
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchMode = SearchPaneMode.Open;
        }

        /// <summary>
        /// Applies an animation to place the search bar correctly in the center of the page
        /// </summary>
        private void OpenSearchPane()
        {
            searchButton.Visibility = Visibility.Collapsed;
            dismissArea.IsHitTestVisible = true;
            resultsGrid.Opacity = 1;
            resultsGrid.IsHitTestVisible = true;

            double x = GetTranslateXToCenter();

            Animation animation = new Animation();
            if (IsCompact)
            {
                boxTranslate.X = x;
                searchBox.Opacity = 0;
                searchBox.Visibility = Visibility.Visible;

                animation.AddDoubleAnimation(1, 800, searchBox, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseInOut));
            }
            else
            {
                animation.AddDoubleAnimation(x, 800, boxTranslate, "X", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseInOut));
            }

            animation.AddDoubleAnimation(0.6, 800, dismissArea, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseInOut));

            animation.Begin();
        }

        /// <summary>
        /// Returns the necessary offset to center the bar into the page
        /// </summary>
        private double GetTranslateXToCenter()
        {
            double center = this.PaneWidth / 2;
            double x = center - (this.SearchBoxWidth / 2);

            if (placement == SearchPlacement.Left)
                return x;
            else if (placement == SearchPlacement.Right)
                return x * -1;
            else
                return 0;
        }

        /// <summary>
        /// Applies an animation to restore its original position
        /// </summary>
        private void CloseSearchPane()
        {
            if (IsCompact)
            {
                searchButton.Visibility = Visibility.Visible;
                searchBox.Visibility = Visibility.Collapsed;
                searchBox.Opacity = 0;
                boxTranslate.X = boxOffset;
            }

            dismissArea.IsHitTestVisible = false;
            resultsGrid.Opacity = 0;
            resultsGrid.IsHitTestVisible = false;

            double translateTo;
            if (placement == SearchPlacement.Left)
                translateTo = boxOffset;
            else if (placement == SearchPlacement.Right)
                translateTo = boxOffset * -1;
            else
                translateTo = GetTranslateXToCenter();

            Animation animation = new Animation();
            animation.AddDoubleAnimation(translateTo, 800, boxTranslate, "X", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseInOut));
            animation.AddDoubleAnimation(0, 800, dismissArea, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseInOut));

            animation.Begin();

            searchBox.TextChanged -= searchBox_TextChanged;
            searchBox.Text = "";
            searchBox.TextChanged += searchBox_TextChanged;

            ClearResults();
        }

        /// <summary>
        /// Prepares the UI to be cleaned
        /// </summary>
        private void ClearResults()
        {
            ResetSearch();
        }

        private void Box_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SearchBarSizeChanged?.Invoke(this, e);
        }

        private void DismissArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SearchMode = SearchPaneMode.Closed;
            UIDismissed?.Invoke(this);
        }



        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();

            if (searchBox.Text.Length > 3)
            {
                this.CurrentSearchTerm = searchBox.Text;

                RunSearch(this.CurrentSearchTerm);
            }
        }

        private void Album_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch)
            {
                Album album = (sender as FrameworkElement).DataContext as Album;

                CreateAlbumPopup(album, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void CreateAlbumPopup(Album album, object sender, Point point)
        {
            if (point == null)
                PopupHelper.GetInstance(sender).ShowPopupMenu(album);
            else
                PopupHelper.GetInstance(sender).ShowPopupMenu(album, true, point);
        }

        private void playAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Album alb = btn.DataContext as Album;

            PlayerController.Play(alb);
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

        private void Artist_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 1200, sender as Image, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut), false, 200);

            animation.Begin();
        }

        private async void DownloadImage(Artist artist)
        {
            try
            {
                //LastFm lfm = new LastFm();
                //lfm.DownloadCompleted += Lfm_DownloadCompleted;
                await Spotify.DownloadArtistImage(artist);
            }
            catch
            {

            }
        }

        private void albumsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["UseTransition"] = true;
            NavigationService.Navigate(this, typeof(AlbumPage), e.ClickedItem);
        }

        private void AlbumCover_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 1200, sender as Image, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut), false, 200);

            animation.Begin();
        }

        private void albumItemOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                Animation animation = new Animation();
                animation.AddDoubleAnimation(0.7, 200, sender as Border, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

                animation.Begin();
            }
        }

        private void albumItemOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                Animation animation = new Animation();
                animation.AddDoubleAnimation(0, 200, sender as Border, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

                animation.Begin();
            }
        }

        private void Artist_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Touch && e.HoldingState == HoldingState.Started)
            {
                Artist artist = (sender as FrameworkElement).DataContext as Artist;

                CreateArtistPopup(artist, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void Artist_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch)
            {
                Artist artist = (sender as FrameworkElement).DataContext as Artist;

                CreateArtistPopup(artist, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void CreateArtistPopup(Artist artist, object sender, Point point)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(artist, true, point);
        }

        private void artistsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(this, typeof(ArtistPage), e.ClickedItem);
        }

        private void CircleImage_ImageFailed(object sender, RoutedEventArgs e)
        {
            Artist art = ((FrameworkElement)sender).DataContext as Artist;

            DownloadImage(art);
        }

        private void CircleImage_ActionClick(object sender, EventArgs e)
        {
            CircleImage cimg = (CircleImage)sender;
            Artist art = cimg.DataContext as Artist;

            PlayerController.Play(art);
        }

        private void AlbumItem_MenuTriggered(object sender, Point point)
        {
            Album album = (sender as FrameworkElement).DataContext as Album;

            CreateAlbumPopup(album, sender, point);
        }

        private void searchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;

                if (timer != null)
                    timer.Stop();
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;

                this.CurrentSearchTerm = searchBox.Text;
                RunSearch(this.CurrentSearchTerm);
            }
        }

        private void RunSearch(string searchTerm)
        {
            //progress.IsActive = true;

            var allSongs = Ctr_Song.Current.GetSongs(false);

            listOfSongs = new List<Song>();
            List<Artist> listOfArtists = new List<Artist>();
            List<Album> listOfAlbums = new List<Album>();

            Song songAUX;
            Artist artistAUX;
            Album albumAUX;
            string title = string.Empty;
            string artist = string.Empty;
            string album = string.Empty;
            string genre = string.Empty;

            foreach (Song song in allSongs)
            {
                title = song.Name;
                artist = song.Artist;
                album = song.Album;
                genre = song.Genre;

                if (title.ToLower().Contains(searchTerm.ToLower()) ||
                    artist.ToLower().Contains(searchTerm.ToLower()) ||
                    album.ToLower().Contains(searchTerm.ToLower()) ||
                    genre.ToLower().Contains(searchTerm.ToLower()))
                {

                    listOfSongs.Add(song);

                    if (artist.ToLower().Contains(searchTerm.ToLower()))
                    {
                        artistAUX = new Artist();
                        if (listOfArtists.Exists(a => a.Name == artist) == false)
                        {
                            artistAUX.Name = artist;
                            listOfArtists.Add(artistAUX);
                        }

                        albumAUX = new Album();
                        if (listOfAlbums.Exists(a => a.ID == song.AlbumID) == false)
                        {
                            albumAUX.Name = album;
                            albumAUX.Artist = artist;
                            albumAUX.ID = song.AlbumID;
                            albumAUX.Year = Convert.ToInt32(song.Year);
                            albumAUX.Genre = song.Genre;
                            albumAUX.HexColor = song.HexColor;

                            listOfAlbums.Add(albumAUX);
                        }
                    }
                    else if (album.ToLower().Contains(searchTerm.ToLower()))
                    {
                        albumAUX = new Album();
                        if (listOfAlbums.Exists(a => a.ID == song.AlbumID) == false)
                        {
                            albumAUX.Name = album;
                            albumAUX.Artist = artist;
                            albumAUX.ID = song.AlbumID;
                            albumAUX.Year = Convert.ToInt32(song.Year);
                            albumAUX.Genre = song.Genre;
                            albumAUX.HexColor = song.HexColor;

                            listOfAlbums.Add(albumAUX);
                        }
                    }
                }
            }

            anyResult = false;

            if (listOfArtists.Count == 0)
            {
                artists.Visibility = Visibility.Collapsed;
            }
            else
            {
                anyResult = true;
                artists.Visibility = Visibility.Visible;
            }

            if (listOfAlbums.Count == 0)
            {
                albums.Visibility = Visibility.Collapsed;
            }
            else
            {
                anyResult = true;
                albums.Visibility = Visibility.Visible;
            }

            songsList.ItemsSource = listOfSongs.OrderBy(s => s.Name).ToList();
            albumsList.ItemsSource = listOfAlbums.OrderBy(s => s.Name).ToList();
            artistsList.ItemsSource = listOfArtists.OrderBy(s => s.Name).ToList();

            if (listOfSongs.Count == 0)
            {
                ResetSearch();

                NoResults.Visibility = Visibility.Visible;
            }
            else
            {
                anyResult = true;
                NoResults.Visibility = Visibility.Collapsed;
                songs.Visibility = Visibility.Visible;
            }

            //progress.IsActive = false;

        }

        private void ResetSearch()
        {
            anyResult = false;
            artists.Visibility = Visibility.Collapsed;
            albums.Visibility = Visibility.Collapsed;
            songs.Visibility = Visibility.Collapsed;

            artistsList.ItemsSource = null;
            albumsList.ItemsSource = null;
            songsList.ItemsSource = null;
            NoResults.Visibility = Visibility.Collapsed;
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResetKeyPressTimer();

            if (searchBox.Text.Length == 0)
            {
                ResetSearch();
            }
        }

        private void ResetKeyPressTimer()
        {
            if (timer != null)
                timer.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void ResultsScroll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (anyResult)
                e.Handled = false;
            else
                SearchMode = SearchPaneMode.Closed;

            UIDismissed?.Invoke(this);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Visibility = Visibility.Visible;
            searchBox.Focus(FocusState.Keyboard);
            //OpenSearchPane();
        }

        private void SongsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Song clickedSong = e.ClickedItem as Song;

            PlayerController.Play(clickedSong);
        }

        private void Root_LostFocus(object sender, RoutedEventArgs e)
        {
            //SearchMode = SearchPaneMode.Closed;
            //UIDismissed?.Invoke(this);
        }
    }
}
