using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class LastFmLoginControl : UserControl
    {
        private Compositor _compositor;
        private SpriteVisual blurSprite;
        private CompositionEffectBrush _brush;

        public LastFmLoginControl()
        {
            this.Loaded += LastFmLoginControl_Loaded;
            this.SizeChanged += LastFmLoginControl_SizeChanged;
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            this.InitializeComponent();
        }

        private void LastFmLoginControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (blurSprite != null)
            {
                blurSprite.Size = e.NewSize.ToVector2();
            }
        }

        private void LastFmLoginControl_Loaded(object sender, RoutedEventArgs e)
        {
            BlendEffectMode blendmode = BlendEffectMode.Overlay;

            // Create a chained effect graph using a BlendEffect, blending color and blur
            var graphicsEffect = new BlendEffect
            {
                Mode = blendmode,
                Background = new ColorSourceEffect()
                {
                    Name = "Tint",
                    Color = Colors.Transparent,
                },

                Foreground = new GaussianBlurEffect()
                {
                    Name = "Blur",
                    Source = new CompositionEffectSourceParameter("Backdrop"),
                    BlurAmount = 4.0f,
                    BorderMode = EffectBorderMode.Hard,
                }
            };

            var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                new[] { "Blur.BlurAmount", "Tint.Color" });

            // Create EffectBrush, BackdropBrush and SpriteVisual
            _brush = blurEffectFactory.CreateBrush();

            var destinationBrush = _compositor.CreateBackdropBrush();
            _brush.SetSourceParameter("Backdrop", destinationBrush);

            blurSprite = _compositor.CreateSpriteVisual();
            blurSprite.Size = new Vector2((float)this.ActualWidth, (float)ApplicationInfo.Current.WindowSize.Height);
            blurSprite.Brush = _brush;

            ElementCompositionPreview.SetElementChildVisual(blur, blurSprite);
        }


        private void blur_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (PageHelper.MainPage != null)
            {
                PageHelper.MainPage.RemovePicker();
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private async void Login()
        {
            bool result = await LastFm.Current.Login(username.Text, password.Password);

            if (result)
            {
                PageHelper.MainPage.RemovePicker();
            }
            else
            {
                loginError.Visibility = Visibility.Visible;
            }
        }

        private async void signUpButon_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationInfo.Current.Language == "EN")
                await Launcher.LaunchUriAsync(new Uri("https://www.last.fm/join", UriKind.Absolute));
            else
                await Launcher.LaunchUriAsync(new Uri("https://www.last.fm/" + ApplicationInfo.Current.Language.ToLower() + "/join", UriKind.Absolute));
        }

        private void textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                Login();
        }
    }
}
