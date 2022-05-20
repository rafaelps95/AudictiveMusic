using AudictiveMusicUWP.Gui.Pages;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared;
using BackgroundAudioShared.Messages;
using ClassLibrary;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
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
        bool wasHolding;
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

        

        bool singleTap;
        public PlayerControl()
        {
            this.SizeChanged += PlayerControl_SizeChanged;
            this.InitializeComponent();
            this.mode = DisplayMode.Compact;

            touch3D.ActionRequested += touch3D_ActionRequested;
            touch3D.VisibilityChanged += Touch3D_VisibilityChanged;
            wasHolding = false;
            IsPlaylistLoaded = false;
            IsPlaylistOpened = false;
            PlaylistHasBeenUpdated = true;
            AllowSliderChange = true;
            Tick = new DispatcherTimer();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            Application.Current.Resuming += Current_Resuming;
            ApplicationSettings.BlurLevelChanged += ApplicationSettings_BlurLevelChanged;
            ApplicationSettings.NowPlayingThemeChanged += ApplicationSettings_NowPlayingThemeChanged;
            ApplicationSettings.CurrentThemeColorChanged += ApplicationSettings_CurrentThemeColorChanged;
            ApplicationSettings.ThemeBackgroundPreferenceChanged += ApplicationSettings_ThemeBackgroundPreferenceChanged;
            PlayerController.FullPlayerRequested += PlayerController_FullPlayerRequested;
            Ctr_Song.FavoritesChanged += Ctr_Song_FavoritesChanged;
        }

        private void Ctr_Song_FavoritesChanged(Song updatedSong)
        {
            if (updatedSong.SongURI == albumCover.CurrentSong.SongURI)
                albumCover.CurrentSong = ApplicationSettings.CurrentSong;
        }

        private void PlayerController_FullPlayerRequested(object sender, RoutedEventArgs e)
        {
            Mode = DisplayMode.Full;
        }

        private void ApplicationSettings_ThemeBackgroundPreferenceChanged()
        {
            if (ApplicationSettings.CurrentSong != null)
                UpdateBackground(ApplicationSettings.CurrentSong);
        }

        private void ApplicationSettings_CurrentThemeColorChanged()
        {
            UpdateThemeColor(ApplicationSettings.CurrentThemeColor);
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
            if (isVisible == false)
                HideTouch3D();
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

                HideTouch3D();

                NavigationHelper.Navigate(this, typeof(ArtistPage), artist);
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
            //MessageService.SendMessageToBackground(new AppStateMessage(AppState.Active));
            //ApplicationData.Current.LocalSettings.Values["AppState"] = AppState.Active.ToString();
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;

            BackgroundMediaPlayer.Current.Volume = 100;

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
                if (BackgroundMediaPlayer.Current != null)
                {
                    if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState != MediaPlaybackState.None)
                    {
                        MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.AskPlaylist));

                        if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                        {
                            Tick.Start();
                        }
                    }
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
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;

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

            Storyboard sb = new Storyboard();
            DoubleAnimation da;
            DoubleAnimation da1;
            if (this.Mode == DisplayMode.Compact)
            {
                fullView.IsHitTestVisible = false;
                acrylic.Opacity = 0;

                HidePlaylist();

                da = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut } 
                };

                da1 = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                };

                sb.Completed += (s, a) =>
                {
                    compactView.IsHitTestVisible = true;
                    //fullView.Visibility = Visibility.Collapsed;
                };
            }
            else
            {
                compactView.IsHitTestVisible = false;
                fullView.IsHitTestVisible = true;

                da = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                };

                da1 = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                };

                sb.Completed += (s, a) =>
                {
                    (this.Resources["fadeInBlur"] as Storyboard).Begin();
                };

                if (ApplicationSettings.ThemesUserAware == false)
                {
                    SimpleNotice notice = new SimpleNotice();
                    notice.Caption = ApplicationInfo.Current.Resources.GetString("ThemesMessageCaption");
                    fullView.Children.Add(notice);
                    Canvas.SetZIndex(notice, 5);
                    notice.Dismissed += (s) =>
                    {
                        NavigationHelper.Navigate(this, typeof(ThemeSelector));
                        ApplicationSettings.ThemesUserAware = true;
                    };
                    notice.Show(ApplicationInfo.Current.Resources.GetString("ThemesMessage"), ApplicationInfo.Current.Resources.GetString("SetUp/Text"));
                }
            }

            Storyboard.SetTarget(da, fullView);
            Storyboard.SetTargetProperty(da, "Opacity");

            sb.Children.Add(da);

            Storyboard.SetTarget(da1, compactView);
            Storyboard.SetTargetProperty(da1, "Opacity");

            sb.Children.Add(da1);

            sb.Begin();
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
                previousButton.IsEnabled = nextButton.IsEnabled = playlistButton.IsEnabled = repeatToggleButton.IsEnabled = shuffleButton.IsEnabled = nextSong.IsEnabled = false;
                PlayPauseButton.Content = "\uF5B0";
            }
            else
            {
                previousButton.IsEnabled = nextButton.IsEnabled = playlistButton.IsEnabled = repeatToggleButton.IsEnabled = shuffleButton.IsEnabled = nextSong.IsEnabled = true;

                if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    PlayPauseButton.Content = "\uF8AE";
                }
                else if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                    PlayPauseButton.Content = "\uF5B0";
                }
            }
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            PlaylistMessage playlistMessage;
            if (MessageService.TryParseMessage(e.Data, out playlistMessage))
            {
                await Dispatcher.RunIdleAsync(async (s) =>
                {
                    PlaylistHasBeenUpdated = true;
                    Debug.WriteLine("PLAYLISTMESSAGE...\nOK");

                    await LoadPlaylist(playlistMessage.Playlist);
                    if (playlistMessage.Playlist.Count < 2)
                        nextSong.Visibility = Visibility.Collapsed;
                    else
                    {
                        nextSong.Visibility = Visibility.Visible;
                    }
                    UpdateNextSong();
                });
            }

            CurrentTrackMessage currentTrackMessage;
            if (MessageService.TryParseMessage(e.Data, out currentTrackMessage))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    playlist.CurrentTrackIndex = currentTrackMessage.Index;
                    UpdatePlayerInfo();
                });
            }

            CurrentStateChangedMessage currentStateChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out currentStateChangedMessage))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    Debug.WriteLine("CURRENT STATE: " + currentStateChangedMessage.State.ToString());

                    UpdateButtons();

                    if (currentStateChangedMessage.State == MediaPlaybackState.None)
                    {
                        DisablePlayer();
                    }
                    else if (currentStateChangedMessage.State == MediaPlaybackState.Playing)
                    {
                        Tick.Start();
                    }
                    else
                    {
                        Tick.Stop();
                    }
                });
            }

            ActionMessage actionMessage;
            if (MessageService.TryParseMessage(e.Data, out actionMessage))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    if (actionMessage.Action == BackgroundAudioShared.Messages.Action.ClearPlayback)
                    {
                        DisablePlayer();
                    }
                });
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

            ApplicationSettings.CurrentThemeColor = Color.FromArgb(255, 77, 77, 77);
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

                    if (ApplicationSettings.ThemeBackgroundPreference == 0)
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

                    if (ApplicationSettings.ThemeColorPreference == 0)
                        ApplicationSettings.CurrentThemeColor = ImageHelper.GetColorFromHex(ApplicationSettings.CurrentSong.HexColor);
                    else if (ApplicationSettings.ThemeColorPreference == 1)
                        ApplicationSettings.CurrentThemeColor = ApplicationInfo.Current.CurrentSystemAccentColor;
                    else if (ApplicationSettings.ThemeColorPreference == 2)
                        ApplicationSettings.CurrentThemeColor = ApplicationSettings.CustomThemeColor;

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

            if (ApplicationSettings.NowPlayingTheme == ClassLibrary.Themes.Theme.Material)
            {
                //Color c = ImageHelper.GetColorFromHex("#FFDC572E");
                Color opposite = color.GetOppositeColor();
                Color strongest = color.ChangeColorBrightness(-0.3f);

                Storyboard sb = new Storyboard();
                ColorAnimation ca1 = new ColorAnimation()
                {
                    To = color,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut },
                };

                Storyboard.SetTarget(ca1, hexagonColorA);
                Storyboard.SetTargetProperty(ca1, "Color");

                ColorAnimation ca2 = new ColorAnimation()
                {
                    To = opposite,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut },
                };

                Storyboard.SetTarget(ca2, hexagonColorC);
                Storyboard.SetTargetProperty(ca2, "Color");

                ColorAnimation ca3 = new ColorAnimation()
                {
                    To = strongest,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut },
                };

                Storyboard.SetTarget(ca3, hexagonColorB);
                Storyboard.SetTargetProperty(ca3, "Color");

                sb.Children.Add(ca1);
                sb.Children.Add(ca2);
                sb.Children.Add(ca3);

                sb.Begin();
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
            Storyboard sb = new Storyboard();

            DoubleAnimation da = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EnableDependentAnimation = false,
            };

            //ColorAnimation ca = new ColorAnimation()
            //{
            //    To = Colors.Black,
            //    Duration = TimeSpan.FromMilliseconds(250),
            //    EnableDependentAnimation = false,
            //};

            Storyboard.SetTarget(da, background);
            Storyboard.SetTargetProperty(da, "Opacity");

            //Storyboard.SetTarget(ca, colorOverlayBrush);
            //Storyboard.SetTargetProperty(ca, "Color");

            sb.Children.Add(da);
            //sb.Children.Add(ca);

            sb.Completed += async (s, a) =>
            {
                if (song != null)
                {
                    await Task.Delay(50);

                    backgroundBitmapImage.UriSource = null;

                    if (ApplicationSettings.ThemeBackgroundPreference == 0)
                        backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + ApplicationSettings.CurrentSong.AlbumID + ".jpg");
                    else
                        backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Artists/artist_" + StringHelper.RemoveSpecialChar(song.Artist) + ".jpg");
                }
            };

            sb.Begin();
        }

        private async void UpdateBackground(Color color, string albumID)
        {
            Storyboard sb = new Storyboard();

            DoubleAnimation da = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EnableDependentAnimation = false,
            };

            //ColorAnimation ca = new ColorAnimation()
            //{
            //    To = Colors.Black,
            //    Duration = TimeSpan.FromMilliseconds(250),
            //    EnableDependentAnimation = false,
            //};

            Storyboard.SetTarget(da, background);
            Storyboard.SetTargetProperty(da, "Opacity");

            //Storyboard.SetTarget(ca, colorOverlayBrush);
            //Storyboard.SetTargetProperty(ca, "Color");

            sb.Children.Add(da);
            //sb.Children.Add(ca);

            sb.Completed += async (s, a) =>
            {
                await Task.Delay(50);
                backgroundBitmapImage.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + albumID + "_blur.jpg");
            };

            sb.Begin();



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
            switch (ApplicationSettings.NowPlayingTheme)
            {
                case ClassLibrary.Themes.Theme.Clean:

                    modernBG.Visibility = Visibility.Collapsed;
                    acrylic.Visibility = Visibility.Collapsed;
                    materialBG.Visibility = Visibility.Collapsed;

                    break;
                case ClassLibrary.Themes.Theme.Blur:
                    SetBlur();
                    break;
                case ClassLibrary.Themes.Theme.Modern:
                    materialBG.Visibility = Visibility.Collapsed;
                    SetModernStyle();
                    break;
                case ClassLibrary.Themes.Theme.Material:
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

            acrylic.AcrylicIntensity = ApplicationSettings.NowPlayingBlurAmount;
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
            Storyboard sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400),
                EnableDependentAnimation = false,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, albumCover);
            Storyboard.SetTargetProperty(da, "Opacity");

            sb.Children.Add(da);

            sb.Begin();

        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (wasHolding)
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.SkipToPrevious, "skip"));
            else
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.SkipToPrevious));

            wasHolding = false;
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

        private async void PlayPauseOrResume()
        {
            //if (BackgroundMediaPlayer.Current == null)
            if (ApplicationSettings.IsBackgroundAudioTaskSuspended == true)
                await ResetBackgroundTask();
            else
                InitializePlayer();

            if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState != MediaPlaybackState.Playing
    && BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState != MediaPlaybackState.Paused)
            {
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.Resume));
                

                return;
            }

            MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.PlayPause));

        }

        private async Task<bool> ResetBackgroundTask()
        {
            const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA  

            bool result = false;
            BackgroundMediaPlayer.Shutdown();
            try
            {
                InitializePlayer();
                result = true;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
                else
                {
                    throw;
                }
                result = false;
            }

            return result;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.SkipToNext));
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
            Storyboard sb = this.Resources["fadeInBackground"] as Storyboard;
            sb.Begin();
            //Storyboard sb = new Storyboard();
            //DoubleAnimation da = new DoubleAnimation()
            //{
            //    To = 0.8,
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    BeginTime = TimeSpan.FromMilliseconds(100),
            //    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut },
            //    EnableDependentAnimation = false,
            //};

            //Storyboard.SetTarget(da, background);
            //Storyboard.SetTargetProperty(da, "Opacity");

            //sb.Children.Add(da);
            //sb.Begin();
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
            #region open playlist animation
            Storyboard sb = new Storyboard();

            DoubleAnimation da = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EnableDependentAnimation = false,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, playlistTranslate);
            Storyboard.SetTargetProperty(da, "X");
            sb.Children.Add(da);


            //DoubleAnimation da1 = new DoubleAnimation()
            //{
            //    To = playlist.ActualWidth * -1,
            //    Duration = TimeSpan.FromMilliseconds(250),
            //    EnableDependentAnimation = false,
            //    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            //};

            //Storyboard.SetTarget(da1, PlayerControlsContainerTranslate);
            //Storyboard.SetTargetProperty(da1, "X");
            //sb.Children.Add(da1);

            sb.Completed += (s, a) =>
            {
                if (IsPlaylistLoaded)
                    playlist.ScrollToSelectedIndex();
            };
            sb.Begin();

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
            MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.Shuffle));
        }

        private void dismissArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            HidePlaylist();
        }

        public void HidePlaylist()
        {
            dismissArea.IsHitTestVisible = false;
            IsPlaylistOpened = false;

            Storyboard sb = new Storyboard();

            DoubleAnimation da = new DoubleAnimation()
            {
                To = playlist.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(250),
                EnableDependentAnimation = false,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, playlistTranslate);
            Storyboard.SetTargetProperty(da, "X");
            sb.Children.Add(da);


            //DoubleAnimation da1 = new DoubleAnimation()
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(250),
            //    EnableDependentAnimation = false,
            //    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            //};

            //Storyboard.SetTarget(da1, PlayerControlsContainerTranslate);
            //Storyboard.SetTargetProperty(da1, "X");
            //sb.Children.Add(da1);
            sb.Begin();
        }

        private void closePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            HidePlaylist();
        }

        private void albumCover_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                Point point = e.GetPosition(this);
                ShowTouch3D(point);
            }
        }

        private void ShowTouch3D(Point point)
        {

            albumCover.ManipulationMode = ManipulationModes.None;
            albumCover.ManipulationDelta -= player_ManipulationDelta;
            albumCover.ManipulationCompleted -= player_ManipulationCompleted;

            //if ((point.X + 15) + 200 > ApplicationInfo.Current.AppArea.Width)
            //{
                //double d = point.X + 15 + 200 - ApplicationInfo.Current.AppArea.Width;
                //touch3D.IconsPosition = new Thickness(point.X + 15 - d, point.Y - 100, 0, 0);
                double d = this.ActualWidth / 2 - 100;
                touch3D.IconsPosition = new Thickness(d, point.Y - 100, 0, 0);
            //}
            //else
            //    touch3D.IconsPosition = new Thickness(point.X + 15, point.Y - 100, 0, 0);

            touch3D.Set(Touch3D.Mode.NowPlaying, Ctr_Song.Current.GetSong(new Song() { SongURI = CurrentTrackPath }));



            touch3D.Show();

            /*Storyboard sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation()
            {
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = false,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, touch3DOverlay);
            Storyboard.SetTargetProperty(da, "Opacity");

            sb.Children.Add(da);
            sb.Begin();

            touch3DOverlay.IsHitTestVisible = true;*/
        }

        private void albumCover_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (touch3D.IsTouch3DOpened)
                touch3D.Hide();
        }

        public void HideTouch3D()
        {
            albumCover.ManipulationMode = ManipulationModes.All;
            albumCover.ManipulationDelta += player_ManipulationDelta;
            albumCover.ManipulationCompleted += player_ManipulationCompleted;
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

            NavigationHelper.Navigate(this, typeof(AlbumPage), album);

        }

        private void player_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this.IsManipulating = false;
            if (touch3D.IsTouch3DOpened)
                return;

            if (e.Cumulative.Translation.Y >= 200)
            {
                this.Mode = DisplayMode.Compact;
            }
            else if (e.Cumulative.Translation.X < -100)
            {
                ShowPlaylist();
            }
            else if (e.Cumulative.Translation.X >= -100)
            {
                HidePlaylist();
            }
        }

        private void player_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            this.IsManipulating = true;

            if (e.IsInertial || touch3D.IsTouch3DOpened)
            {
                e.Complete();
            }

            if (e.Cumulative.Translation.X > playlist.ActualWidth * -1)
            {
                playlistTranslate.X += e.Delta.Translation.X;
                //PlayerControlsContainerTranslate.X += e.Delta.Translation.X;
            }
        }

        private void touch3DOverlay_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            HideTouch3D();
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

            //if (ApplicationSettings.CurrentSong.IsFavorite)
            //    loveIndicator.Text = "";
            //else
            //    loveIndicator.Text = "";

            //Storyboard sb = new Storyboard();
            //DoubleAnimation da = new DoubleAnimation()
            //{
            //    From = 0.8,
            //    To = 1,
            //    Duration = TimeSpan.FromMilliseconds(340),
            //    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            //};

            //Storyboard.SetTarget(da, loveIndicatorScale);
            //Storyboard.SetTargetProperty(da, "ScaleX");

            //DoubleAnimation da1 = new DoubleAnimation()
            //{
            //    From = 0.8,
            //    To = 1,
            //    Duration = TimeSpan.FromMilliseconds(340),
            //    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            //};

            //Storyboard.SetTarget(da1, loveIndicatorScale);
            //Storyboard.SetTargetProperty(da1, "ScaleY");

            //DoubleAnimation da2 = new DoubleAnimation()
            //{
            //    From = 0,
            //    To = 0.8,
            //    Duration = TimeSpan.FromMilliseconds(220),
            //    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            //};

            //Storyboard.SetTarget(da2, loveIndicator);
            //Storyboard.SetTargetProperty(da2, "Opacity");

            //sb.Children.Add(da);
            //sb.Children.Add(da1);
            //sb.Children.Add(da2);

            //sb.Completed += (s, a) =>
            //{
            //    Storyboard ssb = new Storyboard();
            //    DoubleAnimation sda = new DoubleAnimation()
            //    {
            //        To = 0,
            //        Duration = TimeSpan.FromMilliseconds(200),
            //        EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            //    };

            //    Storyboard.SetTarget(sda, loveIndicator);
            //    Storyboard.SetTargetProperty(sda, "Opacity");

            //    ssb.Children.Add(sda);
            //    ssb.Begin();
            //};

            //sb.Begin();
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
                wasHolding = true;
            }
            else
            {
                TooltipDismissed?.Invoke(this, new RoutedEventArgs());
                wasHolding = false;
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
            NavigationHelper.Navigate(this, typeof(ArtistPage), art);
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
                NavigationHelper.Navigate(this, typeof(ThemeSelector));
            };

            mf.Items.Add(mfi);

            MenuFlyoutItem mfi2 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("TimerMenu"),
            };

            mfi2.Click += (s,a) =>
            {
                NavigationHelper.Navigate(this, typeof(Settings), "path=timer");
            };

            mf.Items.Add(mfi2);

            MenuFlyoutItem mfi3 = new MenuFlyoutItem()
            {
                Text = ApplicationInfo.Current.Resources.GetString("ScrobbleSettings"),
            };

            mfi3.Click += (s, a) =>
            {
                NavigationHelper.Navigate(this, typeof(Settings), "path=scrobble");
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
            Storyboard storyboard = new Storyboard();

            //DoubleAnimation da1 = new DoubleAnimation()
            //{
            //    To = 0.1,
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da1, compactPlayerHoverIndicator);
            //Storyboard.SetTargetProperty(da1, "Opacity");

            //storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, compactPlayerExpandIndicator);
            Storyboard.SetTargetProperty(da2, "Opacity");

            storyboard.Children.Add(da2);

            //DoubleAnimation da3 = new DoubleAnimation()
            //{
            //    To = negative,
            //    Duration = TimeSpan.FromMilliseconds(200),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da3, iconTranslate);
            //Storyboard.SetTargetProperty(da3, "Y");

            //storyboard.Children.Add(da3);

            //DoubleAnimation da4 = new DoubleAnimation()
            //{
            //    To = positive,
            //    Duration = TimeSpan.FromMilliseconds(200),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da4, textTranslate);
            //Storyboard.SetTargetProperty(da4, "Y");

            //storyboard.Children.Add(da4);

            //DoubleAnimation da5 = new DoubleAnimation()
            //{
            //    To = 0.6,
            //    Duration = TimeSpan.FromMilliseconds(100),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da5, contentPresenter);
            //Storyboard.SetTargetProperty(da5, "Opacity");

            //storyboard.Children.Add(da5);

            storyboard.Begin();

        }

        private void BeginNormalAnimation()
        {
            Storyboard storyboard = new Storyboard();

            //DoubleAnimation da1 = new DoubleAnimation()
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da1, compactPlayerHoverIndicator);
            //Storyboard.SetTargetProperty(da1, "Opacity");

            //storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, compactPlayerExpandIndicator);
            Storyboard.SetTargetProperty(da2, "Opacity");

            storyboard.Children.Add(da2);

            //DoubleAnimation da3 = new DoubleAnimation()
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(200),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da3, iconTranslate);
            //Storyboard.SetTargetProperty(da3, "Y");

            //storyboard.Children.Add(da3);

            //DoubleAnimation da4 = new DoubleAnimation()
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(200),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da4, textTranslate);
            //Storyboard.SetTargetProperty(da4, "Y");

            //storyboard.Children.Add(da4);

            //DoubleAnimation da5 = new DoubleAnimation()
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(100),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da5, contentPresenter);
            //Storyboard.SetTargetProperty(da5, "Opacity");

            //storyboard.Children.Add(da5);


            storyboard.Begin();
        }

        private void BeginPressedAnimation()
        {
            Storyboard storyboard = new Storyboard();

            //DoubleAnimation da1 = new DoubleAnimation()
            //{
            //    To = 0.2,
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da1, compactPlayerHoverIndicator);
            //Storyboard.SetTargetProperty(da1, "Opacity");

            //storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, compactPlayerExpandIndicator);
            Storyboard.SetTargetProperty(da2, "Opacity");

            storyboard.Children.Add(da2);

            //DoubleAnimation da3 = new DoubleAnimation()
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(200),
            //    EnableDependentAnimation = true,
            //};

            //Storyboard.SetTarget(da3, Background);
            //Storyboard.SetTargetProperty(da3, "Opacity");

            //storyboard.Children.Add(da3);

            storyboard.Begin();

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

            NavigationHelper.Navigate(this, typeof(ArtistPage), artist);
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

            NavigationHelper.Navigate(this, typeof(AlbumPage), album);
        }

        private void AlbumCover_FavoriteStateToggled(object sender, RoutedEventArgs e)
        {
            LikeDislikeSong();
        }

        private void NextSong_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
