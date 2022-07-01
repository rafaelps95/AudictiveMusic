using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class NextTooltip : UserControl
    {
        public event RoutedEventHandler Click;

        private string Status
        {
            get { return ((string)GetValue(StatusProperty)); }
            set
            {
                SetValue(StatusProperty, value);
            }
        }

        private static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(NextTooltip), new PropertyMetadata(string.Empty));

        private string Title
        {
            get { return ((string)GetValue(TitleProperty)); }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        private static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(NextTooltip), new PropertyMetadata(string.Empty));

        private string Subtitle
        {
            get { return ((string)GetValue(SubtitleProperty)); }
            set
            {
                SetValue(SubtitleProperty, value);
            }
        }

        private static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(NextTooltip), new PropertyMetadata(string.Empty));


        private ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(NextTooltip), null);


        public Song Song
        {
            get; private set;
        }

        public void SetSong(Song song)
        {
            this.Song = song;

            this.Title = song.Name;
            this.Subtitle = song.Artist;
            this.Source = song.Image;

            if (this.ShowAcrylicBackground == false)
            {
                this.Width = double.NaN;
                this.MaxWidth = 240;
                button.Visibility = Visibility.Visible;
            }
            else
            {
                this.AccentColor = ImageHelper.GetColorFromHex(song.HexColor);
                button.Visibility = Visibility.Collapsed;
            }
        }


        public ImageSource FallbackSource
        {
            get { return (ImageSource)GetValue(FallbackSourceProperty); }
            set { SetValue(FallbackSourceProperty, value); }
        }

        public static readonly DependencyProperty FallbackSourceProperty =
            DependencyProperty.Register("FallbackSource", typeof(ImageSource), typeof(NextTooltip), new PropertyMetadata(new BitmapImage(new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute))));

        private Color AccentColor
        {
            get { return ((Color)GetValue(AccentColorProperty)); }
            set
            {
                SetValue(AccentColorProperty, value);

                if (value.IsDarkColor())
                {
                    this.RequestedTheme = ElementTheme.Dark;
                    //MusicProgress.Foreground = new SolidColorBrush(lighterColor);
                }
                else
                {
                    this.RequestedTheme = ElementTheme.Light;
                    //MusicProgress.Foreground = new SolidColorBrush(darkerColor);
                }
            }
        }

        private static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register("AccentColor", typeof(Color), typeof(NextTooltip), new PropertyMetadata(Colors.Transparent));



        public bool ShowAcrylicBackground
        {
            get { return (bool)GetValue(ShowAcrylicBackgroundProperty); }
            set { SetValue(ShowAcrylicBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAcrylicBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAcrylicBackgroundProperty =
            DependencyProperty.Register("ShowAcrylicBackground", typeof(bool), typeof(NextTooltip), new PropertyMetadata(true));



        public NextTooltip()
        {
            this.Loaded += NextTooltip_Loaded;
            this.InitializeComponent();

            ThemeSettings.TransparencyEffectToggled += ApplicationSettings_TransparencyEffectToggled;
            ThemeSettings.PerformanceModeToggled += ApplicationSettings_PerformanceModeToggled;
        }

        private void NextTooltip_Loaded(object sender, RoutedEventArgs e)
        {
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
            acrylic.AcrylicEnabled = ThemeSettings.IsTransparencyEnabled;
        }

        private void image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (FallbackSource != null)
            {
                fallbackBrush.ImageSource = FallbackSource;
            }
        }

        public enum Mode
        {
            Previous,
            Next
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Click?.Invoke(this, new RoutedEventArgs());
        }
    }
}
