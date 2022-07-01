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
using ClassLibrary.Control;
using ClassLibrary.Entities.LastFmHelper;
using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Entities;
using System.Collections.ObjectModel;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages.LFM
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class FollowingPage : Page
    {
        private int currentPage = 0;
        private int totalPages = 0;
        const int resultsPerPage = 50;

        private ObservableCollection<CustomLastUser> FriendsList = new ObservableCollection<CustomLastUser>();
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
            DependencyProperty.Register("GridItemSize", typeof(double), typeof(FollowingPage), new PropertyMetadata(100));


        public FollowingPage()
        {
            this.SizeChanged += FollowingPage_SizeChanged;
            this.InitializeComponent();
        }

        private void FollowingPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
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
            listView.ItemsSource = FriendsList;
            LoadFollowedUsers(1, resultsPerPage);
        }

        private async void LoadFollowedUsers(int page = 1, int resultsPerPage = 50)
        {
            progress.IsActive = true;

            LastUser user = this.DataContext as LastUser;

            if (!ApplicationInfo.Current.HasInternetConnection)
                return;

            var response = await LastFm.Current.Client.User.GetFollowedUsers(user, page, resultsPerPage);
            if (response != null)
            {
                currentPage = Convert.ToInt32(response.friends.attr.page);
                totalPages = Convert.ToInt32(response.friends.attr.totalPages);

                foreach (User u in response.friends.user)
                {
                    FriendsList.Add(new CustomLastUser(u));
                }
            }

            progress.IsActive = false;
            if (currentPage < totalPages)
                loadMoreButton.Visibility = Visibility.Visible;
            else
                loadMoreButton.Visibility = Visibility.Collapsed;
        }

        private async void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomLastUser u = e.ClickedItem as CustomLastUser;

            var result = await LastFm.Current.Client.User.GetInfoAsync(u.Name);

            if (result.Success)
            {
                LastUser user = result.Content;
                NavigationService.Navigate(this, typeof(LastFmProfilePage), user);
            }
        }

        private void listView_SizeChanged(object sender, SizeChangedEventArgs e)
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

        private void LoadMoreButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFollowedUsers(currentPage + 1, resultsPerPage);
        }
    }
}
