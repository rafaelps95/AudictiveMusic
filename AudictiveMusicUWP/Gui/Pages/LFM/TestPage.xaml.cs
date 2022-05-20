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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages.LFM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage : Page
    {

        private NavigationMode NavMode
        {
            get;
            set;
        }

        public TestPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            LoadPendingScrobbles();
        }

        private async void LoadPendingScrobbles()
        {
            var cachedScrobbles = await LastFm.Current.Client.Scrobbler.GetCachedAsync();
            List<Scrobble> list = cachedScrobbles.ToList();

            listView.ItemsSource = list;

            OpenPage(NavMode == NavigationMode.Back);
        }

        private void OpenPage(bool reload)
        {
            progress.IsActive = false;
            Storyboard sb = this.Resources["OpenPageTransition"] as Storyboard;

            if (reload)
            {
                layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;
            }

            sb.Begin();
        }

    }
}
