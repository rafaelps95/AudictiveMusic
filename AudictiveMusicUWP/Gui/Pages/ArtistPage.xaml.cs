using AudictiveMusicUWP.Gui.Pages.LFM;
using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ArtistPage : Page
    {
        private ResourceLoader res;
        private int RepeatButtonCycleCounter;
        private bool ClickEventIsTouch;
        private Thickness TouchFlyoutMargin;



        public string Bio
        {
            get { return (string)GetValue(BioProperty); }
            set { SetValue(BioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BioProperty =
            DependencyProperty.Register("Bio", typeof(string), typeof(ArtistPage), new PropertyMetadata(string.Empty));



        private bool _bioInHeader = true;

        public double AlbumItemLength
        {
            get
            {
                //return itemLenght;
                return (double)GetValue(AlbumItemLengthProperty);
            }
            set
            {
                //itemLenght = value;
                SetValue(AlbumItemLengthProperty, value);
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumItemLenght"));
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlbumItemLengthProperty =
            DependencyProperty.Register("AlbumItemLength", typeof(double), typeof(ArtistPage), new PropertyMetadata(150));

        private List<Song> listOfSongs
        {
            get;
            set;
        }

        private List<Album> listOfAlbums
        {
            get;
            set;
        }

        private NavigationMode NavMode
        {
            get;
            set;
        }

        private Artist Artist
        {
            get;
            set;
        }

        private double _selectionBarOffset;
        private bool _selectionBarOnTop = false;

        public ArtistPage()
        {
            this.SizeChanged += ArtistPage_SizeChanged;
            this.Loaded += ArtistPage_Loaded;
            this.InitializeComponent();

            res = new ResourceLoader();
            RepeatButtonCycleCounter = 0;
            ClickEventIsTouch = false;
            TouchFlyoutMargin = new Thickness();
            //this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void ArtistPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateView(scroll.ActualWidth);
        }

        private void ArtistPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            albumsList.Width = e.NewSize.Width;
            selectionItemsBar.Width = e.NewSize.Width;
            _selectionBarOffset = header.ActualHeight + albumsHeader.ActualHeight + albumsScroll.ActualHeight + 30;

            UpdateView(e.NewSize.Width);
        }

        private void UpdateView(double width)
        {
            if (width < 500)
            {
                circleImage.Height = circleImage.Width = 75;
                artistName.FontSize = 18;
                headerBorder.Margin = new Thickness(10);

                if (string.IsNullOrWhiteSpace(this.Bio))
                {
                    bioHeaderGrid.Visibility = bioGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bioHeaderGrid.Visibility = Visibility.Collapsed;
                    bioGrid.Visibility = Visibility.Visible;
                }
            }
            else if (width >= 500 && width < 600)
            {
                circleImage.Height = circleImage.Width = 100;
                artistName.FontSize = 24;
                headerBorder.Margin = new Thickness(15);

                if (string.IsNullOrWhiteSpace(this.Bio))
                {
                    bioHeaderGrid.Visibility = bioGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bioHeaderGrid.Visibility = Visibility.Collapsed;
                    bioGrid.Visibility = Visibility.Visible;
                }
            }
            else if (width >= 600 && width < 800)
            {
                circleImage.Height = circleImage.Width = 150;
                artistName.FontSize = 28;
                headerBorder.Margin = new Thickness(20);
                bioHeaderGrid.MaxWidth = 300;

                if (string.IsNullOrWhiteSpace(this.Bio))
                {
                    bioHeaderGrid.Visibility = bioGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bioHeaderGrid.Visibility = Visibility.Collapsed;
                    bioGrid.Visibility = Visibility.Visible;
                }
            }
            else if (width >= 800 && width < 950)
            {
                circleImage.Height = circleImage.Width = 150;
                artistName.FontSize = 28;
                headerBorder.Margin = new Thickness(20);
                bioHeaderGrid.MaxWidth = 300;

                if (string.IsNullOrWhiteSpace(this.Bio))
                {
                    bioHeaderGrid.Visibility = bioGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bioHeaderGrid.Visibility = Visibility.Visible;
                    bioGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                circleImage.Height = circleImage.Width = 150;
                artistName.FontSize = 28;
                headerBorder.Margin = new Thickness(20);
                bioHeaderGrid.MaxWidth = 500;

                if (string.IsNullOrWhiteSpace(this.Bio))
                {
                    bioHeaderGrid.Visibility = bioGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bioHeaderGrid.Visibility = Visibility.Visible;
                    bioGrid.Visibility = Visibility.Collapsed;
                }
            }

            header.Width = width;

            try
            {
                bioGrid.Width = width - 10;
            }
            catch
            {

            }
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;
            Artist = e.Parameter as Artist;
            //header.SetContext(Artist);
            artistName.Text = Artist.Name;
            UpdateView(scroll.ActualWidth);
            BitmapImage bmp = new BitmapImage();
            headerBackground.ImageSource = bmp;
            bmp.ImageOpened += HeaderBackground_ImageOpened;
            circleImage.Source = bmp;
            bmp.UriSource = Artist.ImageUri;

            //var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("OpenArtistConnectedAnimation");
            //if (anim != null)
            //{
            //    anim.TryStart(header);
            //}

            await LoadMusic();

            _selectionBarOffset = header.ActualHeight + albumsHeader.ActualHeight + albumsScroll.ActualHeight + 30;

            Bio = await LastFm.GetArtistBiography(Artist.Name);

            if (string.IsNullOrWhiteSpace(Bio) == false)
            {
                if (scrollContent.ActualWidth >= 800)
                {
                    bioHeaderGrid.Visibility = Visibility.Visible;
                    bioGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bioHeaderGrid.Visibility = Visibility.Collapsed;
                    bioGrid.Visibility = Visibility.Visible;
                }
                Animation.BeginBasicFadeInAnimation(bioHeaderGrid);
            }

            //StorageFile imgFile = null;

            //try
            //{
            //    imgFile = await StorageFile.GetFileFromApplicationUriAsync(Artist.ImageUri);
            //}
            //catch
            //{

            //}

            //try
            //{
            //    if (imgFile != null)
            //    {
            //        using (var stream = await imgFile.OpenAsync(FileAccessMode.Read))
            //        {
            //            //color = await ImageHelper.GetDominantColor(stream);
            //            BitmapImage b = new BitmapImage();
            //            headerBackground.ImageSource = b;
            //            b.ImageOpened += HeaderBackground_ImageOpened;
            //            b.SetSource(stream);
            //        }

            //        circleImage.SetSource(Artist.ImageUri);
            //    }
            //    else
            //    {
            //        await Spotify.DownloadArtistImage(Artist);
            //    }
            //}
            //catch
            //{

            //}
        }

        private async Task LoadMusic()
        {
            //Task<List<string>> result = Task.Factory.StartNew(() => ExpensiveMethod());
            OpenPage(NavMode == NavigationMode.Back);

            await Task.Run(() =>
            {
                listOfSongs = Ctr_Song.Current.GetSongsByArtist(Artist);
                listOfAlbums = new List<Album>();
                Album albumAUX = null;
                foreach (Song s in listOfSongs)
                {
                    if (listOfAlbums.Exists(a => a.ID == s.AlbumID) == false)
                    {
                        albumAUX = Ctr_Album.Current.GetAlbum(new Album() { ID = s.AlbumID, HexColor = s.HexColor });
                        listOfAlbums.Add(albumAUX);
                    }
                }

                listOfAlbums = listOfAlbums.OrderBy(a => a.Year).ToList();
                listOfAlbums.Reverse();
            });

            albumsList.ItemsSource = listOfAlbums;

            //if (listOfAlbums.Count > 1)
            //    // BUSCA A STRING PLURAL JÁ QUE HÁ MAIS DE UM ÁLBUM NA LISTA
            //    countOfAlbums.Text = listOfAlbums.Count + " " + ApplicationInfo.Current.Resources.GetString("AlbumPlural").ToLower();
            //else
            //    // BUSCA A STRING SINGULAR JÁ QUE HÁ APENAS UM ÁLBUM NA LISTA
            //    countOfAlbums.Text = listOfAlbums.Count + " " + ApplicationInfo.Current.Resources.GetString("AlbumSingular").ToLower();

            await Task.Delay(10);

            listView.ItemsSource = listOfSongs.OrderBy(s => s.Album).ToList();

            //if (listOfSongs.Count > 1)
            //    // BUSCA A STRING PLURAL JÁ QUE HÁ MAIS DE UMA MÚSICA NA LISTA
            //    countOfSongs.Text = listOfSongs.Count + " " + ApplicationInfo.Current.Resources.GetString("SongPlural").ToLower();
            //else
            //    // BUSCA A STRING SINGULAR JÁ QUE HÁ APENAS UMA MÚSICA NA LISTA
            //    countOfSongs.Text = listOfSongs.Count + " " + ApplicationInfo.Current.Resources.GetString("SongSingular").ToLower();

            //header.UpdateNumberOfItems(listOfSongs.Count, listOfAlbums.Count);

            //LoadBio();

            if (listOfAlbums.Count > 1)
                // BUSCA A STRING PLURAL JÁ QUE HÁ MAIS DE UM ÁLBUM NA LISTA
                subtitle1.Text = listOfAlbums.Count + " " + ApplicationInfo.Current.Resources.GetString("AlbumPlural").ToLower();
            else
                // BUSCA A STRING SINGULAR JÁ QUE HÁ APENAS UM ÁLBUM NA LISTA
                subtitle1.Text = listOfAlbums.Count + " " + ApplicationInfo.Current.Resources.GetString("AlbumSingular").ToLower();

            if (listOfSongs.Count > 1)
                // BUSCA A STRING PLURAL JÁ QUE HÁ MAIS DE UMA MÚSICA NA LISTA
                subtitle2.Text = listOfSongs.Count + " " + ApplicationInfo.Current.Resources.GetString("SongPlural").ToLower();
            else
                // BUSCA A STRING SINGULAR JÁ QUE HÁ APENAS UMA MÚSICA NA LISTA
                subtitle2.Text = listOfSongs.Count + " " + ApplicationInfo.Current.Resources.GetString("SongSingular").ToLower();

            subtitleSeparator.Visibility = subtitle2.Visibility = Visibility.Visible;
        }

        private void OpenPage(bool reload)
        {
            //Storyboard sb = this.Resources["OpenPageTransition"] as Storyboard;

            //if (reload)
            //{
            //    layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;
            //}

            //sb.Begin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            scroll.ChangeView(0, 0, 1, true);
            pageBackground.Opacity = 0;
            artistBackgroundBitmap.UriSource = null;
            //layoutRootScale.ScaleX = layoutRootScale.ScaleY = 0.9;
            //layoutRoot.Opacity = 0;

            //header.ClearContext();
        }

        private async void LoadBio()
        {
            //StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artists", CreationCollisionOption.OpenIfExists);
            //StorageFile bioFile = await folder.CreateFileAsync(StringHelper.RemoveSpecialChar(Artist.Name) + "_bio.txt", CreationCollisionOption.OpenIfExists);

            //string b = await FileIO.ReadTextAsync(bioFile);

            //if (string.IsNullOrWhiteSpace(b) == false)
            //{
            //    bio.Text = b;
            //    bioGrid.Visibility = Visibility.Visible;

            //    if (this.ActualWidth >= 700)
            //    {
            //        songsList.Padding = new Thickness(0, 0, 0, 0);
            //        bioGrid.Margin = new Thickness(12, 12, 12, 72);
            //    }
            //    else
            //    {
            //        songsList.Padding = new Thickness(0, 0, 0, 60);
            //        bioGrid.Margin = new Thickness(20, 20, 20, 0);
            //    }
            //}
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
                
            }
        }

        private void albumItemOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                
            }
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        private void artistsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(this, typeof(ArtistPage), e.ClickedItem);
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

        private void artistBackgroundBitmap_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0.4, 1200, pageBackground, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut), false, 200);
            animation.Begin();
        }

        private void playArtist_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.Play(this.Artist);
        }

        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddToNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.AddToQueue(this.Artist);
        }

        private async void shareArtist_Click(object sender, RoutedEventArgs e)
        {
            if (await ShareHelper.Instance.Share(Artist) == false)
            {
                string message = ApplicationInfo.Current.Resources.GetString("ShareErrorMessage").Replace("%", "0x003");

                MessageDialog md = new MessageDialog(message);
                await md.ShowAsync();
            }
        }

        private void albumsList_Loaded(object sender, RoutedEventArgs e)
        {
            //var sv = (ScrollViewer)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.albumsList, 0), 0);
            //sv.ViewChanged += Sv_ViewChanged;
        }

        private void Sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //AlbumItemHelper.PointerIsInContact = false;
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
            PopupHelper.GetInstance(sender).ShowPopupMenu(Artist, true, new Point(0, 0));
        }

        private void songsList_ItemClick(object sender, ItemClickEventArgs e)
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


        private void scroll_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
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

        private void scroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            AlbumItemHelper.PointerIsInContact = false;

            if (e.IsIntermediate == false)
            {
                if (scroll.VerticalOffset > 100)
                {
                    //ReduceHeader();
                }
                else
                {
                    //EnlargeHeader();
                }
            }
        }

        private void AlbumItem_MenuTriggered(object sender, Point point)
        {
            Album album = (sender as FrameworkElement).DataContext as Album;

            CreateAlbumPopup(album, sender, point);
        }




        private void EnableSelectionMode()
        {
            listView.SelectionMode = ListViewSelectionMode.Multiple;
            listView.SelectionChanged += listView_SelectionChanged;
            listView.Padding = new Thickness(0, 25, 0, 110);
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

        private void HeaderBackground_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (ThemeSettings.IsTransparencyEnabled == false)
                headerBackgroundGrid.Opacity = 0.4;
            else if (ThemeSettings.IsPerformanceModeEnabled)
                headerBackgroundGrid.Opacity = 0.4;

            Animation.BeginBasicFadeInAnimation(headerBackground).Completed += async (s, a) =>
            {
                if (ThemeSettings.IsTransparencyEnabled)
                {
                    if (ThemeSettings.IsPerformanceModeEnabled == false)
                    {
                        await Task.Delay(50);
                        acrylic.Opacity = 0;
                        acrylic.AcrylicNoise = acrylic.AcrylicEnabled = true;
                        acrylic.AcrylicOpacity = 1;
                        acrylic.AcrylicBackgroundSource = RPSToolkit.AcrylicType.Backdrop;

                        Animation.BeginBasicFadeInAnimation(acrylic);
                    }
                }
            };
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.Play(this.Artist);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            var songs = Ctr_Song.Current.GetSongsByArtist(Artist);

            foreach (Song song in songs)
                list.Add(song.SongURI);

            PlaylistHelper.RequestPlaylistPicker(this, list);
        }

        private void moreButton_Click(object sender, RoutedEventArgs e)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(Artist, true, new Point(0, 0));
        }

        private void CircleImage_ImageOpened(object sender, RoutedEventArgs e)
        {

        }

        private void CircleImage_ImageFailed(object sender, RoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("OpenArtistConnectedAnimation");
            if (anim != null)
            {
                circleImage.Opacity = 1;
                anim.TryStart(circleImage);
            }
        }

        private async void webServiceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var result = await LastFm.Current.Client.Artist.GetInfoAsync(this.Artist.Name, ApplicationInfo.Current.Language, true);
            if (result.Success)
            {
                LastArtist artist = result.Content;

                NavigationService.Navigate(this, typeof(LastFmProfilePage), artist);
            }
        }

        private void BioTB_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OpenBioFlyout(sender, FlyoutPlacementMode.Left);
        }

        private void OpenBioFlyout(object sender, FlyoutPlacementMode flyoutPlacementMode)
        {
            bioFlyoutTB.Text = bioFlyoutTB2.Text = this.Bio;

            if (ApplicationInfo.Current.IsMobile)
                ((FrameworkElement)sender).ContextFlyout.Placement = FlyoutPlacementMode.Full;
            else
                ((FrameworkElement)sender).ContextFlyout.Placement = flyoutPlacementMode;
            ((FrameworkElement)sender).ContextFlyout.ShowAt((FrameworkElement)sender);
        }

        private async void LastfmButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await LastFm.Current.Client.Artist.GetInfoAsync(Artist.Name, ApplicationInfo.Current.Language, true);
            if (result.Success)
            {
                LastArtist artist = result.Content;

                NavigationService.Navigate(this, typeof(LastFmProfilePage), artist);
            }
        }

        private void ReadMore_Click(object sender, RoutedEventArgs e)
        {
            OpenBioFlyout(bioTB2, FlyoutPlacementMode.Top);
        }
    }
}
