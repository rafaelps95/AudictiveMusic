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
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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
    public sealed partial class Albums : Page //, INotifyPropertyChanged
    {
        private bool updated;

        public event PropertyChangedEventHandler PropertyChanged;

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
            DependencyProperty.Register("AlbumItemLength", typeof(double), typeof(Albums), new PropertyMetadata(100));


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
                    LoadAlbums();
                }
            }
        }

        private int RepeatButtonCycleCounter;
        private bool ClickEventIsTouch;
        private bool CursorEnteredMegaFlyout;
        private Thickness TouchFlyoutMargin;

        public Albums()
        {
            this.SizeChanged += Albums_SizeChanged;
            CollectionHasBeenUpdated = false;
            this.InitializeComponent();
            RepeatButtonCycleCounter = 0;
            ClickEventIsTouch = false;
            CursorEnteredMegaFlyout = false;
            TouchFlyoutMargin = new Thickness();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Albums_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 510)
            {
                AlbumItemLength = (e.NewSize.Width - 55) / 2;
            }
            else if (e.NewSize.Width >= 510 && e.NewSize.Width < 610)
            {
                AlbumItemLength = (e.NewSize.Width - 80) / 3;
            }
            else if (e.NewSize.Width >= 610 && e.NewSize.Width < 710)
            {
                AlbumItemLength = (e.NewSize.Width - 100) / 4;
            }
            else if (e.NewSize.Width >= 710 && e.NewSize.Width < 810)
            {
                AlbumItemLength = (e.NewSize.Width - 110) / 5;
            }
            //else if (e.NewSize.Width >= 810)
            //{
            //    AlbumItemLength = 150;
            //}
            else
            {
                AlbumItemLength = 180;

            }

            //selectionGrid.Margin = new Thickness(20, 20, 20, ApplicationInfo.Current.FooterHeight + 20);

            //pageTitle.Text = AlbumItemLength.ToString() + " / " + e.NewSize.Width.ToString();

            //var panel = (ItemsWrapGrid)listView.ItemsPanelRoot;
            //panel.ItemWidth = AlbumItemLength;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            progress.IsActive = true;

            Collection.SongsChanged -= Collection_SongsChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            if (((CollectionViewSource)Resources["ListOfAlbums"]).Source == null || CollectionHasBeenUpdated)
            {
                CollectionHasBeenUpdated = false;

                await Task.Run(() => LoadAlbums());
            }
            else
            {

            }

            progress.IsActive = false;
            Collection.SongsChanged += Collection_SongsChanged;
        }

        private void Collection_SongsChanged(object sender, RoutedEventArgs e)
        {
            LoadAlbums();
        }

        public async void LoadAlbums()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                ((CollectionViewSource)Resources["ListOfAlbums"]).Source = null;
            });

            List<Album> listOfAlbums = Ctr_Album.Current.GetAlbums();
            List<AlphaKeyGroup<Album>> itemSource;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AlbumsSortBy"))
            {
                if (ApplicationData.Current.LocalSettings.Values["AlbumsSortBy"].ToString() == Sorting.SortByTitle)
                {
                    //listOfAlbums.OrderBy(s => s.Name);
                    itemSource = AlphaKeyGroup<Album>.CreateGroups(listOfAlbums,
    CultureInfo.InvariantCulture,
    a => a.Name, true);
                }
                else if (ApplicationData.Current.LocalSettings.Values["AlbumsSortBy"].ToString() == Sorting.SortByArtist)
                {
                    //listOfAlbums.OrderBy(s => s.Artist);
                    itemSource = AlphaKeyGroup<Album>.CreateGroups(listOfAlbums,
    CultureInfo.InvariantCulture,
    a => a.Artist, true);
                }
                else if (ApplicationData.Current.LocalSettings.Values["AlbumsSortBy"].ToString() == Sorting.SortByYear)
                {
                    //listOfAlbums.OrderBy(s => s.Year);
                    itemSource = AlphaKeyGroup<Album>.CreateGroups(listOfAlbums,
    CultureInfo.InvariantCulture,
    a => Convert.ToString(a.Year), true);
                }
                else
                {
                    itemSource = AlphaKeyGroup<Album>.CreateGroups(listOfAlbums,
    CultureInfo.InvariantCulture,
    a => a.Name, true);
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["AlbumsSortBy"] = Sorting.SortByTitle;
                itemSource = AlphaKeyGroup<Album>.CreateGroups(listOfAlbums,
    CultureInfo.InvariantCulture,
    a => a.Name, true);
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                ((CollectionViewSource)Resources["ListOfAlbums"]).Source = itemSource;
            });
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
            {
                ApplicationData.Current.LocalSettings.Values["UseTransition"] = true;
                NavigationHelper.Navigate(this, typeof(AlbumPage), e.ClickedItem);
            }
        }

        private void AlbumCover_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 1200, sender as Image, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut), false, 200);
            animation.Begin();
        }




        private void albumItemOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {

            }
        }

        private void albumItemOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {

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
            PopupHelper.GetInstance(sender).ShowPopupMenu(album, true, point);
        }

        private void playAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Album alb = btn.DataContext as Album;

            PlayerController.Play(alb);
        }

        private void listView_Loaded(object sender, RoutedEventArgs e)
        {
            var sv = (ScrollViewer)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.listView, 0), 0);
            sv.ViewChanged += Sv_ViewChanged;
        }

        private void Sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            AlbumItemHelper.PointerIsInContact = false;
        }

        private void Album_RightTapped(object sender, RoutedEventArgs e)
        {
            Album album = (sender as FrameworkElement).DataContext as Album;

            CreateAlbumPopup(album, sender, new Point(0,0));
        }

        private void AlbumItem_MenuTriggered(object sender, Point point)
        {
            Album album = (sender as FrameworkElement).DataContext as Album;

            //if (
            CreateAlbumPopup(album, sender, point);
        }

        private void AlbumItem_LongPressed(object sender, LongPressEventArgs args)
        {
            Album album = (sender as FrameworkElement).DataContext as Album;
            if (args.TriggeredByTouch && listView.SelectionMode == ListViewSelectionMode.None)
                ActivateSelecionMode(album);
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
            List<MediaItem> mediaItems = new List<MediaItem>();

            foreach (Album album in listView.SelectedItems)
            {
                mediaItems.Add(album);
            }

            if (playMode == SelectedItemsBar.PlayMode.Play)
                PlayerController.Play(mediaItems);
            else
                PlayerController.AddToQueue(mediaItems, true);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_AddSelected(object sender, SelectedItemsBar.AddMode addMode)
        {
            List<MediaItem> mediaItems = new List<MediaItem>();

            foreach (Album album in listView.SelectedItems)
            {
                mediaItems.Add(album);
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

            foreach (Album album in listView.SelectedItems)
            {
                mediaItems.Add(album);
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
