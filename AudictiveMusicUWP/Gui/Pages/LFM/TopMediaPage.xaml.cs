using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages.LFM
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class TopMediaPage : Page
    {

        private enum PageLayout
        {
            ThreeColumns = 810,
            TwoColumns = 540,
            SingleColumn = 270,
            Unknown = 0
        }

        private PageLayout pageLayout = PageLayout.Unknown;

        private bool IsAuthenticatedUserPage;

        private int? TopTracksMaximumValue
        {
            get
            {
                //return itemLenght;
                return (int?)GetValue(TopTracksMaximumValueProperty);
            }
            set
            {
                //itemLenght = value;
                SetValue(TopTracksMaximumValueProperty, value);
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumItemLenght"));
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty TopTracksMaximumValueProperty =
            DependencyProperty.Register("TopTracksMaximumValue", typeof(int), typeof(TopMediaPage), new PropertyMetadata(0));

        private int? TopArtistsMaximumValue
        {
            get
            {
                //return itemLenght;
                return (int?)GetValue(TopArtistsMaximumValueProperty);
            }
            set
            {
                //itemLenght = value;
                SetValue(TopArtistsMaximumValueProperty, value);
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumItemLenght"));
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty TopArtistsMaximumValueProperty =
            DependencyProperty.Register("TopArtistsMaximumValue", typeof(int), typeof(TopMediaPage), new PropertyMetadata(0));

        private int? TopAlbumsMaximumValue
        {
            get
            {
                //return itemLenght;
                return (int?)GetValue(TopAlbumsMaximumValueProperty);
            }
            set
            {
                //itemLenght = value;
                SetValue(TopAlbumsMaximumValueProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty TopAlbumsMaximumValueProperty =
            DependencyProperty.Register("TopAlbumsMaximumValue", typeof(int), typeof(TopMediaPage), new PropertyMetadata(0));

        public TopMediaPage()
        {
            this.SizeChanged += TopMediaPage_SizeChanged;
            this.InitializeComponent();
        }

        private async void TopMediaPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePageLayout(e.NewSize);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                this.DataContext = e.Parameter;
                if (e.Parameter.GetType() == typeof(LastUser))
                {
                    LoadUserInfo(e.Parameter as LastUser);
                }
                else if (e.Parameter.GetType() == typeof(LastArtist))
                {
                    LoadArtistInfo(e.Parameter as LastArtist);
                }

                UpdatePageLayout(new Size(this.ActualWidth, this.ActualHeight));
            }
        }

        private void UpdatePageLayout(Size newSize)
        {
            if (newSize.Width < (int)PageLayout.TwoColumns)
            {
                if (pageLayout != PageLayout.SingleColumn)
                {
                    if (this.DataContext.GetType() != typeof(LastArtist))
                        firstColumn.Width = new GridLength(1, GridUnitType.Star);

                    Grid.SetColumn(topArtistsGrid, 0);
                    Grid.SetColumn(topAlbumsGrid, 0);
                    Grid.SetColumn(topTracksGrid, 0);
                    Grid.SetColumnSpan(topArtistsGrid, 3);
                    Grid.SetColumnSpan(topAlbumsGrid, 3);
                    Grid.SetColumnSpan(topTracksGrid, 3);
                    Grid.SetRow(topArtistsGrid, 0);
                    Grid.SetRow(topAlbumsGrid, 1);
                    Grid.SetRow(topTracksGrid, 2);

                    pageLayout = PageLayout.SingleColumn;
                }

            }
            else if (newSize.Width >= (int)PageLayout.TwoColumns && newSize.Width < (int)PageLayout.ThreeColumns)
            {

                if (pageLayout != PageLayout.TwoColumns)
                {
                    if (this.DataContext.GetType() != typeof(LastArtist))
                    {
                        firstColumn.Width = new GridLength(0, GridUnitType.Pixel);
                        Grid.SetColumn(topArtistsGrid, 1);
                        Grid.SetColumn(topAlbumsGrid, 2);
                        Grid.SetColumn(topTracksGrid, 1);
                        Grid.SetColumnSpan(topArtistsGrid, 1);
                        Grid.SetColumnSpan(topAlbumsGrid, 1);
                        Grid.SetColumnSpan(topTracksGrid, 3);
                        Grid.SetRow(topArtistsGrid, 0);
                        Grid.SetRow(topAlbumsGrid, 0);
                        Grid.SetRow(topTracksGrid, 1);
                    }
                    else
                    {
                        Grid.SetColumn(topArtistsGrid, 0);
                        Grid.SetColumn(topAlbumsGrid, 1);
                        Grid.SetColumn(topTracksGrid, 2);
                        Grid.SetColumnSpan(topArtistsGrid, 1);
                        Grid.SetColumnSpan(topAlbumsGrid, 1);
                        Grid.SetColumnSpan(topTracksGrid, 1);
                        Grid.SetRow(topArtistsGrid, 0);
                        Grid.SetRow(topAlbumsGrid, 0);
                        Grid.SetRow(topTracksGrid, 0);
                    }
                    pageLayout = PageLayout.TwoColumns;
                }
            }
            else
            {
                if (this.DataContext.GetType() == typeof(LastArtist))
                {
                    if (pageLayout != PageLayout.TwoColumns)
                    {
                        Grid.SetColumn(topArtistsGrid, 0);
                        Grid.SetColumn(topAlbumsGrid, 1);
                        Grid.SetColumn(topTracksGrid, 2);
                        Grid.SetColumnSpan(topArtistsGrid, 1);
                        Grid.SetColumnSpan(topAlbumsGrid, 1);
                        Grid.SetColumnSpan(topTracksGrid, 1);
                        Grid.SetRow(topArtistsGrid, 0);
                        Grid.SetRow(topAlbumsGrid, 0);
                        Grid.SetRow(topTracksGrid, 0);
                        pageLayout = PageLayout.TwoColumns;
                    }
                }
                else
                {
                    if (pageLayout != PageLayout.ThreeColumns)
                    {
                        firstColumn.Width = new GridLength(1, GridUnitType.Star);

                        Grid.SetColumn(topArtistsGrid, 0);
                        Grid.SetColumn(topAlbumsGrid, 1);
                        Grid.SetColumn(topTracksGrid, 2);
                        Grid.SetColumnSpan(topArtistsGrid, 1);
                        Grid.SetColumnSpan(topAlbumsGrid, 1);
                        Grid.SetColumnSpan(topTracksGrid, 1);
                        Grid.SetRow(topArtistsGrid, 0);
                        Grid.SetRow(topAlbumsGrid, 0);
                        Grid.SetRow(topTracksGrid, 0);

                        pageLayout = PageLayout.ThreeColumns;
                    }
                }
            }
            Debug.WriteLine(string.Format("WindowWidth: {0}, PageLayout: {1}", newSize.Width.ToString(), pageLayout.ToString()));
        }

        private async void LoadArtistInfo(LastArtist lastArtist)
        {
            recentTopArtists.Visibility = Visibility.Collapsed;
            firstColumn.Width = new GridLength(0, GridUnitType.Pixel);

            var resultALB = await LastFm.Current.Client.Artist.GetTopAlbumsAsync(lastArtist.Name, true, 1, 10);
            if (resultALB.Success)
            {
                var list = resultALB.Content.ToList();
                if (list.Count > 0)
                {
                    LoadTopAlbums(list);
                }
            }

            var result = await LastFm.Current.Client.Artist.GetTopTracksAsync(lastArtist.Name, true, 1, 10);
            if (result.Success)
            {
                var list = result.Content.ToList();
                if (list.Count > 0)
                {
                    LoadArtistTopTracks(list);
                }
            }

            progress.IsActive = false;
        }

        private async void LoadUserInfo(LastUser user)
        {
            this.IsAuthenticatedUserPage = user.Name.ToLower() == ApplicationSettings.LastFmSessionUsername.ToLower();

            recentTopArtists.Visibility = Visibility.Visible;

            List<LastTrack> list;
            List<LastArtist> artList;

            if (!ApplicationInfo.Current.HasInternetConnection)
                return;

            var artResult = await LastFm.Current.Client.User.GetTopArtists(user.Name, IF.Lastfm.Core.Api.Enums.LastStatsTimeSpan.Overall, 1, 20);

            if (artResult.Success)
            {
                artList = artResult.Content.ToList();

                LoadTopArtists(artList);
            }

            var resultALB = await LastFm.Current.Client.User.GetTopAlbums(user.Name, IF.Lastfm.Core.Api.Enums.LastStatsTimeSpan.Overall, 1, 20);
            if (resultALB.Success)
            {
                var listALB = resultALB.Content.ToList();
                if (listALB.Count > 0)
                {
                    LoadTopAlbums(listALB);
                }
            }

            var result = await LastFm.Current.Client.User.GetTopTracks(user);
            if (result != null)
            {
                LoadTopTracks(result);
            }

            progress.IsActive = false;
        }

        private void LoadTopAlbums(List<LastAlbum> list)
        {
            list = list.OrderByDescending(t => t.PlayCount).ToList();


            this.TopAlbumsMaximumValue = list[0].PlayCount;

            recentTopAlbums.ItemsSource = list;
            recentTopAlbums.Visibility = Visibility.Visible;
        }

        private void LoadArtistTopTracks(List<LastTrack> list)
        {
            list = list.OrderByDescending(t => t.PlayCount).ToList();


            this.TopTracksMaximumValue = list[0].PlayCount;

            recentTopTracks.ItemTemplate = this.Resources["artistTopTracksTemplate"] as DataTemplate;
            recentTopTracks.ItemsSource = list;
            recentTopTracks.Visibility = Visibility.Visible;
        }




        private void LoadTopArtists(List<LastArtist> artList)
        {
            artList = artList.OrderByDescending(a => a.PlayCount).ToList();
            List<CustomLastArtist> claList = new List<CustomLastArtist>();
            foreach (LastArtist la in artList)
            {
                CustomLastArtist cla = new CustomLastArtist();
                cla.SetLastArtist(la);
                claList.Add(cla);
            }

            this.TopArtistsMaximumValue = artList[0].PlayCount;


            recentTopArtists.ItemsSource = claList;
            recentTopArtists.Visibility = Visibility.Visible;
        }

        private void LoadTopTracks(List<LastTrack> list)
        {
            list = list.OrderByDescending(t => t.PlayCount).ToList();

            this.TopTracksMaximumValue = list[0].PlayCount;

            recentTopTracks.ItemTemplate = this.Resources["artistTopTracksTemplate"] as DataTemplate;
            recentTopTracks.ItemsSource = list;
            recentTopTracks.Visibility = Visibility.Visible;
        }

        private async void recentTopArtists_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomLastArtist la = e.ClickedItem as CustomLastArtist;

            var result = await LastFm.Current.Client.Artist.GetInfoAsync(la.Name, ApplicationInfo.Current.Language, true);

            if (result.Success)
            {
                LastArtist artist = result.Content;
                //artist.PlayCount = la.PlayCount;
                NavigationService.Navigate(this, typeof(LastFmProfilePage), artist);
            }
        }

        private async void CircleImage_ImageFailed(object sender, RoutedEventArgs e)
        {
            CustomLastArtist art = ((FrameworkElement)sender).DataContext as CustomLastArtist;

            if (string.IsNullOrWhiteSpace(art.Name))
                return;

            await Task.Run(() =>
            {
                if (ImageHelper.IsDownloadEnabled)
                {
                    DownloadImages(art.Artist);
                }
            });
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

    }
}
