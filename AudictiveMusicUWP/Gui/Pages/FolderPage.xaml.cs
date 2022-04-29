using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FolderPage : Page
    {
        private NavigationMode NavMode
        {
            get;
            set;
        }

        private int SelectedItemsCount;

        private StorageFolder CurrentFolder { get; set; }

        public FolderPage()
        {
            this.SizeChanged += FolderPage_SizeChanged;
            this.SelectedItemsCount = 0;

            this.InitializeComponent();
        }

        private void FolderPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (e.NewSize.Width < 510)
            //{
            //    refreshButton.MaxWidth = 55;
            //}
            //else if (e.NewSize.Width >= 510 && e.NewSize.Width < 610)
            //{
            //    refreshButton.MaxWidth = 900;
            //}
            //else if (e.NewSize.Width >= 610 && e.NewSize.Width < 710)
            //{
            //    refreshButton.MaxWidth = 900;
            //}
            //else if (e.NewSize.Width >= 710 && e.NewSize.Width < 810)
            //{
            //    refreshButton.MaxWidth = 900;
            //}
            //else
            //{
            //    refreshButton.MaxWidth = 900;
            //}

            selectionGrid.Margin = new Thickness(20, 20, 20, ApplicationInfo.Current.FooterHeight + 20);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            progress.IsActive = true;
            Storyboard sb = this.Resources["ExitPageTransition"] as Storyboard;
            sb.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavMode = e.NavigationMode;

            if (e.Parameter != null)
            {
                this.CurrentFolder = e.Parameter as StorageFolder;
                pageTitle.Text = this.CurrentFolder.Path;
            }
            else
            {
                this.CurrentFolder = KnownFolders.MusicLibrary;
                pageTitle.Text = this.CurrentFolder.DisplayName;
            }

            LoadFolder();
        }

        private async void LoadFolder()
        {
            var items = await StorageHelper.ReadFolder(this.CurrentFolder);
            List<FolderItem> source = new List<FolderItem>();

            FolderItem aux;
            foreach (IStorageItem item in items)
            {
                aux = new FolderItem(item.Name, item.Path, item.IsOfType(StorageItemTypes.Folder));
                //if (aux.IsFolder == false)
                //{
                //    if (CheckIfSongExists(aux.Path) == false)
                //    {

                //    }

                //}

                source.Add(aux);
            }

            source.OrderBy(f => f.IsFolder).OrderBy(f => f.Name);

            ItemsList.ItemsSource = source;

            OpenPage(NavMode == NavigationMode.Back);
        }

        private bool CheckIfSongExists(string path)
        {
            return Ctr_Song.Current.SongExists(new Song() { SongURI = path });
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

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            progress.IsActive = true;

            layoutRoot.Opacity = 0;
            layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;

            LoadFolder();
        }

        private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isAdded;
            int i = 0;
            IList<object> list;
            /// O EVENTO FOI CHAMADO PORQUE ITENS FORAM MARCADOS
            if (e.AddedItems.Count > 0)
            {
                list = e.AddedItems;
                isAdded = true;
            }
            /// O EVENTO FOI CHAMADO PORQUE ITENS FORAM DESMARCADOS
            else
            {
                list = e.RemovedItems;
                isAdded = false;
            }

            foreach (object obj in list)
            {
                FolderItem item = obj as FolderItem;

                var items = Ctr_Song.Current.GetSongsByPath(item.Path);

                i += items.Count;
            }

            if (isAdded)
                this.SelectedItemsCount += i;
            else
                this.SelectedItemsCount -= i;

            string s = this.SelectedItemsCount + " " + ApplicationInfo.Current.GetSingularPlural(i, "ItemSelected");

            selectedItemsLabel.Text = s;

            if (this.SelectedItemsCount > 0)
            {
                topPlay.IsEnabled = topAdd.IsEnabled = topMore.IsEnabled = true;

                selectedItemsLabel.Visibility = Visibility.Visible;
            }
            else
            {
                topPlay.IsEnabled = topAdd.IsEnabled = topMore.IsEnabled = false;
                selectedItemsLabel.Text = string.Empty;
                selectedItemsLabel.Visibility = Visibility.Collapsed;
            }
        }

        private async void ItemsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ItemsList.SelectionMode != ListViewSelectionMode.None)
                return;

            FolderItem item = e.ClickedItem as FolderItem;

            if (item.IsFolder)
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                NavigationHelper.Navigate(this, typeof(FolderPage), folder);
            }
            else
            {
                if (StorageHelper.IsMusicFile(item.Path))
                    MessageService.SendMessageToBackground(new SetPlaylistMessage(new List<string>() { item.Path }));
            }

        }

        private void folderItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                FolderItem item = (sender as FrameworkElement).DataContext as FolderItem;

                if (item.IsFolder)
                    return;

                if (!StorageHelper.IsMusicFile(item.Path))
                    return;

                Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = item.Path });
                CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void folderItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && e.HoldingState == HoldingState.Started)
            {
                FolderItem item = (sender as FrameworkElement).DataContext as FolderItem;

                if (item.IsFolder)
                    return;

                if (!StorageHelper.IsMusicFile(item.Path))
                    return;

                Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = item.Path });
                CreateSongPopup(song, sender, e.GetPosition(sender as FrameworkElement));
            }
        }

        private void CreateSongPopup(Song song, object sender, Point point)
        {
            this.ShowPopupMenu(song, sender, Enumerators.MediaItemType.Song, true, point);
        }

        private void ItemsList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var listViewItem = args.ItemContainer;

            if (listViewItem != null)
            {
                var model = (FolderItem)args.Item;

                if (model.IsFolder == false)
                {
                    if (StorageHelper.IsMusicFile(model.Path) == false)
                        listViewItem.IsEnabled = false;

                    // OR
                    //listViewItem.IsEnabled = false;
                }
            }
        }

        private async void topPlay_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (object obj in ItemsList.SelectedItems)
            {
                FolderItem item = obj as FolderItem;
                if (item.IsFolder)
                {
                    list.AddRange(await Ctr_FolderItem.GetSongs(item));
                }
                else
                {
                    if (StorageHelper.IsMusicFile(item.Path))
                        list.Add(item.Path);
                }
            }

            if (list.Count == 0)
                return;

            MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        private async void topAdd_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (object obj in ItemsList.SelectedItems)
            {
                FolderItem item = obj as FolderItem;
                if (item.IsFolder)
                {
                    list.AddRange(await Ctr_FolderItem.GetSongs(item));
                }
                else
                {
                    if (StorageHelper.IsMusicFile(item.Path))
                        list.Add(item.Path);
                }
            }

            PlaylistHelper.RequestPlaylistPicker(this, list);

            DisableSelectionMode();
        }

        private async void topMore_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (object obj in ItemsList.SelectedItems)
            {
                FolderItem item = obj as FolderItem;
                if (item.IsFolder)
                {
                    list.AddRange(await Ctr_FolderItem.GetSongs(item));
                }
                else
                {
                    if (StorageHelper.IsMusicFile(item.Path))
                        list.Add(item.Path);
                }
            }

            if (list.Count == 0)
                return;

            this.ShowPopupMenu(list, sender, Enumerators.MediaItemType.ListOfStrings);
        }

        private void selection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisableSelectionMode();
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectionMode == ListViewSelectionMode.None)
                EnableSelectionMode();
            else
                DisableSelectionMode();
        }

        private void EnableSelectionMode()
        {
            selectButton.Content = "";
            ItemsList.SelectionMode = ListViewSelectionMode.Multiple;
            ItemsList.SelectionChanged += ItemsList_SelectionChanged;
            topAppBar.Visibility = Visibility.Visible;
        }

        private void DisableSelectionMode()
        {
            selectButton.Content = "";
            ItemsList.SelectedItem = null;
            ItemsList.SelectionChanged -= ItemsList_SelectionChanged;
            ItemsList.SelectionMode = ListViewSelectionMode.None;
            topAppBar.Visibility = Visibility.Collapsed;
        }
    }
}
