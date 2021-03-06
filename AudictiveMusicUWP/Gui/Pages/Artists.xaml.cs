using AudictiveMusicUWP.Gui.UC;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using InAppNotificationLibrary;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
using Windows.Foundation;
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
    public sealed partial class Artists : Page
    {
        public double GridItemSize
        {
            get
            {
                //return itemLenght;
                return (double)GetValue(GridItemSizeProperty);
            }
            set
            {
                //itemLenght = value;
                SetValue(GridItemSizeProperty, value);
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumItemLenght"));
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridItemSizeProperty =
            DependencyProperty.Register("GridItemSize", typeof(double), typeof(Artists), new PropertyMetadata(100));

        //private ItemsWrapGrid artistsWrapGrid;
        private ResourceLoader res;
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
                    LoadArtists();
                }
            }
        }

        public Artists()
        {
            res = new ResourceLoader();
            CollectionHasBeenUpdated = false;
            this.SizeChanged += Artists_SizeChanged;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Artists_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 410)
            {
                this.GridItemSize = (e.NewSize.Width - 60) / 2;
            }
            else if (e.NewSize.Width >= 410 && e.NewSize.Width < 610)
            {
                this.GridItemSize = (e.NewSize.Width - 65) / 3;
            }
            else if (e.NewSize.Width >= 610 && e.NewSize.Width < 710)
            {
                this.GridItemSize = (e.NewSize.Width - 80) / 4;
            }
            else if (e.NewSize.Width >= 710 && e.NewSize.Width < 810)
            {
                this.GridItemSize = (e.NewSize.Width - 90) / 4;
            }
            else
            {
                this.GridItemSize = (e.NewSize.Width - 100) / 5;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            progress.IsActive = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            //if (ApplicationInfo.Current.IsMobile == false)
            //{
            //    listView.ItemContainerTransitions.Add(new EntranceThemeTransition() { FromVerticalOffset = 250, IsStaggeringEnabled = true });
            //}
            //else
            //{
            //    listView.ItemContainerTransitions.Add(new EntranceThemeTransition() { FromVerticalOffset = 250, IsStaggeringEnabled = false });
            //}

            if (((CollectionViewSource)Resources["ListOfArtists"]).Source == null || CollectionHasBeenUpdated)
            {
                CollectionHasBeenUpdated = false;
                LoadArtists();
            }
            else
            {
                OpenPage(NavMode == NavigationMode.Back);
            }
        }

        private void LoadArtists()
        {

            ((CollectionViewSource)Resources["ListOfArtists"]).Source = null;
            List<Artist> listOfArtists = Ctr_Artist.Current.GetArtists();

            listOfArtists.OrderBy(s => s.Name);

            List<AlphaKeyGroup<Artist>> itemSource = AlphaKeyGroup<Artist>.CreateGroups(listOfArtists,
    CultureInfo.CurrentUICulture,
    a => a.Name, true);

            ((CollectionViewSource)Resources["ListOfArtists"]).Source = itemSource;


            //DownloadImages(listOfArtists);
            OpenPage(NavMode == NavigationMode.Back);

            if (listOfArtists.Count == 0)
            {
                CreateEmptyLibraryNotification();

            }
            else
            {
            }
        }

        private void CreateEmptyLibraryNotification()
        {
            InAppNotification inAppNotification = new InAppNotification();
            string message;
            if (ApplicationInfo.Current.GetDeviceFormFactorType() == ApplicationInfo.DeviceFormFactorType.Desktop
                || ApplicationInfo.Current.GetDeviceFormFactorType() == ApplicationInfo.DeviceFormFactorType.Tablet)
            {
                message = ApplicationInfo.Current.Resources.GetString("EmptyLibraryDesktopTip");
                inAppNotification.PrimaryButtonContent = ApplicationInfo.Current.Resources.GetString("SettingsString");
                inAppNotification.PrimaryButtonClicked += (s, a) => { NavigationService.Navigate(this, typeof(Settings), "path=dataManagement"); };
            }
            else
            {
                message = ApplicationInfo.Current.Resources.GetString("EmptyLibraryMobileTip");
            }

            inAppNotification.Title = ApplicationInfo.Current.Resources.GetString("EmptyLibrary");
            inAppNotification.Message = message;
            inAppNotification.Icon = "\uE783";

            InAppNotificationHelper.ShowNotification(inAppNotification);
        }

        private void OpenPage(bool reload)
        {
            progress.IsActive = false;
            //Storyboard sb = this.Resources["OpenPageTransition"] as Storyboard;

            //if (reload)
            //{
            //    layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;
            //}

            //sb.Begin();
        }

        private async void DownloadImages(Artist artist)
        {
            string lang = ApplicationInfo.Current.Language;

            //Debug.WriteLine("Preparing to download image: " + artist.Name);

            try
            {
                await Spotify.DownloadArtistImage(artist);
            }
            catch
            {

            }
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
            {
                //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("OpenArtistConnectedAnimation", (UIElement)listView.ContainerFromItem(e.ClickedItem));
                NavigationService.Navigate(this, typeof(ArtistPage), e.ClickedItem);
            }
        }

        private void Artist_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 1200, sender as Image, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut), false, 200);
            animation.Begin();
        }

        private async void Artist_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Artist art = ((Image)sender).DataContext as Artist;

            if (string.IsNullOrWhiteSpace(art.Name))
                return;

            await Task.Run(() =>
            {
                if (ImageHelper.IsDownloadEnabled)
                {
                    DownloadImages(art);
                }
            });
        }


        private void Artist_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch)
            {
                Artist artist = (sender as FrameworkElement).DataContext as Artist;

                CreateArtistPopup(artist, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void Artist_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Touch && e.HoldingState == HoldingState.Started && listView.SelectionMode == ListViewSelectionMode.None)
            {
                Artist artist = (sender as FrameworkElement).DataContext as Artist;

                ActivateSelecionMode(artist);
                //CreateArtistPopup(artist, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void CreateArtistPopup(Artist artist, object sender, Point point)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(artist, true, point);
        }

        private void SortByButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void SortByMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void CircleImage_ImageFailed(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunIdleAsync((s) =>
            {
                Artist art = ((FrameworkElement)sender).DataContext as Artist;

                if (ImageHelper.IsDownloadEnabled)
                {
                    DownloadImages(art);
                }
            });
        }

        private void CircleImage_ActionClick(object sender, EventArgs e)
        {
            CircleImage cimg = (CircleImage)sender;
            Artist art = cimg.DataContext as Artist;

            PlayerController.Play(art);
        }

        private void listView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties.Add("mediaItem", e.Items.First());
        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (((SemanticZoom)sender).IsZoomedInViewActive)
            {
                selectionItemsBar.Visibility = Visibility.Visible;
                //selectionGrid.Visibility = Visibility.Visible;
            }
            else
            {
                selectionItemsBar.Visibility = Visibility.Collapsed;
                //selectionGrid.Visibility = Visibility.Collapsed;
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

        private void ActivateSelecionMode(Artist artist)
        {
            if (listView.SelectedItems.Contains(artist))
                return;

            ApplicationInfo.Current.VibrateDevice(25);

            EnableSelectionMode();
            listView.SelectedItems.Add(artist);
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
            List<MediaItem> list = new List<MediaItem>();

            foreach (Artist album in listView.SelectedItems)
            {
                list.Add(album);
            }

            if (playMode == SelectedItemsBar.PlayMode.Play)
                PlayerController.Play(list);
            else
                PlayerController.AddToQueue(list, true);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_AddSelected(object sender, SelectedItemsBar.AddMode addMode)
        {
            List<MediaItem> mediaItems = new List<MediaItem>();

            foreach (Artist artist in listView.SelectedItems)
            {
                mediaItems.Add(artist);
            }

            List<string> list = await Collection.FetchSongs(mediaItems);

            if (addMode == SelectedItemsBar.AddMode.AddToPlaylist)
                PlaylistHelper.RequestPlaylistPicker(this, list);
            else
                PlayerController.AddToQueue(list);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_ShareSelected(object sender, RoutedEventArgs e)
        {
            List<MediaItem> mediaItems = new List<MediaItem>();

            foreach (Artist artist in listView.SelectedItems)
            {
                mediaItems.Add(artist);
            }

            List<string> list = await Collection.FetchSongs(mediaItems);

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
    }
}
