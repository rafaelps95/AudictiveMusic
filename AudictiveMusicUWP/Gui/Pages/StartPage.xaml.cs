using AudictiveMusicUWP.Gui.Pages.LFM;
using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Dao;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using NotificationsVisualizerLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        private ObservableCollection<Song> favorites;
        private enum PageLayout
        {
            ThreeColumns = 960,
            TwoColumns = 640,
            SingleColumn = 320,
            Unknown = 0
        }

        private PageLayout pageLayout = PageLayout.Unknown;

        private NavigationMode NavMode
        {
            get;
            set;
        }

        public StartPage()
        {
            LastFm.Current.Connected += LastFm_Connected;
            LastFm.Current.Disconnected += LastFm_Disconnected;
            SongDao.FavoritesChanged += Collection_FavoritesChanged;
            this.SizeChanged += StartPage_SizeChanged;
            this.Loaded += StartPage_Loaded;
            this.InitializeComponent();

            favorites = new ObservableCollection<Song>();
            favorites.CollectionChanged += Favorites_CollectionChanged;

            SetWebServiceButton(ApplicationInfo.Current.Resources.GetString("SignInLastFm"), WebServiceButton.WebService.LastFm);
        }

        private void SetWebServiceButton(string text, WebServiceButton.WebService service)
        {
            lastFmButton.Text = text;
            lastFmButton.Service = service;

            //if (searchUI.GetSearchBarPlacement() == NewSearchUX.SearchPlacement.Right)
            //    searchUI.SetOffset(headerRightButtons.ActualWidth);
        }

        private void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            //searchUI.SetSearchBarPlacement(NewSearchUX.SearchPlacement.Right);
            //searchUI.SetOffset(headerRightButtons.ActualWidth);

        }

        private void Favorites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateScrollButtons();
        }

        private void StartPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePageLayout(e.NewSize);
        }

        private void UpdatePageLayout(Size newSize)
        {
            PageHelper.MainPage.SearchUISetOffset(headerRightButtons.ActualWidth);

            if (newSize.Width < headerRightButtons.ActualWidth + PageHelper.MainPage.SearchUIGetSearchBoxWidth())
            {
                //searchUI.SetSearchBarPlacement(NewSearchUX.SearchBoxPlacement.Center);
                //searchUI.SetOffset(0);
                PageHelper.MainPage.SearchUISetCompactMode(true);
            }
            else
            {
                PageHelper.MainPage.SearchUISetCompactMode(false);
            }

            //searchUI.SetOffset(headerRightButtons.ActualWidth);

            if (newSize.Height < 700)
            {
                layoutGrid.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                layoutGrid.VerticalAlignment = VerticalAlignment.Center;
            }

            if (newSize.Width < (int)PageLayout.TwoColumns)
            {
                //FavoritesList.Width = newSize.Width;
                firstColumn.Width = new GridLength(newSize.Width, GridUnitType.Pixel);

                if (pageLayout != PageLayout.SingleColumn)
                {
                    secondColumn.Width = thirdColumn.Width = new GridLength(0, GridUnitType.Pixel);

                    layoutGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Grid.SetColumn(suggestionGrid, 0);
                    Grid.SetColumn(favoritesGrid, 0);
                    Grid.SetColumn(scrobblesGrid, 0);
                    Grid.SetColumnSpan(suggestionGrid, 1);
                    Grid.SetColumnSpan(favoritesGrid, 1);
                    Grid.SetColumnSpan(scrobblesGrid, 1);
                    Grid.SetRow(suggestionGrid, 1);
                    Grid.SetRow(favoritesGrid, 2);
                    Grid.SetRow(scrobblesGrid, 3);

                    pageLayout = PageLayout.SingleColumn;
                }

            }
            else if (newSize.Width >= (int)PageLayout.TwoColumns && newSize.Width < (int)PageLayout.ThreeColumns)
            {
                //favoritesScroll.Width = 320;
                layoutGrid.HorizontalAlignment = HorizontalAlignment.Center;
                firstColumn.Width = new GridLength(320);
                secondColumn.Width = new GridLength(320);
                thirdColumn.Width = new GridLength(0, GridUnitType.Pixel);

                if (pageLayout != PageLayout.TwoColumns)
                {
                    Grid.SetColumn(suggestionGrid, 0);
                    Grid.SetColumn(favoritesGrid, 1);
                    Grid.SetColumn(scrobblesGrid, 0);
                    Grid.SetColumnSpan(suggestionGrid, 1);
                    Grid.SetColumnSpan(favoritesGrid, 1);
                    Grid.SetColumnSpan(scrobblesGrid, 2);
                    Grid.SetRow(suggestionGrid, 1);
                    Grid.SetRow(favoritesGrid, 1);
                    Grid.SetRow(scrobblesGrid, 2);

                    pageLayout = PageLayout.TwoColumns;
                }
            }
            else
            {
                //favoritesScroll.Width = 320;

                if (pageLayout != PageLayout.ThreeColumns)
                {
                    layoutGrid.HorizontalAlignment = HorizontalAlignment.Center;
                    firstColumn.Width = secondColumn.Width = thirdColumn.Width = new GridLength(320, GridUnitType.Pixel);


                    Grid.SetColumn(suggestionGrid, 0);
                    Grid.SetColumn(favoritesGrid, 1);
                    Grid.SetColumn(scrobblesGrid, 2);
                    Grid.SetColumnSpan(suggestionGrid, 1);
                    Grid.SetColumnSpan(favoritesGrid, 1);
                    Grid.SetColumnSpan(scrobblesGrid, 1);
                    Grid.SetRow(suggestionGrid, 1);
                    Grid.SetRow(favoritesGrid, 1);
                    Grid.SetRow(scrobblesGrid, 1);

                    pageLayout = PageLayout.ThreeColumns;
                }
            }

            UpdateScrollButtons();
        }

        private void LastFm_Connected(LastUser loggedUser)
        {
            LoggedIn(loggedUser);
        }

        private void LoggedIn(LastUser loggedUser)
        {
            LoadUserInfo();
        }

        private void LastFm_Disconnected()
        {
            LoggedOut();
        }

        private void LoggedOut()
        {
            //lastFmUserImage.RemoveSource();
            lastFmButton.RemoveImage();
            SetWebServiceButton(ApplicationInfo.Current.Resources.GetString("SignInLastFm"), WebServiceButton.WebService.LastFm);
            recentScroblles.ItemsSource = null;
            recentScroblles.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            UpdatePageLayout(new Size(this.ActualWidth, this.ActualHeight));

            OpenPage(NavMode == NavigationMode.Back);
            //SetTile();
        }

        private void LoadUserInfo()
        {
            if (!string.IsNullOrWhiteSpace(ApplicationSettings.LastFmSessionUserImageUri))
                //lastFmUserImage.SetSource(new Uri(ApplicationSettings.LastFmSessionUserImageUri, UriKind.Absolute), CircleImage.ImageType.LastFmUser);
                lastFmButton.SetImageSource(new Uri(ApplicationSettings.LastFmSessionUserImageUri, UriKind.Absolute));

            SetWebServiceButton(ApplicationInfo.Current.Resources.GetString("WelcomeUser").Replace("#", ApplicationSettings.LastFmSessionUsername), WebServiceButton.WebService.LoggedUser);
            LoadRecentScrobbles();
        }

        private async void LoadRecentScrobbles()
        {
            if (!ApplicationInfo.Current.HasInternetConnection)
                return;

            //progress.IsActive = true;
            DateTimeOffset dateTime = DateTimeOffset.Now.ToUniversalTime();
            dateTime = dateTime.AddDays(-7);

            var result = await LastFm.Current.Client.User.GetRecentScrobbles(ApplicationSettings.LastFmSessionUsername, dateTime, 1, 5);
            if (result.Success)
            {
                var content = result.Content.ToList();
                if (content.Count > 0)
                {
                    //if (content[0].IsNowPlaying == true)
                    //    content.RemoveAt(0);

                    recentScroblles.ItemsSource = content;
                    recentScroblles.Visibility = Visibility.Visible;
                }
            }

            //progress.IsActive = false;
        }

        private void LoadCards()
        {
            LoadRecommendation();
            LoadFavorites();

            if (LastFm.Current.IsAuthenticated && ApplicationInfo.Current.HasInternetConnection)
                LoadGlobalTopArtists();
        }

        private async void LoadFavorites()
        {
            favorites.Clear();

            List<Song> songs = Ctr_Song.Current.GetFavoriteSongs();

            if (songs.Count == 0)
            {
                noFavoritesGrid.Visibility = Visibility.Visible;
                favoritesScroll.Visibility = Visibility.Collapsed;
                openFavoritesPageButton.Visibility = Visibility.Collapsed;
                return;
            }

            noFavoritesGrid.Visibility = Visibility.Collapsed;
            favoritesScroll.Visibility = Visibility.Visible;

            await songs.Shuffle();

            var smallList = songs.Take(5).ToList();

            foreach (Song s in smallList)
                favorites.Add(s);

            FavoritesList.ItemsSource = favorites;

            if (songs.Count > 5)
            {
                int remainingItems = songs.Count - 5;
                openFavoritesPageButton.Visibility = Visibility.Visible;
                openFavoritesPageButton.Content = ApplicationInfo.Current.Resources.GetString("FavoritesSeeOther").Replace("#", remainingItems.ToString());
            }
            else
            {
                openFavoritesPageButton.Visibility = Visibility.Collapsed;
            }

            UpdateScrollButtons();
        }

        private void Collection_FavoritesChanged()
        {
            LoadFavorites();
        }

        private void LoadRecommendation()
        {
            Song song = Ctr_Song.Current.GetRandomSong();

            if (song == null)
            {
                card1.Visibility = Visibility.Collapsed;
                return;
            }

            card1.SetContext(song);
            card1.Visibility = Visibility.Visible;
        }

        private void LoadGlobalTopArtists()
        {
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
        }
        private void OpenPage(bool reload)
        {
            try
            {
                Storyboard sb = this.Resources["OpenPageTransition"] as Storyboard;

                if (reload)
                {
                    layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;
                }

                sb.Begin();
            }
            catch
            {

            }
        }

        private void Tile_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                Song song = ((PreviewTile)sender).Tag as Song;

                this.ShowPopupMenu(song, sender, Enumerators.MediaItemType.Song, true, e.GetPosition((PreviewTile)sender));
            }
        }

        private void Tile_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                Song song = ((PreviewTile)sender).Tag as Song;

                this.ShowPopupMenu(song, sender, Enumerators.MediaItemType.Song, true, e.GetPosition((PreviewTile)sender));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //tile.TileSize = TileSize.Small;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //tile.TileSize = TileSize.Medium;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //tile.TileSize = TileSize.Wide;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //tile.TileSize = TileSize.Large;
        }

        private void Tile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SetPlaylistMessage(new List<string>() { (((PreviewTile)sender).Tag as Song).SongURI }));
            //UpdateSingleTile(tilesContainer.Children.IndexOf((UIElement)sender));
        }

        private void PreviewTile_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //tilesContainer.ItemWidth = tilesContainer.ItemHeight = e.NewSize.Width + 4;
        }

        private void playButton_Tapped(object sender, TappedRoutedEventArgs e)
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

            PageHelper.MainPage.OpenPlayer();
        }

        private void pageTransition_Completed(object sender, object e)
        {
            LoadCards();

            if (LastFm.Current.IsAuthenticated && ApplicationInfo.Current.HasInternetConnection)
                LoadUserInfo();
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.PlayEverything));

            PageHelper.MainPage.OpenPlayer();
        }

        private void collectionButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.Navigate(typeof(CollectionPage), "page=artists");
        }

        private void favoritesButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.Navigate(typeof(Favorites), null);
        }

        private void foldersButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.Navigate(typeof(FolderPage), null);
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.CreateSearchGrid();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.Navigate(typeof(Settings), "path=menu");
        }

        private void lastFmLoginButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.CreateLastFmLogin();
        }

        private async void lastFmProfileButton_Click(object sender, RoutedEventArgs e)
        {
            LastUser user = await LastFm.Current.GetUserInfo(ApplicationSettings.LastFmSessionUsername);

            PageHelper.MainPage.Navigate(typeof(LastFmProfilePage), user);
        }

        private async void lastFmUserButton_Click(object sender, RoutedEventArgs e)
        {
            LastUser user;
            //LastTrack track = new LastTrack();
            //track.Images.Medium
            if (ApplicationInfo.Current.HasInternetConnection)
            {
                if (LastFm.Current.Client.Auth.Authenticated)
                    user = await LastFm.Current.GetUserInfo(ApplicationSettings.LastFmSessionUsername);
                else
                    user = new LastUser();

                ((Button)sender).ShowLastFmPopupMenu(user);
            }
            else
            {

            }
        }

        private void reloadScrobblesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRecentScrobbles();
        }

        private async void lastFmButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LastUser user;
            //LastTrack track = new LastTrack();
            //track.Images.Medium
            if (ApplicationInfo.Current.HasInternetConnection)
            {
                if (LastFm.Current.Client.Auth.Authenticated)
                    user = await LastFm.Current.GetUserInfo(ApplicationSettings.LastFmSessionUsername);
                else
                    user = new LastUser();

                ((FrameworkElement)sender).ShowLastFmPopupMenu(user);
            }
            else
            {

            }

        }

        private void FavoritesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Song clickedSong = e.ClickedItem as Song;

            List<string> list = new List<string>();
            list.Add(clickedSong.SongURI);

            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        private void FavoriteItem_MenuTriggered(object sender, Point point)
        {
            Song song = (sender as FrameworkElement).DataContext as Song;

            //if (
            CreateSongPopup(song, sender, point);
        }

        private void CreateSongPopup(Song song, object sender, Point point)
        {
            this.ShowPopupMenu(song, sender, Enumerators.MediaItemType.Song, true, point);

        }

        private void openFavoritesPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageHelper.MainPage.Navigate(typeof(Favorites));
        }

        private void leftBorder_Click(object sender, RoutedEventArgs e)
        {
            if (favoritesScroll.HorizontalOffset > 0)
            {
                favoritesScroll.ChangeView(favoritesScroll.HorizontalOffset - 100, 0, 1, false);
            }
        }

        private void rightBorder_Click(object sender, RoutedEventArgs e)
        {
            if (favoritesScroll.HorizontalOffset < FavoritesList.ActualWidth)
            {
                favoritesScroll.ChangeView(favoritesScroll.HorizontalOffset + 100, 0, 1, false);
            }
        }

        private void favoritesScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateScrollButtons();

        }

        private void UpdateScrollButtons()
        {
            //tempScrollValues.Text = string.Format("ViewportWidth: {0}\nCurrentOffset: {1}\nScrollableWidth: {2}\nGridActualWidth: {3}\nGridViewActualWidth: {4}\nExtentWidth: {5}", favoritesScroll.ViewportWidth, favoritesScroll.HorizontalOffset, favoritesScroll.ScrollableWidth, favoritesGrid.ActualWidth, FavoritesList.ActualWidth, favoritesScroll.ExtentWidth);
            if (favoritesScroll.ScrollableWidth == 0)
            {
                if (favoritesScroll.ExtentWidth == 0 && favorites.Count > 0)
                {
                    double singleItemActualWidth = 175;
                    double totalNeededSize = singleItemActualWidth * favorites.Count;

                    if (totalNeededSize > favoritesScroll.ViewportWidth)
                    {
                        rightBorder.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        leftBorder.Visibility = rightBorder.Visibility = Visibility.Collapsed;
                    }
                }
                else
                    leftBorder.Visibility = rightBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (favoritesScroll.HorizontalOffset == 0)
                {
                    leftBorder.Visibility = Visibility.Collapsed;
                }
                else if (favoritesScroll.HorizontalOffset == favoritesScroll.ScrollableWidth)
                {
                    rightBorder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    leftBorder.Visibility = rightBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void FavoritesList_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateScrollButtons();
        }

        private void HeaderRightButtons_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageHelper.MainPage.SearchUISetOffset(headerRightButtons.ActualWidth);

            if (this.ActualWidth < headerRightButtons.ActualWidth + PageHelper.MainPage.SearchUIGetSearchBoxWidth())
            {
                //searchUI.SetSearchBarPlacement(NewSearchUX.SearchBoxPlacement.Center);
                //searchUI.SetOffset(0);
                PageHelper.MainPage.SearchUISetCompactMode(true);
            }
            else
            {
                PageHelper.MainPage.SearchUISetCompactMode(false);
            }
        }
    }
}
