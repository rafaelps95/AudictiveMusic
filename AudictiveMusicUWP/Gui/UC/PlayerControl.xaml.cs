using AudictiveMusicUWP.Gui.Pages;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using ClassLibrary.Helpers.Enumerators;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class PlayerControl : UserControl
    {
        public delegate void ViewChangedEventArgs(DisplayMode mode);
        public delegate void PlayerTooltipEventHandler(NextTooltip.Mode mode);

        public event ViewChangedEventArgs ViewChanged;
        public event PlayerTooltipEventHandler TooltipRequested;
        public event RoutedEventHandler TooltipDismissed;

        //True for horizontal translation and False for vertical translation. Null for no manipulation
        private bool? _isManipulatingX = null;
        private double _playlistActualWidth
        {
            get { return playlist.ActualWidth; }
        }

        private bool _eventsSet = false;

        private bool IsPlaying
        {
            get
            {
                if (BackgroundMediaPlayer.Current == null)
                    return false;

                if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.None)
                    return false;
                else
                    return true;
            }
        }
        public enum DisplayMode
        {
            Compact,
            Full
        }

        private DisplayMode mode;
        public DisplayMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;

                UpdateView();
            }
        }

        private bool _isManipulating;

        private bool IsManipulating
        {
            get
            {
                return _isManipulating;
            }
            set
            {
                _isManipulating = value;
                albumCover.IsHitTestVisible = !value;
            }
        }
        bool forcePreviousSkip;
        private CompositionEffectBrush _brush;
        private Compositor _compositor;
        private SpriteVisual sprite;

        private string CurrentArtist
        {
            get;
            set;
        }

        private string CurrentAlbumID
        {
            get;
            set;
        }

        private bool PlaylistHasBeenUpdated
        {
            get;
            set;
        }

        private bool AllowSliderChange
        {
            get;
            set;
        }

        private int CurrentTrack
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("CurrentTrackIndex"))
                    return Convert.ToInt32(ApplicationData.Current.LocalSettings.Values["CurrentTrackIndex"]);
                else
                    return 0;
            }
        }

        private string CurrentTrackPath
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("CurrentTrackPath"))
                    return Convert.ToString(ApplicationData.Current.LocalSettings.Values["CurrentTrackPath"]);
                else
                    return string.Empty;
            }
        }

        public bool IsPlaylistOpened
        {
            get;
            set;
        }

        public bool IsPlaylistLoaded
        {
            get;
            set;
        }

        private DispatcherTimer Tick
        {
            get;
            set;
        }



        bool _suppressAnimation;
        bool singleTap;
        public PlayerControl()
        {
            this.SizeChanged += PlayerControl_SizeChanged;
            this.InitializeComponent();
            this.mode = DisplayMode.Compact;
            forcePreviousSkip = false;
            IsPlaylistLoaded = false;
            IsPlaylistOpened = false;
            PlaylistHasBeenUpdated = true;
            AllowSliderChange = true;
            Tick = new DispatcherTimer();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            Application.Current.Resuming += Current_Resuming;
            ThemeSettings.BlurLevelChanged += ApplicationSettings_BlurLevelChanged;
            ThemeSettings.NowPlayingThemeChanged += ApplicationSettings_NowPlayingThemeChanged;
            ThemeSettings.CurrentThemeColorChanged += ApplicationSettings_CurrentThemeColorChanged;
            ThemeSettings.ThemeBackgroundPreferenceChanged += ApplicationSettings_ThemeBackgroundPreferenceChanged;
            PlayerController.FullPlayerRequested += PlayerController_FullPlayerRequested;
            Ctr_Song.FavoritesChanged += Ctr_Song_FavoritesChanged;
        }

        private async void PlayerController_BackgroundActionRequested(BackgroundAudioShared.Messages.Action action)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (action == BackgroundAudioShared.Messages.Action.ClearPlayback)
                {
                    DisablePlayer();
                }
            });
        }

        private async void PlayerController_CurrentStateChanged(MediaPlaybackState state)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Debug.WriteLine("CURRENT STATE: " + state.ToString());

                UpdateButtons();

                if (state == MediaPlaybackState.None)
                {
                    DisablePlayer();
                }
                else if (state == MediaPlaybackState.Playing)
                {
                    Tick.Start();
                }
                else
                {
                    Tick.Stop();
                }
            });
        }

        private async void PlayerController_IndexReceived(int index)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                playlist.CurrentTrackIndex = index;
                UpdatePlayerInfo();
            });
        }

        private async void PlayerController_PlaylistReceived(List<string> playlist)
        {
            await Dispatcher.RunIdleAsync(async (s) =>
            {
                PlaylistHasBeenUpdated = true;
                Debug.WriteLine("PLAYLISTMESSAGE...\nOK");

                await LoadPlaylist(playlist);
                if (playlist.Count < 2)
                    nextSong.Visibility = Visibility.Collapsed;
                else
                {
                    nextSong.Visibility = Visibility.Visible;
                }
                UpdateNextSong();
            });
        }

        private void Ctr_Song_FavoritesChanged(Song updatedSong)
        {
            if (updatedSong.SongURI == albumCover.CurrentSong.SongURI)
                albumCover.CurrentSong = ApplicationSettings.CurrentSong;
        }

        private void PlayerController_FullPlayerRequested(bool suppressAnimation)
        {
            _suppressAnimation = suppressAnimation;
            Mode = DisplayMode.Full;
        }

        private void ApplicationSettings_ThemeBackgroundPreferenceChanged()
        {
            if (ApplicationSettings.CurrentSong != null)
                UpdateBackground(ApplicationSettings.CurrentSong);
        }

        private void ApplicationSettings_CurrentThemeColorChanged()
        {
            UpdateThemeColor(ThemeSettings.CurrentThemeColor);
        }

        private void ApplicationSettings_NowPlayingThemeChanged()
        {
            SetBackgroundStyle();
        }

        private void Current_Resuming(object sender, object e)
        {
            SetBackgroundStyle();
        }

        private void Touch3D_VisibilityChanged(bool isVisible)
        {

        }

        private void ApplicationSettings_BlurLevelChanged()
        {
            SetBackgroundStyle();
        }

        private async void touch3D_ActionRequested(object sender, Touch3DEventArgs e)
        {
            if (e.Action == Touch3DEventArgs.Type.OpenArtist)
            {
                Artist artist = new Artist()
                {
                    Name = e.Argument,
                };

                NavigationService.Navigate(this, typeof(ArtistPage), artist);
            }
            else if (e.Action == Touch3DEventArgs.Type.LikeSong)
            {
                LikeDislikeSong();
            }
            else if (e.Action == Touch3DEventArgs.Type.ShareSong)
            {
                if (await ShareHelper.Instance.Share(ApplicationSettings.CurrentSong) == false)
                {
                    MessageDialog md = new MessageDialog("Não foi possível compartilhar este item");
                    await md.ShowAsync();
                }
            }
            else if (e.Action == Touch3DEventArgs.Type.AddSong)
            {
                List<string> list = new List<string>();
                list.Add(ApplicationSettings.CurrentTrackPath);
                PlaylistHelper.RequestPlaylistPicker(this, list);
            }
        }

        public void InitializePlayer()
        {
            ApplicationSettings.AppState = AppState.Active;

            if (_eventsSet == false)
            {
                PlayerController.PlaylistReceived += PlayerController_PlaylistReceived;
                PlayerController.IndexReceived += PlayerController_IndexReceived;
                PlayerController.CurrentStateChanged += PlayerController_CurrentStateChanged;
                PlayerController.BackgroundActionRequested += PlayerController_BackgroundActionRequested;
                _eventsSet = true;
            }
            //ApplicationData.Current.LocalSettings.Values["AppState"] = AppState.Active.ToString();

            UpdateSliderInfo();
            Tick.Interval = TimeSpan.FromMilliseconds(250);
            Tick.Tick -= Tick_Tick;
            Tick.Tick += Tick_Tick;


            if (string.IsNullOrWhiteSpace(CurrentTrackPath) == false)
            {
                UpdatePlayerInfo();
            }

            UpdateButtons();

            try
            {
                if (ApplicationSettings.BackgroundTaskState == BackgroundTaskState.Running)
                    PlayerController.RequestPlaylist();

                if (PlayerController.Current.CurrentState == MediaPlaybackState.Playing)
                {
                    Tick.Start();
                }
            }
            catch
            {

            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("RepeatMode"))
            {
                string value = ApplicationData.Current.LocalSettings.Values["RepeatMode"].ToString();

                if (value == "All")
                {
                    repeatToggleButton.Content = "";
                    repeatToggleButton.IsChecked = true;
                    BackgroundMediaPlayer.Current.IsLoopingEnabled = false;
                }
                else if (value == "Single")
                {
                    repeatToggleButton.Content = "";
                    repeatToggleButton.IsChecked = null;
                    BackgroundMediaPlayer.Current.IsLoopingEnabled = true;
                }
                else
                {
                    repeatToggleButton.Content = "";
                    repeatToggleButton.IsChecked = false;
                    BackgroundMediaPlayer.Current.IsLoopingEnabled = false;
                }
            }
            else
            {
                repeatToggleButton.Content = "";
                repeatToggleButton.IsChecked = false;
            }
        }

        public void ClearPlayerState(bool removeHandlers)
        {
            if (removeHandlers)
            {
                PlayerController.PlaylistReceived -= PlayerController_PlaylistReceived;
                PlayerController.IndexReceived -= PlayerController_IndexReceived;
                PlayerController.CurrentStateChanged -= PlayerController_CurrentStateChanged;
                PlayerController.BackgroundActionRequested -= PlayerController_BackgroundActionRequested;
                _eventsSet = false;
            }

            CurrentArtist = string.Empty;
            //backgroundBitmapImage.UriSource = null;
            //compactAlbumBitmap.UriSource = new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute);
            //albumCover.CurrentSong = null;
            Tick.Tick -= Tick_Tick;
            HidePlaylist();
            Tick.Stop();
        }

        private void Tick_Tick(object sender, object e)
        {
            UpdateSliderInfo();
        }

        private void UpdateSliderInfo()
        {
            if (BackgroundMediaPlayer.Current != null)
            {
                if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState != MediaPlaybackState.None)
                {
                    MusicProgress.Maximum = BackgroundMediaPlayer.Current.PlaybackSession.NaturalDuration.TotalSeconds;
                    MusicProgress.Value = BackgroundMediaPlayer.Current.PlaybackSession.Position.TotalSeconds;

                    if (AllowSliderChange)
                    {
                        musicSlider.Maximum = BackgroundMediaPlayer.Current.PlaybackSession.NaturalDuration.TotalSeconds;
                        musicSlider.Value = BackgroundMediaPlayer.Current.PlaybackSession.Position.TotalSeconds;
                        MusicPositionTime.Text = TimeSpan.FromSeconds(musicSlider.Value).ToString(@"mm\:ss");
                        MusicDurationTime.Text = TimeSpan.FromSeconds(musicSlider.Maximum).ToString(@"mm\:ss");
                    }
                }
                else
                {
                    musicSlider.Value = 0;
                    musicSlider.Maximum = 100;
                    MusicProgress.Value = 0;
                    MusicProgress.Maximum = 100;
                }
            }
            else
            {
                musicSlider.Value = 0;
                musicSlider.Maximum = 100;
                MusicProgress.Value = 0;
                MusicProgress.Maximum = 100;
            }
        }

        private void UpdateView()
        {
            ViewChanged?.Invoke(this.Mode);

            if (_suppressAnimation)
            {
                _suppressAnimation = false;
                compactView.IsHitTestVisible = false;
                fullView.IsHitTestVisible = true;
                fullView.Opacity = 1;
                musicSlider.IsEnabled = albumCover.IsEnabled = true;
                compactView.Opacity = 0;
                fullViewTransform.TranslateY = 0;
                Animation.RunAnimation(this.Resources["fadeInBlur"]);
                this.Focus(FocusState.Keyboard);
                return;
            }

            Animation animation = new Animation();

            if (this.Mode == DisplayMode.Compact)
            {
                fullView.IsHitTestVisible = false;
                acrylic.Opacity = 0;
                albumCover.IsEnabled = nextSong.IsEnabled = optionsButton.IsEnabled = musicSlider.IsEnabled = closeButton.IsEnabled = false;
                HidePlaylist();

                compactBarExpandButton.IsTabStop = compactPreviousButton.IsTabStop = compactPlayPauseButton.IsTabStop = compactNextButton.IsTabStop = true;
                fullPlayPauseButton.IsTabStop = playlistButton.IsTabStop = repeatToggleButton.IsTabStop = shuffleButton.IsTabStop = playlistButton.IsTabStop = false;

                animation.AddDoubleAnimation(0, 150, fullView, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
                animation.AddDoubleAnimation(1, 150, compactView, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
                animation.AddDoubleAnimation(80, 150, fullViewTransform, "TranslateY", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

                animation.Completed += (s, a) =>
                {
                    compactView.IsHitTestVisible = true;
                    //fullView.Visibility = Visibility.Collapsed;
                };
            }
            else
            {

                compactView.IsHitTestVisible = false;
                fullView.IsHitTestVisible = true;
                albumCover.IsEnabled = nextSong.IsEnabled = optionsButton.IsEnabled = musicSlider.IsEnabled = closeButton.IsEnabled = true;
                compactBarExpandButton.IsTabStop = compactPreviousButton.IsTabStop = compactPlayPauseButton.IsTabStop = compactNextButton.IsTabStop = false;
                fullPlayPauseButton.IsTabStop = playlistButton.IsTabStop = repeatToggleButton.IsTabStop = shuffleButton.IsTabStop = playlistButton.IsTabStop = true;
                animation.AddDoubleAnimation(1, 150, fullView, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
                animation.AddDoubleAnimation(0, 150, compactView, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
                animation.AddDoubleAnimation(0, 150, fullViewTransform, "TranslateY", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

                animation.Completed += (s, a) =>
                {
                    Animation.RunAnimation(this.Resources["fadeInBlur"]);
                };

                if (ApplicationSettings.ThemesUserAware == false)
                {
                    SimpleNotice notice = new SimpleNotice();
                    notice.Caption = ApplicationInfo.Current.Resources.GetString("ThemesMessageCaption");
                    fullView.Children.Add(notice);
                    Canvas.SetZIndex(notice, 5);
                    notice.Dismissed += (s) =>
                    {
                        NavigationService.Navigate(this, typeof(ThemeSelector));
                        ApplicationSettings.ThemesUserAware = true;
                    };
                    notice.Show(ApplicationInfo.Current.Resources.GetString("ThemesMessage"), ApplicationInfo.Current.Resources.GetString("SetUp/Text"));
                }
            }

            animation.Begin();
        }

        private void PlayerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 500)
            {
                if (IsPlaylistOpened == false)
                    playlistTranslate.X = 500;

                try
                {
                    playlist.Width = 500;
                    //if (IsPlaylistOpened)
                    //    PlayerControlsContainerTranslate.X = playlist.Width * -1;
                }
                catch
                {

                }

                playlist.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                if (IsPlaylistOpened == false)
                    playlistTranslate.X = e.NewSize.Width;

                try
                {
                    playlist.Width = e.NewSize.Width;
                    //if (IsPlaylistOpened)
                    //    PlayerControlsContainerTranslate.X = playlist.Width * -1;
                }
                catch
                {

                }

                playlist.HorizontalAlignment = HorizontalAlignment.Right;
            }

            if (sprite != null)
            {
                try
                {
                    sprite.Size = e.NewSize.ToVector2();
                }
                catch
                {

                }
            }
        }

        private void UpdateButtons()
        {
            if (BackgroundMediaPlayer.Current == null)
                return;

            if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.None)
            {
                fullPreviousButton.IsEnabled = compactPreviousButton.IsEnabled = fullNextButton.IsEnabled = compactNextButton.IsEnabled = playlistButton.IsEnabled = repeatToggleButton.IsEnabled = shuffleButton.IsEnabled = albumCover.IsEnabled = nextSong.IsEnabled = false;
                fullPlayPauseButton.Content = compactPlayPauseButton.Content = "\uF5B0";
            }
            else
            {
                fullPreviousButton.IsEnabled = compactPreviousButton.IsEnabled = fullNextButton.IsEnabled = compactNextButton.IsEnabled = playlistButton.IsEnabled = repeatToggleButton.IsEnabled = shuffleButton.IsEnabled = albumCover.IsEnabled = nextSong.IsEnabled = true;

                if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    fullPlayPauseButton.Content = compactPlayPauseButton.Content = "\uF8AE";
                }
                else if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                    fullPlayPauseButton.Content = compactPlayPauseButton.Content = "\uF5B0";
                }
            }
        }

        private void UpdateNextSong()
        {
            if (ApplicationSettings.NextSong == "")
            {
                nextSong.Visibility = Visibility.Collapsed;
            }
            else if (IsPlaying)
            {
                nextSong.Visibility = Visibility.Visible;
            }

            bool updateSong = true;

            if (nextSong.Song == null)
                updateSong = true;
            else
            {
                if (nextSong.Song.SongURI == ApplicationSettings.NextSong)
                    updateSong = false;
                else
                    updateSong = true;
            }

            if (updateSong)
            { 
                Song next = Ctr_Song.Current.GetSong(new Song() { SongURI = ApplicationSettings.NextSong });
                nextSong.SetSong(next);
            }
        }

        public void DisablePlayer()
        {
            Tick.Tick -= Tick_Tick;
            Tick.Stop();

            CurrentArtist = string.Empty;
            //UpdateBackground(null);
            SongName.Text = string.Empty;
            ArtistName.Text = string.Empty;
            //AlbumName.Text = string.Empty;

            //compactAlbumBitmap.UriSource = new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute);
            //albumCover.CurrentSong = null;

            HidePlaylist();

            UpdateSliderInfo();

            this.Mode = DisplayMode.Compact;

            ThemeSettings.CurrentThemeColor = Color.FromArgb(255, 77, 77, 77);
        }

        private async Task<bool> LoadPlaylist(List<string> list)
        {
            PlaylistHasBeenUpdated = false;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                playlist.Clear();

                playlist.SetLoadingState(true);
            });

            //Song aux;

            List<Song> songs = Ctr_Song.Current.GetSongsFromPlaylist(list);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => playlist.AddRange(songs));

            //foreach (string path in list)
            //{
            //    aux = Ctr_Song.Current.GetSong(new Song() { SongURI = path });
            //    if (aux != null)
            //    {
            //        aux.IsPlaying = aux.SongURI == ApplicationSettings.CurrentTrackPath;
            //        await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => playlist.Add(aux));
            //    }
            //}

            IsPlaylistLoaded = true;

            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    if (playlist.Count > 0)
                    {
                        playlist.CurrentTrackIndex = ApplicationSettings.CurrentTrackIndex;
                        playlist.ScrollToSelectedIndex();
                    }
                });
            }
            catch
            {

            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => playlist.SetLoadingState(false));

            return true;
        }

        public async void UpdatePlayerInfo()
        {
            if (ApplicationSettings.IsCollectionLoaded)
            {
                Debug.WriteLine("PLAYLIST AND COLLECTION: OK");

                if (ApplicationSettings.CurrentSong != null)
                {
                    SongName.Text = ApplicationSettings.CurrentSong.Name;
                    //await Task.Delay(50);
                    ArtistName.Text = ApplicationSettings.CurrentSong.Artist;
                    //AlbumName.Text = ApplicationSettings.CurrentSong.Album;
                    compactAlbumBitmap.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + ApplicationSettings.CurrentSong.AlbumID + ".jpg", UriKind.Absolute);
                    albumCover.CurrentSong = ApplicationSettings.CurrentSong;
                    albumCover.FallbackUri = new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute);

                    if (ThemeSettings.ThemeBackgroundPreference == ThemeBackgroundSource.AlbumCover)
                    {
                        if (CurrentAlbumID != ApplicationSettings.CurrentSong.AlbumID)
                        {
                            UpdateBackground(ApplicationSettings.CurrentSong);
                        }
                    }
                    else
                    {
                        if (CurrentArtist != ApplicationSettings.CurrentSong.Artist)
                        {
                            UpdateBackground(ApplicationSettings.CurrentSong);
                        }
                    }

                    CurrentAlbumID = ApplicationSettings.CurrentSong.AlbumID;
                    CurrentArtist = ApplicationSettings.CurrentSong.Artist;

                    if (ThemeSettings.ThemeColorPreference == ThemeColorSource.AlbumColor)
                        ThemeSettings.CurrentThemeColor = ImageHelper.GetColorFromHex(ApplicationSettings.CurrentSong.HexColor);
                    else if (ThemeSettings.ThemeColorPreference == ThemeColorSource.AccentColor)
                        ThemeSettings.CurrentThemeColor = ApplicationInfo.Current.CurrentSystemAccentColor;
                    else if (ThemeSettings.ThemeColorPreference == ThemeColorSource.CustomColor)
                        ThemeSettings.CurrentThemeColor = ThemeSettings.CustomThemeColor;

                    ToolTip toolTip = new ToolTip();
                    toolTip.Content = ApplicationSettings.CurrentSong.Name + " " + ApplicationInfo.Current.Resources.GetString("By") + ApplicationSettings.CurrentSong.Artist;
                    ToolTipService.SetToolTip(compactBarExpandButton, toolTip);


                    UpdateNextSong();
                }
                else
                {
                    
                }
                //UpdateBackground(ImageHelper.GetColorFromHex(element.GetAttribute("HexColor")), element.GetAttribute("AlbumID"));
            }
        }


        public void UpdateThemeColor(Color color)
        {
            Color darkerColor = color.ChangeColorBrightness(-0.7f);
            Color lighterColor = color.ChangeColorBrightness(0.4f);

            gradientStop1.Color = gradientStop3.Color = darkerColor;
            gradientStop2.Color = color.ChangeColorBrightness(-0.1f);

            if (ThemeSettings.NowPlayingTheme == Theme.Material)
            {
                //Color c = ImageHelper.GetColorFromHex("#FFDC572E");
                Color opposite = color.GetOppositeColor();
                Color strongest = color.ChangeColorBrightness(-0.3f);

                Animation animation = new Animation();
                animation.AddColorAnimation(color, 400, hexagonColorA, "Color", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
                animation.AddColorAnimation(opposite, 400, hexagonColorC, "Color", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
                animation.AddColorAnimation(strongest, 400, hexagonColorB, "Color", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

                animation.Begin();
            }

            if (color.IsDarkColor())
            {
                compactViewContent.RequestedTheme = ElementTheme.Dark;
                //MusicProgress.Foreground = new SolidColorBrush(lighterColor);
            }
            else
            {
                compactViewContent.RequestedTheme = ElementTheme.Light;
                //MusicProgress.Foreground = new SolidColorBrush(darkerColor);
            }
        }

        private void UpdateBackground(Song song)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 250, background, "Opacity");

            animation.Completed += async (s, a) =>
            {
                if (song != null)
                {
                    await Task.Delay(50);

                    backgroundBitmapImage.UriSource = null;

                    if (ThemeSettings.ThemeBackgroundPreference == ThemeBackgroundSource.AlbumCover)
                        backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + ApplicationSettings.CurrentSong.AlbumID + ".jpg");
                    else
                        backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Artists/artist_" + StringHelper.RemoveSpecialChar(song.Artist) + ".jpg");
                }
            };

            animation.Begin();
        }

        private void UpdateBackground(Color color, string albumID)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 250, background, "Opacity");

            animation.Completed += async (s, a) =>
            {
                await Task.Delay(50);
                backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + albumID + "_blur.jpg");
            };

            animation.Begin();
        }


        private void OpenPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //if (this.IsPlaying)
                this.Mode = DisplayMode.Full;
        }

        private void fullView_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackgroundStyle();
        }

        private void SetBackgroundStyle()
        {
            switch (ThemeSettings.NowPlayingTheme)
            {
                case Theme.Clean:

                    modernBG.Visibility = Visibility.Collapsed;
                    acrylic.Visibility = Visibility.Collapsed;
                    materialBG.Visibility = Visibility.Collapsed;

                    break;
                case Theme.Blur:
                    SetBlur();
                    break;
                case Theme.Modern:
                    materialBG.Visibility = Visibility.Collapsed;
                    SetModernStyle();
                    break;
                case Theme.Material:
                    materialBG.Visibility = Visibility.Visible;
                    SetModernStyle();
                    break;
            }
        }

        private void SetBlur()
        {
            modernBG.Visibility = Visibility.Collapsed;
            acrylic.Visibility = Visibility.Visible;
            materialBG.Visibility = Visibility.Collapsed;

            acrylic.AcrylicIntensity = ThemeSettings.NowPlayingBlurAmount;
            acrylic.AcrylicEnabled = true;
        }

        private async void SetModernStyle()
        {
            modernBG.Visibility = Visibility.Visible;
            acrylic.Visibility = Visibility.Collapsed;

            var canvasDevice = CanvasDevice.GetSharedDevice();
            var graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, canvasDevice);

            var bitmap = await CanvasBitmap.LoadAsync(canvasDevice, new Uri("ms-appx:///Assets/points.png"));

            var drawingSurface = graphicsDevice.CreateDrawingSurface(bitmap.Size,
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using (var ds = CanvasComposition.CreateDrawingSession(drawingSurface))
            {
                ds.Clear(Colors.Transparent);
                ds.DrawImage(bitmap);
            }

            var surfaceBrush = _compositor.CreateSurfaceBrush(drawingSurface);
            surfaceBrush.Stretch = CompositionStretch.None;

            var border = new BorderEffect
            {
                ExtendX = CanvasEdgeBehavior.Wrap,
                ExtendY = CanvasEdgeBehavior.Wrap,
                Source = new CompositionEffectSourceParameter("source")
            };

            var fxFactory = _compositor.CreateEffectFactory(border);
            var fxBrush = fxFactory.CreateBrush();
            fxBrush.SetSourceParameter("source", surfaceBrush);

            sprite = _compositor.CreateSpriteVisual();
            sprite.Size = new Vector2((float)modernBG.ActualWidth, (float)modernBG.ActualHeight);
            sprite.Brush = fxBrush;

            ElementCompositionPreview.SetElementChildVisual(modernBG, sprite);

            return;


            //if (ApplicationSettings.NowPlayingGrayscale)
            //{
            SpriteVisual _effectVisual;
                CompositionEffectBrush _effectBrush;
                var graphicsEffect = new SaturationEffect
                {
                    Name = "Saturation",
                    Saturation = 0.0f,
                    Source = new CompositionEffectSourceParameter("mySource")
                };

            var effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
               new[] { "Saturation.Saturation" });
            _effectBrush = effectFactory.CreateBrush();

                CompositionBackdropBrush backdrop = _compositor.CreateBackdropBrush();

                _effectBrush.SetSourceParameter("mySource", backdrop);

                _effectVisual = _compositor.CreateSpriteVisual();
                _effectVisual.Brush = _effectBrush;
                _effectVisual.Size = new Vector2(10000);

                ElementCompositionPreview.SetElementChildVisual(acrylic, _effectVisual);
            //}

        }

        private void compactView_Loaded(object sender, RoutedEventArgs e)
        {
            //bottomBarBlurSprite = _compositor.CreateSpriteVisual();

            //BlendEffectMode blendmode = BlendEffectMode.Overlay;

            //var graphicsEffect = new BlendEffect
            //{
            //    Mode = blendmode,
            //    Background = new ColorSourceEffect()
            //    {
            //        Name = "Tint",
            //        Color = Colors.Transparent,
            //    },

            //    Foreground = new GaussianBlurEffect()
            //    {
            //        Name = "Blur",
            //        Source = new CompositionEffectSourceParameter("Backdrop"),
            //        BlurAmount = 18.0f,
            //        BorderMode = EffectBorderMode.Hard,
            //    }
            //};

            //var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect,
            //    new[] { "Blur.BlurAmount", "Tint.Color" });

            //_brush = blurEffectFactory.CreateBrush();

            //var destinationBrush = _compositor.CreateBackdropBrush();
            //_brush.SetSourceParameter("Backdrop", destinationBrush);

            //bottomBarBlurSprite.Size = new Vector2((float)compactView.ActualWidth, (float)compactView.ActualHeight);
            //bottomBarBlurSprite.Brush = _brush;

            //ElementCompositionPreview.SetElementChildVisual(compactViewBlur, bottomBarBlurSprite);
        }


        private void currentAlbumBitmap_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 1, 400, albumCover, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();

        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.Previous(forcePreviousSkip);
            forcePreviousSkip = false;
        }

        private void previousButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
                return;

            TooltipRequested?.Invoke(NextTooltip.Mode.Previous);
        }

        private void nextButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
                return;

            TooltipRequested?.Invoke(NextTooltip.Mode.Next);
        }

        private void previousButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TooltipDismissed?.Invoke(this, new RoutedEventArgs());
        }

        private void nextButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TooltipDismissed?.Invoke(this, new RoutedEventArgs());
        }

        private void playPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPauseOrResume();
        }

        private void PlayPauseOrResume()
        {
            //if (BackgroundMediaPlayer.Current == null)

            if (ApplicationSettings.BackgroundTaskState != BackgroundTaskState.Running)
                PlayerController.Current.ResetAfterLostBackground();
            else
                InitializePlayer();

            PlayerController.PlayPause();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.Next();
        }

        private void TickBar_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            AllowSliderChange = true;
            musicSlider.ValueChanged -= MusicSlider_ValueChanged;

            BackgroundMediaPlayer.Current.PlaybackSession.Position = TimeSpan.FromSeconds(musicSlider.Value);
        }

        private void TickBar_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            AllowSliderChange = false;
            musicSlider.ValueChanged += MusicSlider_ValueChanged;
        }

        private void MusicSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MusicPositionTime.Text = TimeSpan.FromSeconds(musicSlider.Value).ToString(@"mm\:ss");
            MusicDurationTime.Text = TimeSpan.FromSeconds(musicSlider.Maximum).ToString(@"mm\:ss");
        }

        private void backgroundBitmapImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation.RunAnimation(this.Resources["fadeInBackground"]);
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (repeatToggleButton.IsChecked == true)
            {
                BackgroundMediaPlayer.Current.IsLoopingEnabled = false;
                ApplicationData.Current.LocalSettings.Values["RepeatMode"] = "All";
                repeatToggleButton.Content = "";
            }
            else if (repeatToggleButton.IsChecked == false)
            {
                repeatToggleButton.Content = "";
                ApplicationData.Current.LocalSettings.Values["RepeatMode"] = "None";
                BackgroundMediaPlayer.Current.IsLoopingEnabled = false;
            }
            else
            {
                repeatToggleButton.Content = "";
                ApplicationData.Current.LocalSettings.Values["RepeatMode"] = "Single";
                BackgroundMediaPlayer.Current.IsLoopingEnabled = true;
            }
        }

        private void playlistButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPlaylist();
        }

        private void ShowPlaylist()
        {
            playlist.Opacity = 1;
            playlistRightBorderOverlay.Opacity = 0;
            #region open playlist animation

            Animation animation = new Animation();
            animation.AddDoubleAnimation(0,
                250,
                playlistTranslate,
                "X", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Completed += (s, a) =>
            {
                playlist.IsHitTestVisible = true;
                playlist.IsTabStop = true;
                playlist.Focus(FocusState.Programmatic);

                if (IsPlaylistLoaded)
                    playlist.ScrollToSelectedIndex();
            };
            animation.Begin();

            #endregion 

            IsPlaylistOpened = true;
            dismissArea.IsHitTestVisible = true;

            if (ApplicationSettings.PlaylistReorderUserAware == false)
            {
                SimpleNotice notice = new SimpleNotice();
                fullView.Children.Add(notice);
                Canvas.SetZIndex(notice, 7);
                notice.Dismissed += (s) =>
                {
                    ApplicationSettings.PlaylistReorderUserAware = true;
                };
                notice.Show(ApplicationInfo.Current.Resources.GetString("PlaylistReorderMessage"), ApplicationInfo.Current.Resources.GetString("PlaylistReorderMessageTitle"));
            }
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.ShuffleCurrentPlaylist();
        }

        private void dismissArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            HidePlaylist();
        }

        public void HidePlaylist()
        {
            playlist.IsHitTestVisible = false;

            dismissArea.IsHitTestVisible = false;
            IsPlaylistOpened = false;
            playlist.IsTabStop = false;
            this.Focus(FocusState.Keyboard);

            Animation animation = new Animation();
            animation.AddDoubleAnimation(playlist.ActualWidth,
                250,
                playlistTranslate,
                "X", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void closePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            HidePlaylist();
        }

        private void albumCover_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private async void albumCover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.singleTap = true;
            await Task.Delay(200);
            if (this.singleTap == false)
                return;

            Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = CurrentTrackPath });
            Album album = new Album()
            {
                Name = song.Album,
                Artist = song.Artist,
                ID = song.AlbumID,
                Year = Convert.ToInt32(song.Year),
                Genre = song.Genre,
                HexColor = song.HexColor
            };
            ApplicationData.Current.LocalSettings.Values["UseTransition"] = true;

            NavigationService.Navigate(this, typeof(AlbumPage), album);

        }

        private void player_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this.IsManipulating = false;

            _isManipulatingX = null;
            if (fullViewTransform.TranslateY >= 60)
            {
                this.Mode = DisplayMode.Compact;
            }
            else
            {
                this.Mode = DisplayMode.Full;
            }
            if (playlistTranslate.X < _playlistActualWidth - 100)
            {
                ShowPlaylist();
            }
            else
            {
                HidePlaylist();
            }
        }

        private void player_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            this.IsManipulating = true;

            if (e.IsInertial)
            {
                e.Complete();
            }

            double opacityRatio = e.Delta.Translation.Y / 200 * -1;

            if (_isManipulatingX == null)
            {
                if (e.Cumulative.Translation.X < -20)
                    _isManipulatingX = true;
                else if (e.Cumulative.Translation.Y > 20)
                    _isManipulatingX = false;
            }

            if (_isManipulatingX == true)
            {
                playlist.Opacity = 1;
                //if (e.Cumulative.Translation.X < -20)
                //{
                    if (playlistTranslate.X >= 0 && playlistTranslate.X <= playlist.ActualWidth)
                    {
                        if (playlistTranslate.X + e.Delta.Translation.X >= 0 &&
                            playlistTranslate.X + e.Delta.Translation.X <= playlist.ActualWidth)
                        {
                            playlistTranslate.X += e.Delta.Translation.X;
                        }
                        //PlayerControlsContainerTranslate.X += e.Delta.Translation.X;
                    }
                //}
            }
            else if (_isManipulatingX == false)
            {
                //if (e.Cumulative.Translation.Y > 50)
                //{
                    if (fullViewTransform.TranslateY >= 0 && fullViewTransform.TranslateY <= 80)
                    {
                        if (fullViewTransform.TranslateY + e.Delta.Translation.Y >= 0 &&
                            fullViewTransform.TranslateY + e.Delta.Translation.Y <= 80)
                        {
                            fullViewTransform.TranslateY += e.Delta.Translation.Y;
                        }

                        if (fullView.Opacity >= 0.6 && fullView.Opacity <= 1.0)
                        {
                            if (fullView.Opacity + opacityRatio >= 0.6 &&
                            fullView.Opacity + opacityRatio <= 1.0)
                                fullView.Opacity += opacityRatio;
                        }
                    }
                //}
            }
        }

        private void touch3DOverlay_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void currentAlbumBitmap_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            compactAlbumBitmap.UriSource = new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute);
        }

        private async void backgroundBitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (ApplicationSettings.CurrentSong == null)
                return;

            StorageFolder coversFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
            var cover = await coversFolder.TryGetItemAsync("cover_" + ApplicationSettings.CurrentSong.AlbumID + ".jpg");
            if (cover != null)
                backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + ApplicationSettings.CurrentSong.AlbumID + ".jpg");
        }

        private void albumCover_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.singleTap = false;

            LikeDislikeSong();
        }

        private void LikeDislikeSong()
        {
            if (ApplicationSettings.CurrentSong.IsFavorite)
            {
                Ctr_Song.Current.SetFavoriteState(ApplicationSettings.CurrentSong, false);
            }
            else
            {
                Ctr_Song.Current.SetFavoriteState(ApplicationSettings.CurrentSong, true);
            }
        }

        private void previousButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                TooltipRequested?.Invoke(NextTooltip.Mode.Previous);
            }
            else if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
            {
                forcePreviousSkip = true;
            }
            else
            {
                TooltipDismissed?.Invoke(this, new RoutedEventArgs());
                forcePreviousSkip = false;
            }
        }

        private void nextButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                TooltipRequested?.Invoke(NextTooltip.Mode.Next);
            }
            else
            {
                TooltipDismissed?.Invoke(this, new RoutedEventArgs());
            }
        }

        public void SetCompactViewMargin(Thickness margin)
        {
            compactView.Margin = margin;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Mode = DisplayMode.Compact;
        }

        private void lastFmArtistProfile_Click(object sender, RoutedEventArgs e)
        {
            //var result = await LastFm.Current.Client.Artist.GetInfoAsync(this.CurrentSong.Artist);
            Artist art = new Artist()
            {
                Name = ApplicationSettings.CurrentSong.Artist,
            };
            NavigationService.Navigate(this, typeof(ArtistPage), art);
        }

        private void optionsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout mf = new MenuFlyout();

                MenuFlyoutItem mfi = new MenuFlyoutItem()
                {
                    Text = ApplicationInfo.Current.Resources.GetString("ThemeSettings"),
                };

            mfi.Click += (s, a) =>
            {
                NavigationService.Navigate(this, typeof(ThemeSelector));
            };

            mf.Items.Add(mfi);

            MenuFlyoutItem mfi2 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("TimerMenu"),
            };

            mfi2.Click += (s,a) =>
            {
                NavigationService.Navigate(this, typeof(Settings), "path=timer");
            };

            mf.Items.Add(mfi2);

            MenuFlyoutItem mfi3 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("ScrobbleSettings"),
            };

            mfi3.Click += (s, a) =>
            {
                NavigationService.Navigate(this, typeof(Settings), "path=scrobble");
            };

            mf.Items.Add(mfi3);


            mf.Items.Add(new MenuFlyoutSeparator());

            MenuFlyoutItem mfi4 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("GoToArtistString"),
            };

            mfi4.Click += lastFmArtistProfile_Click;

            mf.Items.Add(mfi4);

            mf.ShowAt((FrameworkElement)sender);
        }

        private void albumCover_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            //{
            //    Point point = e.GetCurrentPoint(this).Position;
            //    ShowTouch3D(point);
            //}
        }







        private void PlayerBottomBarInfoPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            BeginPressedAnimation();
        }

        private void PlayerBottomBarInfoPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            BeginNormalAnimation();
        }

        private void PlayerBottomBarInfoPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BeginPointerOverAnimation();
        }

        private void PlayerBottomBarInfoPointerExited(object sender, PointerRoutedEventArgs e)
        {
            BeginNormalAnimation();
        }

        private void PlayerBottomBarInfoPointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void PlayerBottomBarInfoPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            BeginNormalAnimation();
        }

        private void BeginPointerOverAnimation()
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 200, compactPlayerExpandIndicator, "Opacity");
            animation.Begin();
        }

        private void BeginNormalAnimation()
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 200, compactPlayerExpandIndicator, "Opacity");
            animation.Begin();
        }

        private void BeginPressedAnimation()
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 200, compactPlayerExpandIndicator, "Opacity");
            animation.Begin();
        }

        private void CompactBarExpandButton_Click(object sender, RoutedEventArgs e)
        {
            this.Mode = DisplayMode.Full;
        }

        private void AlbumCover_ArtistRequested(object sender, RoutedEventArgs e)
        {
            Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = CurrentTrackPath });

            ApplicationData.Current.LocalSettings.Values["UseTransition"] = true;

            Artist artist = new Artist()
            {
                Name = song.Artist,
            };

            NavigationService.Navigate(this, typeof(ArtistPage), artist);
        }

        private void AlbumCover_AddRequested(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            list.Add(ApplicationSettings.CurrentTrackPath);
            PlaylistHelper.RequestPlaylistPicker(this, list);
        }

        private void AlbumCover_Click(object sender, RoutedEventArgs e)
        {
            Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = CurrentTrackPath });
            Album album = new Album()
            {
                Name = song.Album,
                Artist = song.Artist,
                ID = song.AlbumID,
                Year = Convert.ToInt32(song.Year),
                Genre = song.Genre,
                HexColor = song.HexColor
            };
            ApplicationData.Current.LocalSettings.Values["UseTransition"] = true;

            NavigationService.Navigate(this, typeof(AlbumPage), album);
        }

        private void AlbumCover_FavoriteStateToggled(object sender, RoutedEventArgs e)
        {
            LikeDislikeSong();
        }

        private void NextSong_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlaylistRightBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse && !_isManipulating && this.IsPlaylistOpened == false)
                ShowPlaylistHint();
        }

        private void PlaylistRightBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse && !_isManipulating && this.IsPlaylistOpened == false)
                HidePlaylistHint();
        }

        private void PlaylistRightBorder_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse && !_isManipulating && this.IsPlaylistOpened == false)
                HidePlaylistHint();
        }

        private void PlaylistRightBorder_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse && !_isManipulating && this.IsPlaylistOpened == false)
                HidePlaylistHint();
        }

        private void ShowPlaylistHint()
        {
            playlist.Opacity = 0.5;
            Animation animation = new Animation();
            animation.AddDoubleAnimation(playlist.ActualWidth - 110,
                150,
                playlistTranslate,
                "X", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.AddDoubleAnimation(1,
                150,
                playlistRightBorderOverlay,
                "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void HidePlaylistHint()
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(playlist.ActualWidth,
                150,
                playlistTranslate,
                "X", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.AddDoubleAnimation(0,
                150,
                playlistRightBorderOverlay,
                "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();

            animation.Completed += (s, a) =>
            {
                playlist.Opacity = 1;
            };

            animation.Begin();
        }

        private void PlaylistRightBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse && this.IsPlaylistOpened == false)
                ShowPlaylist();
        }
    }
}
