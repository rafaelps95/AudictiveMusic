using AudictiveMusicUWP.Gui.Util;
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

            Animation animation = new Animation();
            animation.AddDoubleAnimation(1, 150, actionBar, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0, 150, actionBarTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(1, 150, shadow, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Completed += (s, a) => { actionBar.IsHitTestVisible = true; };

            animation.Begin();
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

            Animation animation = new Animation();
            animation.AddDoubleAnimation(0.8, 1, 340, loveIndicatorScale, "ScaleX", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0.8, 1, 340, loveIndicatorScale, "ScaleY", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0, 0.8, 220, loveIndicator, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Completed += (s, a) =>
            {
                Animation animation2 = new Animation();
                animation2.AddDoubleAnimation(0, 200, loveIndicator, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

                animation2.Begin();
            };

            animation.Begin();

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
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 1, 400, albumCover, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();

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


            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 150, actionBar, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(20, 150, actionBarTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0, 150, shadow, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
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
