using ClassLibrary.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class RecentScrobblesPage : Page
    {
        private bool IsAuthenticatedUserPage;

        public RecentScrobblesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(LastUser))
                {
                    LoadUserInfo(e.Parameter as LastUser);
                }
            }
        }


        private void LoadUserInfo(LastUser user)
        {
            this.DataContext = user;
            LoadRecentScrobbles();
        }

        private void reloadScrobblesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserInfo(this.DataContext as LastUser);
        }

        private async void LoadRecentScrobbles()
        {
            progress.IsActive = true;
            LastUser user = this.DataContext as LastUser;

            if (!ApplicationInfo.Current.HasInternetConnection)
                return;

            DateTimeOffset dateTime = DateTimeOffset.Now.ToUniversalTime();
            dateTime = dateTime.AddDays(-7);
            List<LastTrack> list;
            List<LastArtist> artList;

            var result = await LastFm.Current.Client.User.GetRecentScrobbles(user.Name, dateTime, 1, 20);
            if (result.Success)
            {
                list = result.Content.ToList();
                if (list.Count > 0)
                {
                    recentScroblles.ItemsSource = list;
                    recentScroblles.Visibility = Visibility.Visible;
                }
            }

            progress.IsActive = false;
        }

    }
}
