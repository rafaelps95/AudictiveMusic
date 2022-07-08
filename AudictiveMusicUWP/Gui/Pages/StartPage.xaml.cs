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
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
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

        private bool _shadowVisible = false;

        Compositor _compositor;
        SpriteVisual _sprite;
        CompositionEffectBrush _brush;

        public StartPage()
        {
            LastFm.Current.Connected += LastFm_Connected;
            LastFm.Current.Disconnected += LastFm_Disconnected;
            Ctr_Song.FavoritesChanged += Ctr_Song_FavoritesChanged;
            this.SizeChanged += StartPage_SizeChanged;
            this.Loaded += StartPage_Loaded;
            this.InitializeComponent();

            favorites = new ObservableCollection<Song>();
            favorites.CollectionChanged += Favorites_CollectionChanged;

            SetWebServiceButton(ApplicationInfo.Current.Resources.GetString("SignInLastFm"), WebServiceButton.WebService.LastFm);
        }

        private void Ctr_Song_FavoritesChanged(Song updatedSong)
        {
            LoadFavorites();
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
            //ApplyElementShadow(headerShadow);
        }

        private void ApplyElementShadow(Panel panel)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _sprite = _compositor.CreateSpriteVisual();
            //_sprite.Brush = _compositor.CreateColorBrush(Colors.Blue);
            _sprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);

            var basicShadow = _compositor.CreateDropShadow();
            basicShadow.BlurRadius = 25f;
            basicShadow.Offset = new Vector3(0, 5, 3);

            _sprite.Shadow = basicShadow;

            ElementCompositionPreview.SetElementChildVisual(panel, _sprite);
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
            PageHelper.SetSearchBarOffset(headerRightButtons.ActualWidth);

            double d = (headerRightButtons.ActualWidth + PageHelper.SearchBoxSize.Width + 13);
            PageHelper.SetSearchBarCompactMode(newSize.Width < d);
            //if (newSize.Width < headerRightButtons.ActualWidth + PageHelper.SearchBoxSize.Width + 13)
            //{
            //    PageHelper.SetSearchBarCompactMode(true);
            //}
            //else
            //{
            //    PageHelper.SetSearchBarCompactMode(false);
            //}

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

            var result = await LastFm.Current.Client.User.GetRecentScrobbles(ApplicationSettings.LastFmSessionUsername, dateTime, null, false, 1, 5);
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

        private void LoadFavorites()
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

            songs.Shuffle();

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
        private async void OpenPage(bool reload)
        {
            await Task.Delay(200);
            LoadCards();

            if (LastFm.Current.IsAuthenticated && ApplicationInfo.Current.HasInternetConnection)
                LoadUserInfo();
        }

        private void pageTransition_Completed(object sender, object e)
        {

        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.ShuffleCollection();
            PlayerController.OpenPlayer();
        }

        private void collectionButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(CollectionPage), "page=artists");
        }

        private void favoritesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(Favorites));
        }

        private void foldersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(FolderPage));
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(Settings), "path=menu");
        }

        private void lastFmLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LastFm.Current.RequestLogin(this);
        }

        private async void lastFmProfileButton_Click(object sender, RoutedEventArgs e)
        {
            LastUser user = await LastFm.Current.GetUserInfo(ApplicationSettings.LastFmSessionUsername);

            NavigationService.Navigate(this, typeof(LastFmProfilePage), user);
        }

        private void reloadScrobblesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRecentScrobbles();
        }

        private void FavoritesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Song clickedSong = e.ClickedItem as Song;

            List<string> list = new List<string>();
            list.Add(clickedSong.SongURI);

            PlayerController.Play(clickedSong);
        }

        private void FavoriteItem_MenuTriggered(object sender, Point point)
        {
            Song song = (sender as FrameworkElement).DataContext as Song;

            //if (
            CreateSongPopup(song, sender, point);
        }

        private void CreateSongPopup(Song song, object sender, Point point)
        {
            if (point == null)
                PopupHelper.GetInstance(sender).ShowPopupMenu(song);
            else
                PopupHelper.GetInstance(sender).ShowPopupMenu(song, true, point);

        }

        private void openFavoritesPageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(Favorites));
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
            PageHelper.SetSearchBarOffset(headerRightButtons.ActualWidth);
            PageHelper.SetSearchBarCompactMode(this.ActualWidth < headerRightButtons.ActualWidth + PageHelper.SearchBoxSize.Width + 13);
        }

        private void HeaderShadow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_sprite != null)
                _sprite.Size = e.NewSize.ToVector2();
        }

        private void ScrobbleSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(Settings), "path=scrobble");
        }

        private void PendingScrobblesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(PendingScrobbles));
        }

        private void RecentsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(RecentlyAdded));
        }

        private void ReloadSuggestionButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRecommendation();
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset > 0)
            {
                if (!_shadowVisible)
                {
                    _shadowVisible = true;
                    Animation.BeginBasicFadeInAnimation(fakeShadow, 300, 0.5);
                }
            }
            else
            {
                if (_shadowVisible)
                {
                    _shadowVisible = false;
                    Animation.BeginBasicFadeOutAnimation(fakeShadow);
                }
            }
        }

        private void LastFmButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationInfo.Current.HasInternetConnection)
            {
                if (LastFm.Current.Client.Auth.Authenticated)
                    PopupHelper.GetInstance(sender).ShowLastFmMenu(ApplicationSettings.LastFmSessionUsername);
            }
            else
            {

            }

        }

        private void LastFmButton_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lastFmButton.FocusState == FocusState.Keyboard)
                PageHelper.DismissSearchUI();
        }
    }
}
