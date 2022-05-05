using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public enum BlurType
    {
        Window,
        App
    }

    public sealed partial class Acrylic : UserControl
    {
        #region PROPERTIES

        public BlurType AcrylicMode
        {
            get { return (BlurType)GetValue(AcrylicModeProperty); }
            set { SetValue(AcrylicModeProperty, value);
                SetAcrylic();
            }
        }

        // Using a DependencyProperty as the backing store for AcrylicMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcrylicModeProperty =
            DependencyProperty.Register("AcrylicMode", typeof(BlurType), typeof(Acrylic), new PropertyMetadata(BlurType.Window));



        public double BlurIntensity
        {
            get { return (double)GetValue(BlurIntensityProperty); }
            set { SetValue(BlurIntensityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlurIntensity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlurIntensityProperty =
            DependencyProperty.Register("BlurIntensity", typeof(double), typeof(Acrylic), new PropertyMetadata(18.0));



        public double Contrast
        {
            get { return (double)GetValue(ContrastProperty); }
            set { SetValue(ContrastProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Contrast.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContrastProperty =
            DependencyProperty.Register("Contrast", typeof(double), typeof(Acrylic), new PropertyMetadata(0));



        public Color Tint
        {
            get { return ((Color)GetValue(TintProperty)); }
            set
            {
                SetValue(TintProperty, value);
            }
        }

        public static readonly DependencyProperty TintProperty =
            DependencyProperty.Register("Tint", typeof(Color), typeof(Acrylic), new PropertyMetadata(Colors.Transparent));

        public double TintOpacity
        {
            get { return ((double)GetValue(TintOpacityProperty)); }
            set
            {
                SetValue(TintOpacityProperty, value);
                if (this.IsBlurEnabled)
                    tintColor.Opacity = value;
                else
                    tintColor.Opacity = 1;
            }
        }

        public static readonly DependencyProperty TintOpacityProperty =
            DependencyProperty.Register("TintOpacity", typeof(double), typeof(Acrylic), new PropertyMetadata(0.6));


        public bool IsBlurEnabled
        {
            get { return ((bool)GetValue(IsBlurEnabledProperty)); }
            set
            {
                SetValue(IsBlurEnabledProperty, value);
                SetAcrylic();
            }
        }

        public static readonly DependencyProperty IsBlurEnabledProperty =
            DependencyProperty.Register("IsBlurEnabled", typeof(bool), typeof(Acrylic), new PropertyMetadata(true));

        public double BlurOpacity
        {
            get { return (double)GetValue(BlurOpacityProperty); }
            set { SetValue(BlurOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlurOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlurOpacityProperty =
            DependencyProperty.Register("BlurOpacity", typeof(double), typeof(Acrylic), new PropertyMetadata(0.6));

        #endregion


        private Compositor _compositor;
        private SpriteVisual _backgroundSprite;
        private CompositionEffectBrush _brush;

        public Acrylic()
        {
            this.Loaded += Acrylic_Loaded;
            this.SizeChanged += Acrylic_SizeChanged;
            this.InitializeComponent();
        }

        private void Acrylic_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_backgroundSprite != null)
                _backgroundSprite.Size = e.NewSize.ToVector2();
        }

        private void Acrylic_Loaded(object sender, RoutedEventArgs e)
        {
            SetAcrylic();
        }

        private void SetAcrylic()
        {
            if (this.IsBlurEnabled)
            {
                _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

                if (this.AcrylicMode == BlurType.Window)
                {
                    if (ApplicationInfo.Current.GetDeviceFormFactorType() != ApplicationInfo.DeviceFormFactorType.Phone
                        && ApplicationInfo.Current.IsMobile == false
                        && ApplicationInfo.Current.IsTabletModeEnabled == false)
                    {
                        tintColor.Opacity = this.TintOpacity;
                        blurGrid.Visibility = Visibility.Visible;
                        _backgroundSprite = _compositor.CreateSpriteVisual();

                        _backgroundSprite.Size = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);

                        _backgroundSprite.Brush = _compositor.CreateHostBackdropBrush();

                        ElementCompositionPreview.SetElementChildVisual(blurGrid, _backgroundSprite);
                    }
                    else
                    {

                        // HOW SHOULD THE SURFACE LOOK IF THE APP IS RUNNING ON A PHONE??

                    }
                }
                else
                {
                    _backgroundSprite = _compositor.CreateSpriteVisual();
                    blurGrid.Visibility = Visibility.Visible;
                    tintColor.Opacity = this.TintOpacity;

                    BlendEffectMode blendmode = BlendEffectMode.Overlay;

                    var graphicsEffect = new BlendEffect
                    {
                        Mode = blendmode,
                        Background = new ColorSourceEffect()
                        {
                            Name = "BlurTint",
                            Color = Colors.Transparent,
                        },

                        Foreground = new GaussianBlurEffect()
                        {
                            Name = "Blur",
                            Source = new CompositionEffectSourceParameter("Backdrop"),
                            BlurAmount = (float)this.BlurIntensity,
                            BorderMode = EffectBorderMode.Hard,
                        }
                    };

                    var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                        new[] { "Blur.BlurAmount", "BlurTint.Color" });

                    _brush = blurEffectFactory.CreateBrush();

                    var destinationBrush = _compositor.CreateBackdropBrush();
                    _brush.SetSourceParameter("Backdrop", destinationBrush);

                    _backgroundSprite.Size = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);
                    _backgroundSprite.Brush = _brush;

                    ElementCompositionPreview.SetElementChildVisual(blurGrid, _backgroundSprite);
                }
            }
            else
            {
                blurGrid.Visibility = Visibility.Collapsed;
                tintColor.Opacity = 1;
                _compositor = null;
                _brush = null;
                _backgroundSprite = null;
            }
        }
    }
}
