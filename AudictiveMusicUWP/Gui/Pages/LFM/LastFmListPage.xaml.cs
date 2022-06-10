using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages.LFM
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class LastFmListPage : Page
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
            DependencyProperty.Register("GridItemSize", typeof(double), typeof(LastFmListPage), new PropertyMetadata(100));

        private NavigationMode NavMode
        {
            get;
            set;
        }

        public LastFmListPage()
        {
            this.SizeChanged += LastFmListPage_SizeChanged;
            this.InitializeComponent();
        }

        private void LastFmListPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 410)
            {
                this.GridItemSize = (e.NewSize.Width - 60) / 3;
            }
            else if (e.NewSize.Width >= 410 && e.NewSize.Width < 610)
            {
                this.GridItemSize = (e.NewSize.Width - 65) / 4;
            }
            else if (e.NewSize.Width >= 610 && e.NewSize.Width < 710)
            {
                this.GridItemSize = (e.NewSize.Width - 80) / 5;
            }
            else if (e.NewSize.Width >= 710 && e.NewSize.Width < 810)
            {
                this.GridItemSize = (e.NewSize.Width - 90) / 6;
            }
            else
            {
                this.GridItemSize = (e.NewSize.Width - 100) / 7;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(LastTag))
                {
                    pageTitle.Text = (e.Parameter as LastTag).Name;
                    LoadTagTopArtists(e.Parameter as LastTag);
                }
                else if (e.Parameter.GetType() == typeof(LastArtist))
                {
                    header.Visibility = Visibility.Collapsed;
                    LoadSimilarArtists(e.Parameter as LastArtist);
                }
                else if (e.Parameter.GetType() == typeof(LastUser))
                {
                    header.Visibility = Visibility.Collapsed;
                    LoadRecommendedArtists();
                }
            }

            OpenPage(NavMode == NavigationMode.Back);
        }

        private async void LoadRecommendedArtists()
        {
            var list = await LastFm.Current.Client.User.GetRecommendedArtistsAsync();
            if (list != null)
            {
                listView.ItemsSource = list;
            }
            progress.IsActive = false;
        }

        private void LoadSimilarArtists(LastArtist lastArtist)
        {
            List<CustomLastArtist> claList = new List<CustomLastArtist>();
            foreach (LastArtist la in lastArtist.Similar)
            {
                CustomLastArtist cla = new CustomLastArtist();
                cla.SetLastArtist(la);
                claList.Add(cla);
            }

            listView.ItemsSource = claList;
            progress.IsActive = false;
        }

        private async void LoadTagTopArtists(LastTag lastTag)
        {
            try
            {
                var response = await LastFm.Current.Client.Tag.GetTopArtistsAsync(lastTag.Name);

                if (response.Success)
                {
                    var list = response.Content;

                    list = list.OrderByDescending(a => a.PlayCount).ToList();

                    List<CustomLastArtist> claList = new List<CustomLastArtist>();
                    foreach (LastArtist la in list)
                    {
                        CustomLastArtist cla = new CustomLastArtist();
                        cla.SetLastArtist(la);
                        claList.Add(cla);
                    }

                    listView.ItemsSource = claList;
                }
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

            progress.IsActive = false;
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
        private async void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomLastArtist la = e.ClickedItem as CustomLastArtist;

            var result = await LastFm.Current.Client.Artist.GetInfoAsync(la.Name, ApplicationInfo.Current.Language, true);

            if (result.Success)
            {
                LastArtist artist = result.Content;
                //artist.PlayCount = la.PlayCount;
                NavigationHelper.Navigate(this, typeof(LastFmProfilePage), artist);
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
