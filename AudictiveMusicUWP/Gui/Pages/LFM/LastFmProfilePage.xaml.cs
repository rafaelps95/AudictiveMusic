using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public sealed partial class LastFmProfilePage : Page
    {
        private bool IsAuthenticatedUserPage;

        private enum LastFmPage
        {
            Scrobbles,
            Following,
            Followers,
            TopMedia,
            Similar,
            Recommended
        }



        public string Bio
        {
            get { return (string)GetValue(BioProperty); }
            set { SetValue(BioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BioProperty =
            DependencyProperty.Register("Bio", typeof(string), typeof(LastFmProfilePage), new PropertyMetadata(string.Empty));



        private NavigationMode NavMode
        {
            get;
            set;
        }


        public LastFmProfilePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(LastUser))
                {
                    LoadUserInfo(e.Parameter as LastUser);
                }
                else if (e.Parameter.GetType() == typeof(LastArtist))
                {
                    LoadArtistInfo(e.Parameter as LastArtist);
                }
                else
                {

                }
            }

            OpenPage(NavMode == NavigationMode.Back);
        }

        private void LoadUserInfo(LastUser user)
        {
            this.DataContext = user;
            header.SetContext(user);
            similarButton.Visibility = Visibility.Collapsed;

            if (LastFm.Current.IsAuthenticated)
            {
                this.IsAuthenticatedUserPage = user.Name.ToLower() == ApplicationSettings.LastFmSessionUsername.ToLower();
                
                NavigateToPage(LastFmPage.Scrobbles, user);
            }

            if (this.IsAuthenticatedUserPage)
                ApplicationSettings.LastFmSessionUserImageUri = user.Avatar.Large.AbsoluteUri;
        }

        private void NavigateToPage(LastFmPage page, object param)
        {
            switch (page)
            {
                case LastFmPage.Following:
                    frame.Navigate(typeof(FollowingPage), param, new DrillInNavigationTransitionInfo());
                    break;
                case LastFmPage.Scrobbles:
                    frame.Navigate(typeof(RecentScrobblesPage), param, new DrillInNavigationTransitionInfo());
                    break;
                case LastFmPage.TopMedia:
                    frame.Navigate(typeof(TopMediaPage), param, new DrillInNavigationTransitionInfo());
                    break;
                case LastFmPage.Similar:
                    frame.Navigate(typeof(LastFmListPage), param, new DrillInNavigationTransitionInfo());
                    break;
                case LastFmPage.Recommended:
                    frame.Navigate(typeof(LastFmListPage), param, new DrillInNavigationTransitionInfo());
                    break;
            };
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


        private void LoadArtistInfo(LastArtist lastArtist)
        {
            this.DataContext = lastArtist;
            header.SetContext(lastArtist);
            recentButton.Visibility = followingButton.Visibility = Visibility.Collapsed;

            this.Bio = lastArtist.Bio.Content;
            if (string.IsNullOrWhiteSpace(this.Bio) == false)
                bioGrid.Visibility = Visibility.Visible;

            NavigateToPage(LastFmPage.TopMedia, lastArtist);
        }

        private void recentButton_Checked(object sender, RoutedEventArgs e)
        {
            topMediaButton.IsChecked = followingButton.IsChecked = similarButton.IsChecked = false;
        }

        private void topMediaButton_Checked(object sender, RoutedEventArgs e)
        {
            recentButton.IsChecked = followingButton.IsChecked = similarButton.IsChecked = false;
        }

        private void followingButton_Checked(object sender, RoutedEventArgs e)
        {
            topMediaButton.IsChecked = recentButton.IsChecked = similarButton.IsChecked = false;
        }

        private void followersButton_Checked(object sender, RoutedEventArgs e)
        {
            topMediaButton.IsChecked = followingButton.IsChecked = similarButton.IsChecked = recentButton.IsChecked = false;
        }

        private void recentButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(LastFmPage.Scrobbles, this.DataContext);
        }

        private void topMediaButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(LastFmPage.TopMedia, this.DataContext);
        }

        private void followingButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(LastFmPage.Following, this.DataContext);
        }

        private void followersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(LastFmPage.Followers, this.DataContext);
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(RecentScrobblesPage))
            {
                recentButton.IsChecked = true;
            }
            else if (e.SourcePageType == typeof(TopMediaPage))
            {
                topMediaButton.IsChecked = true;
            }
            else if (e.SourcePageType == typeof(FollowingPage))
            {
                followingButton.IsChecked = true;
            }
            else if (e.SourcePageType == typeof(LastFmListPage))
            {
                similarButton.IsChecked = true;
            }
        }

        private void similarButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(LastFmPage.Similar, this.DataContext);
        }

        private void similarButton_Checked(object sender, RoutedEventArgs e)
        {
            topMediaButton.IsChecked = followingButton.IsChecked = recentButton.IsChecked = false;
        }

        private void recommendedButton_Checked(object sender, RoutedEventArgs e)
        {
            topMediaButton.IsChecked = followingButton.IsChecked = recentButton.IsChecked = similarButton.IsChecked = false;
        }

        private void recommendedButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(LastFmPage.Recommended, this.DataContext);
        }

        private void BioTB_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            OpenBioFlyout(sender);
        }

        private void OpenBioFlyout(object sender)
        {
            bioFlyoutTB.Text = this.Bio;

            if (ApplicationInfo.Current.IsMobile)
                bioTB.ContextFlyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full;
            else
                bioTB.ContextFlyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Top;
            bioTB.ContextFlyout.ShowAt((FrameworkElement)sender);
        }

        private void ReadMore_Click(object sender, RoutedEventArgs e)
        {
            OpenBioFlyout(sender);
        }
    }
}
