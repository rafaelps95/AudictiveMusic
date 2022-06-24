using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Input;
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
    public sealed partial class PendingScrobbles : Page
    {
        private ObservableCollection<PendingScrobble> PendingList = new ObservableCollection<PendingScrobble>();

        public PendingScrobbles()
        {
            this.InitializeComponent();
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            listView.ItemsSource = this.PendingList;
            Ctr_PendingScrobble.Current.Updated += PendingScrobble_Updated;
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            //try
            //{
            //    sendButton.IsEnabled = this.PendingList.Count > 0 && ApplicationInfo.Current.HasInternetConnection;
            //}
            //catch
            //{

            //}
        }

        private void PendingScrobble_Updated(object sender, RoutedEventArgs e)
        {
            LoadPendingScrobbles();
        }

        private void LoadPendingScrobbles()
        {
            DisableSelectionMode();
            this.PendingList.Clear();
            List<PendingScrobble> list = Ctr_PendingScrobble.Current.GetPendingScrobbles();
            foreach (PendingScrobble ps in list)
                this.PendingList.Add(ps);
            progress.IsActive = false;

            EmptyListMessage.Visibility = this.PendingList.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            //sendButton.IsEnabled = this.PendingList.Count > 0 && ApplicationInfo.Current.HasInternetConnection;

            OpenPage(NavMode == NavigationMode.Back);
        }

        private bool updated;

        private NavigationMode NavMode
        {
            get;
            set;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            LoadPendingScrobbles();
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


        private void SongItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                Song song = (sender as FrameworkElement).DataContext as Song;
                CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void SongItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && e.HoldingState == HoldingState.Started)
            {
                Song song = (sender as FrameworkElement).DataContext as Song;
                CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void CreateSongPopup(Song song, object sender, Point point)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(song, true, point);
        }

        private void EnableSelectionMode()
        {
            Thickness padding = new Thickness(listView.Padding.Left, 70, listView.Padding.Right, listView.Padding.Bottom);
            listView.Padding = padding;

            listView.SelectionMode = ListViewSelectionMode.Multiple;
            listView.SelectionChanged += listView_SelectionChanged;
        }

        private void DisableSelectionMode()
        {
            Thickness padding = new Thickness(listView.Padding.Left, 0, listView.Padding.Right, listView.Padding.Bottom);
            listView.Padding = padding;

            listView.SelectedItem = null;
            listView.SelectionChanged -= listView_SelectionChanged;
            listView.SelectionMode = ListViewSelectionMode.None;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            List<PendingScrobble> list = Ctr_PendingScrobble.Current.GetPendingScrobbles();
            await Ctr_PendingScrobble.Current.SendScrobbles(this.PendingList.ToList());
        }
    }
}
