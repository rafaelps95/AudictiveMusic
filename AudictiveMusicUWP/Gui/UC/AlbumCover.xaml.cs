using ClassLibrary.Entities;
using RPSToolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class AlbumCover : UserControl
    {
        public event RoutedEventHandler ImageFailed;
        public event RoutedEventHandler ImageOpened;
        public event RoutedEventHandler FavoriteStateToggled;
        public event RoutedEventHandler AddRequested;
        public event RoutedEventHandler ArtistRequested;
        public event RoutedEventHandler ActionBarInitiated;
        public event RoutedEventHandler ActionBarClosed;
        public event RoutedEventHandler Click;





        public Song CurrentSong
        {
            get { return (Song)GetValue(CurrentSongProperty); }
            set { SetValue(CurrentSongProperty, value);
                if (value != null)
                    UriSource = new Uri("ms-appdata:///local/Covers/cover_" + value.AlbumID + ".jpg", UriKind.Absolute);
                else
                    bitmap.UriSource = this.FallbackUri;
            }
        }

        // Using a DependencyProperty as the backing store for CurrentSong.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentSongProperty =
            DependencyProperty.Register("CurrentSong", typeof(Song), typeof(AlbumCover), new PropertyMetadata(new Song()));




        public Uri FallbackUri
        {
            get { return (Uri)GetValue(FallbackUriProperty); }
            set { SetValue(FallbackUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FallbackUriProperty =
            DependencyProperty.Register("FallbackUri", typeof(Uri), typeof(AlbumCover), new PropertyMetadata(null));



        public double CoverSize
        {
            get { return (double)GetValue(CoverSizeProperty); }
            set { SetValue(CoverSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CoverSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoverSizeProperty =
            DependencyProperty.Register("CoverSize", typeof(double), typeof(AlbumCover), new PropertyMetadata(150));




        private Uri UriSource
        {
            get { return (Uri)GetValue(UriSourceProperty); }
            set
            {
                SetValue(UriSourceProperty, value);
                bitmap.UriSource = value;
            }
        }

        // Using a DependencyProperty as the backing store for UriSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.Register("UriSource", typeof(Uri), typeof(AlbumCover), new PropertyMetadata(null));


        private bool singleTap;
        private bool _isOpen = false;
        private bool _isTouchInteraction = false;

        public AlbumCover()
        {
            this.Loaded += AlbumCover_Loaded;
            this.InitializeComponent();
        }

        private void AlbumCover_Loaded(object sender, RoutedEventArgs e)
        {
            shadow.ApplyShadow(new Vector3(0,10,10));
        }

        private void albumCover_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                _isTouchInteraction = false;
                OpenActionBar();
            }
        }

        private void OpenActionBar()
        {
            _isOpen = true;
            ActionBarInitiated?.Invoke(this, new RoutedEventArgs());

            Storyboard sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150),
                EnableDependentAnimation = true,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, actionBar);
            Storyboard.SetTargetProperty(da, "Opacity");

            sb.Children.Add(da);


            DoubleAnimation da1 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EnableDependentAnimation = true,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da1, actionBarTranslate);
            Storyboard.SetTargetProperty(da1, "Y");

            sb.Children.Add(da1);


            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150),
                EnableDependentAnimation = true,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da2, shadow);
            Storyboard.SetTargetProperty(da2, "Opacity");

            sb.Children.Add(da2);

            sb.Completed += (s, a) => { actionBar.IsHitTestVisible = true; };

            sb.Begin();
        }

        private void albumCover_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                _isTouchInteraction = true;
                Point point = e.GetPosition(this);

                OpenActionBar();

                
            }
        }

        private void albumCover_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.singleTap = false;

            if (CurrentSong.IsFavorite)
                loveIndicator.Text = "";
            else
                loveIndicator.Text = "";

            Storyboard sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation()
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(340),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, loveIndicatorScale);
            Storyboard.SetTargetProperty(da, "ScaleX");

            DoubleAnimation da1 = new DoubleAnimation()
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(340),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da1, loveIndicatorScale);
            Storyboard.SetTargetProperty(da1, "ScaleY");

            DoubleAnimation da2 = new DoubleAnimation()
            {
                From = 0,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da2, loveIndicator);
            Storyboard.SetTargetProperty(da2, "Opacity");

            sb.Children.Add(da);
            sb.Children.Add(da1);
            sb.Children.Add(da2);

            sb.Completed += (s, a) =>
            {
                Storyboard ssb = new Storyboard();
                DoubleAnimation sda = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(sda, loveIndicator);
                Storyboard.SetTargetProperty(sda, "Opacity");

                ssb.Children.Add(sda);
                ssb.Begin();
            };

            sb.Begin();

            FavoriteStateToggled?.Invoke(this, new RoutedEventArgs());
        }

        private async void albumCover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.singleTap = true;
            await Task.Delay(200);
            if (this.singleTap == false)
                return;

            Click?.Invoke(this, new RoutedEventArgs());
        }

        private void Bitmap_ImageOpened(object sender, RoutedEventArgs e)
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

            ImageOpened?.Invoke(sender, e);
        }

        private void Bitmap_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageFailed?.Invoke(sender, new RoutedEventArgs());
            try
            {
                bitmap.UriSource = this.FallbackUri;
            }
            catch (Exception ex)
            {

            }
        }

        private void CloseActionBar()
        {
            _isOpen = false;
            ActionBarClosed?.Invoke(this, new RoutedEventArgs());
            actionBar.IsHitTestVisible = false;

            Storyboard sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EnableDependentAnimation = true,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da, actionBar);
            Storyboard.SetTargetProperty(da, "Opacity");

            sb.Children.Add(da);


            DoubleAnimation da1 = new DoubleAnimation()
            {
                To = 20,
                Duration = TimeSpan.FromMilliseconds(150),
                EnableDependentAnimation = true,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da1, actionBarTranslate);
            Storyboard.SetTargetProperty(da1, "Y");

            sb.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EnableDependentAnimation = true,
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(da2, shadow);
            Storyboard.SetTargetProperty(da2, "Opacity");

            sb.Children.Add(da2);

            sb.Begin();
        }

        private async void AlbumGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                CloseActionBar();
            else
            {
                if (_isOpen && _isTouchInteraction)
                    await Task.Delay(1500);

                if (_isOpen && _isTouchInteraction)
                {
                    CloseActionBar();
                }
            }
        }

        private async void AlbumGrid_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                CloseActionBar();
            else
            {
                if (_isOpen && _isTouchInteraction)
                    await Task.Delay(4000);

                if (_isOpen && _isTouchInteraction)
                {
                    CloseActionBar();
                }
            }
        }

        private async void AlbumGrid_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                CloseActionBar();
            else
            {
                if (_isOpen && _isTouchInteraction)
                    await Task.Delay(4000);

                if (_isOpen && _isTouchInteraction)
                {
                    CloseActionBar();
                }
            }
        }

        private void NavigationBarItem_Click(object sender, RoutedEventArgs e)
        {
            ArtistRequested?.Invoke(this, new RoutedEventArgs());
        }

        private void NavigationBarItem_Click_1(object sender, RoutedEventArgs e)
        {
            AddRequested?.Invoke(this, new RoutedEventArgs());
        }

        private void NavigationBarItem_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ArtistRequested?.Invoke(this, new RoutedEventArgs());
        }

        private void NavigationBarItem_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            AddRequested?.Invoke(this, new RoutedEventArgs());
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            FavoriteStateToggled?.Invoke(this, new RoutedEventArgs());
        }
    }
}
