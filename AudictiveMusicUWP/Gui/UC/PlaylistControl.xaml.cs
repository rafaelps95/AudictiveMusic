using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class PlaylistControl : UserControl
    {
        public ObservableCollection<Song> PlaylistList = new ObservableCollection<Song>();
        //private Compositor _compositor;
        //private SpriteVisual blurSprite;
        //private CompositionEffectBrush _brush;

        private int tempDragOldIndex;
        private int tempDragNewIndex;
        private object tempDragItem;

        private bool reordering { get => tempDragItem != null; }

        public enum EditMode
        {
            Enabled,
            Disabled
        }

        public EditMode editMode
        {
            get;
            set;
        }

        public static readonly DependencyProperty CurrentTrackProperty = 
            DependencyProperty.Register("CurrentTrackIndex", typeof(int), typeof(PlaylistControl), new PropertyMetadata(0));





        public Color AccentColor
        {
            get { return (Color)GetValue(AccentColorProperty); }
            set { SetValue(AccentColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register("AccentColor", typeof(Color), typeof(PlaylistControl), new PropertyMetadata(Colors.Transparent));



        public PlaylistControl()
        {
            PlaylistList.CollectionChanged += PlaylistList_CollectionChanged;
            this.Loaded += PlaylistControl_Loaded;
            this.InitializeComponent();
            MusicPlaylist.ItemsSource = PlaylistList;
            CurrentTrackIndex = -1;
            editMode = EditMode.Disabled;
            ThemeSettings.CurrentThemeColorChanged += ApplicationSettings_CurrentThemeColorChanged;
            ThemeSettings.TransparencyEffectToggled += ApplicationSettings_TransparencyEffectToggled;
            ThemeSettings.PerformanceModeToggled += ApplicationSettings_PerformanceModeToggled;
            //if (ApplicationInfo.Current.IsMobile == false)
            //_compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        }

        private void ApplicationSettings_CurrentThemeColorChanged()
        {
            //if (ApplicationSettings.CurrentThemeColor.IsDarkColor())
            //    this.RequestedTheme = ElementTheme.Dark;
            //else
            //    this.RequestedTheme = ElementTheme.Light;

            //acrylic.AcrylicTint = ApplicationSettings.CurrentThemeColor;
        }

        private void PlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (ApplicationSettings.CurrentThemeColor.IsDarkColor())
            //    this.RequestedTheme = ElementTheme.Dark;
            //else
            //    this.RequestedTheme = ElementTheme.Light;

            //acrylic.AcrylicTint = ApplicationSettings.CurrentThemeColor;
            SetAcrylic();
        }

        private void ApplicationSettings_PerformanceModeToggled()
        {
            SetAcrylic();
        }

        private void ApplicationSettings_TransparencyEffectToggled()
        {
            SetAcrylic();
        }

        private void SetAcrylic()
        {
            if (ThemeSettings.IsPerformanceModeEnabled == false)
                acrylic.AcrylicEnabled = ThemeSettings.IsTransparencyEnabled;
            else
                acrylic.AcrylicEnabled = false;
        }

        private void PlaylistList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                tempDragOldIndex = e.OldStartingIndex;
                tempDragItem = e.OldItems[0];
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                MusicPlaylist.SelectionChanged -= Playlist_SelectionChanged;
                tempDragNewIndex = e.NewStartingIndex;
                PlayerController.MoveItem(tempDragOldIndex, tempDragNewIndex);
                tempDragItem = null;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move
                || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                int newIndex = e.NewStartingIndex;

                
            }
        }

        public void Clear()
        {
            MusicPlaylist.SelectionChanged -= Playlist_SelectionChanged;
            PlaylistList.Clear();
            MusicPlaylist.SelectionChanged += Playlist_SelectionChanged;
        }

        public int Count
        {
            get
            {
                return PlaylistList.Count;
            }
        }

        public void SetIndex(int index)
        {

        }

        public void Insert(int position, Song item)
        {
            PlaylistList.CollectionChanged -= PlaylistList_CollectionChanged;
            PlaylistList.Insert(position, item);
            PlaylistList.CollectionChanged += PlaylistList_CollectionChanged;
        }

        public void Add(Song item)
        {
            PlaylistList.CollectionChanged -= PlaylistList_CollectionChanged;
            PlaylistList.Add(item);
            PlaylistList.CollectionChanged += PlaylistList_CollectionChanged;
        }

        public void AddRange(List<Song> list)
        {
            PlaylistList.CollectionChanged -= PlaylistList_CollectionChanged;
            foreach (Song song in list)
                PlaylistList.Add(song);
            PlaylistList.CollectionChanged += PlaylistList_CollectionChanged;
        }

        public void SetLoadingState(bool state)
        {
            progress.IsActive = state;
        }

        public async void ScrollToSelectedIndex()
        {
            try
            {
                if (MusicPlaylist.Items.Count - 1 >= CurrentTrackIndex)
                    await MusicPlaylist.ScrollToIndex(CurrentTrackIndex);
            }
            catch
            {

            }
        }

        public int CurrentTrackIndex
        {
            get { return (int)this.GetValue(CurrentTrackProperty); }
            set
            {
                this.SetValue(CurrentTrackProperty, value);
                if (editMode == EditMode.Disabled)
                {
                    if (value <= PlaylistList.Count - 1)
                    {
                        MusicPlaylist.SelectionChanged -= Playlist_SelectionChanged;
                        MusicPlaylist.SelectedIndex = value;
                        MusicPlaylist.SelectionChanged += Playlist_SelectionChanged;
                    }
                }
            }
        }

        private void UpdatePreviousAndNext()
        {
            //if (PlaylistList.Count == 1)
            //{
            //    ApplicationData.Current.LocalSettings.Values["PreviousSong"] = PlaylistList[0].SongURI;
            //    ApplicationData.Current.LocalSettings.Values["NextSong"] = PlaylistList[0].SongURI;
            //}
            //else
            //{
            //    string prev, next = string.Empty;
            //    if (CurrentTrackIndex > 0)
            //        prev = PlaylistList[CurrentTrackIndex - 1].SongURI;
            //    else
            //        prev = PlaylistList[PlaylistList.Count - 1].SongURI;

            //    ApplicationData.Current.LocalSettings.Values["PreviousSong"] = prev;

            //    if (CurrentTrackIndex < PlaylistList.Count - 1)
            //        next = PlaylistList[CurrentTrackIndex + 1].SongURI;
            //    else
            //        next = PlaylistList[0].SongURI;

            //    ApplicationData.Current.LocalSettings.Values["NextSong"] = next;
            //}
        }

        private void RemoveSelectedSongs_Click(object sender, RoutedEventArgs e)
        {
            int index = PlaylistList.IndexOf(MusicPlaylist.SelectedItem as Song);
            PlaylistList.CollectionChanged -= PlaylistList_CollectionChanged;
            PlaylistList.RemoveAt(index);
            PlaylistList.CollectionChanged += PlaylistList_CollectionChanged;

            UpdatePreviousAndNext();

            PlayerController.RemoveIndex(index);
        }

        private void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicPlaylist.SelectedItem == null)
                return;

            if (reordering)
                return;

            PlayerController.SkipToIndex(MusicPlaylist.SelectedIndex);
        }

        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            foreach (Song item in PlaylistList)
            {
                list.Add(item.SongURI);
            }

            PlaylistHelper.RequestPlaylistPicker(this, list);
        }

        private void MusicPlaylist_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            ShowDropArea();

            e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;

            Song song = e.Items.First() as Song;
            int index = PlaylistList.IndexOf(song);

            e.Data.SetData("indexToRemove", index);
        }

        private void ShowDropArea()
        {
            dropArea.IsHitTestVisible = true;
            Storyboard sb = this.Resources["showDropAreaAnimation"] as Storyboard;
            sb.Begin();
        }


        private async void dropArea_Drop(object sender, DragEventArgs e)
        {
            //HideDropArea();
            object obj = await e.DataView.GetDataAsync("indexToRemove");
            int index = Convert.ToInt32(obj);

            tempDragItem = null;
            tempDragNewIndex = 0;
            tempDragOldIndex = 0;

            PlaylistList.CollectionChanged -= PlaylistList_CollectionChanged;
            PlaylistList.RemoveAt(index);
            PlaylistList.CollectionChanged += PlaylistList_CollectionChanged;

            UpdatePreviousAndNext();

            PlayerController.RemoveIndex(index);

            HideDropAreaOverlayColor();
        }

        private void HideDropArea()
        {
            dropArea.IsHitTestVisible = false;

            Storyboard sb = this.Resources["hideDropAreaAnimation"] as Storyboard;
            sb.Begin();
        }

        private void MusicPlaylist_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            HideDropArea();
        }

        private void dropArea_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            /*e.DragUIOverride.IsCaptionVisible = */e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.Caption = ApplicationInfo.Current.Resources.GetString("Remove/Content");

            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 200, dropAreaOverlay, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void MusicPlaylist_Drop(object sender, DragEventArgs e)
        {
            HideDropArea();
        }

        private void MusicPlaylist_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            HideDropArea();
        }

        private void dropArea_DragLeave(object sender, DragEventArgs e)
        {
            HideDropAreaOverlayColor();
        }

        private void HideDropAreaOverlayColor()
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 200, dropAreaOverlay, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.ClearPlaylist();
        }
    }
}
