using System;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Helpers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionPage : Page
    {
        private NavigationMode NavMode
        {
            get;
            set;
        }

        public CollectionPage()
        {
            this.Loaded += CollectionPage_Loaded;
            this.SizeChanged += CollectionPage_SizeChanged;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void CollectionPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 400)
            {
                navigationButtons.HorizontalAlignment = HorizontalAlignment.Left;
                artistsButton.FontSize = albumsButton.FontSize = songsButton.FontSize = 14;
            }
            else
            {
                navigationButtons.HorizontalAlignment = HorizontalAlignment.Center;
                artistsButton.FontSize = albumsButton.FontSize = songsButton.FontSize = 16;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.Refresh)
                return;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            string arguments = string.Empty;

            if (e.Parameter != null)
                arguments = e.Parameter.ToString();

            if (NavMode != NavigationMode.Back)
            {
                if (string.IsNullOrWhiteSpace(arguments) == false)
                {
                    if (NavigationService.ContainsAttribute(arguments, "page"))
                    {
                        if (NavigationService.GetParameter(arguments, "page") == "artists")
                        {
                            frame.Navigate(typeof(Artists));
                        }
                        else if (NavigationService.GetParameter(arguments, "page") == "albums")
                        {
                            frame.Navigate(typeof(Albums));
                        }
                        else if (NavigationService.GetParameter(arguments, "page") == "songs")
                        {
                            frame.Navigate(typeof(Songs));
                        }
                        else
                        {
                            //OTHER ACTIONS
                            frame.Navigate(typeof(Artists));
                        }
                    }
                    else
                    {
                        frame.Navigate(typeof(Artists));
                    }
                }
                else
                {
                    frame.Navigate(typeof(Artists));
                }
            }

            if (NavMode == NavigationMode.Refresh)
                return;

            OpenPage(NavMode == NavigationMode.Back);
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

        private void CollectionPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void artistsButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate(typeof(Artists));
        }

        private void Navigate(Type destination)
        {
            frame.Navigate(destination, null, new ContinuumNavigationTransitionInfo());
        }

        private void albumsButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate(typeof(Albums));
        }

        private void songsButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate(typeof(Songs));
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.ShuffleCollection();
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(Artists))
            {
                artistsButton.IsChecked = true;
                sortButton.Visibility = Visibility.Collapsed;
            }
            else if (e.SourcePageType == typeof(Albums))
            {
                albumsButton.IsChecked = true;
                sortButton.Visibility = Visibility.Visible;
            }
            else if (e.SourcePageType == typeof(Songs))
            {
                songsButton.IsChecked = true;
                sortButton.Visibility = Visibility.Visible;
            }
        }

        private void artistsButton_Checked(object sender, RoutedEventArgs e)
        {
            albumsButton.IsChecked = false;
            songsButton.IsChecked = false;
        }

        private void albumsButton_Checked(object sender, RoutedEventArgs e)
        {
            artistsButton.IsChecked = false;
            songsButton.IsChecked = false;
        }

        private void songsButton_Checked(object sender, RoutedEventArgs e)
        {
            artistsButton.IsChecked = false;
            albumsButton.IsChecked = false;
        }

        private void sortButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout fly = new MenuFlyout()
            {
                Placement = FlyoutPlacementMode.Bottom,
            };

            ToggleMenuFlyoutItem mfi = new ToggleMenuFlyoutItem();

            ToggleMenuFlyoutItem mfi2 = new ToggleMenuFlyoutItem();

            ToggleMenuFlyoutItem mfi3 = new ToggleMenuFlyoutItem();


            if (frame.SourcePageType == typeof(Albums))
            {
                mfi.Text = ApplicationInfo.Current.Resources.GetString("Artist");
                mfi2.Text = ApplicationInfo.Current.Resources.GetString("Title");

                mfi.Click += (s,a) => 
                {
                    ApplicationSettings.SaveSettingsValue("AlbumsSortBy", Sorting.SortByArtist);
                    Collection.RefreshUI(this);
                };
                mfi2.Click += (s, a) => 
                {
                    ApplicationSettings.SaveSettingsValue("AlbumsSortBy", Sorting.SortByTitle);
                    Collection.RefreshUI(this);
                };

                fly.Items.Add(mfi);
                fly.Items.Add(mfi2);

                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AlbumsSortBy"))
                {
                    if (ApplicationData.Current.LocalSettings.Values["AlbumsSortBy"].ToString() == Sorting.SortByArtist)
                    {
                        mfi.IsChecked = true;
                    }
                    else if (ApplicationData.Current.LocalSettings.Values["AlbumsSortBy"].ToString() == Sorting.SortByTitle)
                    {
                        mfi2.IsChecked = true;
                    }
                }
            }
            else if (frame.SourcePageType == typeof(Songs))
            {
                mfi.Text = ApplicationInfo.Current.Resources.GetString("Artist");
                mfi2.Text = ApplicationInfo.Current.Resources.GetString("Album");
                mfi3.Text = ApplicationInfo.Current.Resources.GetString("Title");

                mfi.Click += (s, a) => 
                {
                    ApplicationSettings.SaveSettingsValue("SongsSortBy", Sorting.SortByArtist);
                    Collection.RefreshUI(this);
                };
                mfi2.Click += (s, a) => 
                {
                    ApplicationSettings.SaveSettingsValue("SongsSortBy", Sorting.SortByAlbum);
                    Collection.RefreshUI(this);
                };
                mfi3.Click += (s, a) => 
                {
                    ApplicationSettings.SaveSettingsValue("SongsSortBy", Sorting.SortByTitle);
                    Collection.RefreshUI(this);
                };

                fly.Items.Add(mfi);
                fly.Items.Add(mfi2);
                fly.Items.Add(mfi3);

                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SongsSortBy"))
                {
                    if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByArtist)
                    {
                        mfi.IsChecked = true;
                    }
                    else if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByAlbum)
                    {
                        mfi2.IsChecked = true;
                    }
                    else if (ApplicationData.Current.LocalSettings.Values["SongsSortBy"].ToString() == Sorting.SortByTitle)
                    {
                        mfi3.IsChecked = true;
                    }
                }
            }

            fly.ShowAt((FrameworkElement)sender);
        }

        private void foldersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(this, typeof(FolderPage), null);
        }
    }
}
