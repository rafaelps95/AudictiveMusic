using AudictiveMusicUWP.Gui.UC;
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

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            progress.IsActive = true;
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

            listView.ItemsSource = source;

            OpenPage(NavMode == NavigationMode.Back);
        }

        private bool CheckIfSongExists(string path)
        {
            return Ctr_Song.Current.SongExists(new Song() { SongURI = path });
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

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            progress.IsActive = true;

            //layoutRoot.Opacity = 0;
            //layoutRootScale.ScaleX = layoutRootScale.ScaleY = 1.1;

            LoadFolder();
        }

        //private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    bool isAdded;
        //    int i = 0;
        //    IList<object> list;
        //    /// O EVENTO FOI CHAMADO PORQUE ITENS FORAM MARCADOS
        //    if (e.AddedItems.Count > 0)
        //    {
        //        list = e.AddedItems;
        //        isAdded = true;
        //    }
        //    /// O EVENTO FOI CHAMADO PORQUE ITENS FORAM DESMARCADOS
        //    else
        //    {
        //        list = e.RemovedItems;
        //        isAdded = false;
        //    }

        //    foreach (object obj in list)
        //    {
        //        FolderItem item = obj as FolderItem;

        //        var items = Ctr_Song.Current.GetSongsByPath(item.Path);

        //        i += items.Count;
        //    }

        //    if (isAdded)
        //        this.SelectedItemsCount += i;
        //    else
        //        this.SelectedItemsCount -= i;

        //    string s = this.SelectedItemsCount + " " + ApplicationInfo.Current.GetSingularPlural(i, "ItemSelected");

        //    selectedItemsLabel.Text = s;

        //    if (this.SelectedItemsCount > 0)
        //    {
        //        topPlay.IsEnabled = topAdd.IsEnabled = topMore.IsEnabled = true;

        //        selectedItemsLabel.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        topPlay.IsEnabled = topAdd.IsEnabled = topMore.IsEnabled = false;
        //        selectedItemsLabel.Text = string.Empty;
        //        selectedItemsLabel.Visibility = Visibility.Collapsed;
        //    }
        //}

        private async void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectionMode != ListViewSelectionMode.None)
                return;

            FolderItem item = e.ClickedItem as FolderItem;

            if (item.IsFolder)
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                NavigationService.Navigate(this, typeof(FolderPage), folder);
            }
            else
            {
                if (StorageHelper.IsMusicFile(item.Path))
                    PlayerController.Play(item);
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
            PopupHelper.GetInstance(sender).ShowPopupMenu(song, true, point);
        }

        private void listView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
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

        private void selection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisableSelectionMode();
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectionMode == ListViewSelectionMode.None)
                EnableSelectionMode();
            else
                DisableSelectionMode();
        }







        private void EnableSelectionMode()
        {
            Thickness padding = new Thickness(listView.Padding.Left, 70, listView.Padding.Right, listView.Padding.Bottom);
            listView.Padding = padding;

            listView.SelectionMode = ListViewSelectionMode.Multiple;
            listView.SelectionChanged += listView_SelectionChanged;
            selectionItemsBar.SelectionModeChanged -= SelectionItemsBar_SelectionModeChanged;
            selectionItemsBar.SelectionMode = SelectedItemsBar.BarMode.Enabled;
            selectionItemsBar.SelectionModeChanged += SelectionItemsBar_SelectionModeChanged;
        }

        private void DisableSelectionMode()
        {
            Thickness padding = new Thickness(listView.Padding.Left, 0, listView.Padding.Right, listView.Padding.Bottom);
            listView.Padding = padding;

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

        private async void SelectionItemsBar_PlaySelected(object sender, SelectedItemsBar.PlayMode playMode)
        {
            List<string> list = new List<string>();

            foreach (object obj in listView.SelectedItems)
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

            if (playMode == SelectedItemsBar.PlayMode.Play)
                PlayerController.Play(list);
            else
                PlayerController.AddToQueue(list, true);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_AddSelected(object sender, SelectedItemsBar.AddMode addMode)
        {
            List<string> list = new List<string>();

            foreach (object obj in listView.SelectedItems)
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

            if (addMode == SelectedItemsBar.AddMode.AddToPlaylist)
                PlaylistHelper.RequestPlaylistPicker(this, list);
            else
                PlayerController.AddToQueue(list);

            DisableSelectionMode();
        }

        private async void SelectionItemsBar_ShareSelected(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (object obj in listView.SelectedItems)
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
