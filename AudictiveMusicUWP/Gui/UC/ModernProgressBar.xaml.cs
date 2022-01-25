using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class ModernProgressBar : UserControl
    {
        private Storyboard sb;

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value);
                if (value)
                    BeginAnimating();
                else
                    StopAnimating();
            }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ModernProgressBar), new PropertyMetadata(false));



        public ModernProgressBar()
        {
            this.InitializeComponent();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            smallRectangleTranslate.X = smallRectangle.ActualWidth * -1;
            bigRectangleTranslate.X = bigRectangle.ActualWidth * -1;

            if (IsActive)
                BeginAnimating();
            else
                StopAnimating();
        }

        private double SmallBarAnimationTime()
        {
            return (double)0;
        }

        private double DelayTime()
        {
            return (double)0;
        }

        private void BeginAnimating()
        {
            //border.Background = new SolidColorBrush(Colors.Green);

            sb = new Storyboard();
            sb.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation da = new DoubleAnimation()
            {
                From = -200,
                To = this.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(1200),
                BeginTime = TimeSpan.FromMilliseconds(1000),
                EnableDependentAnimation = true,
                EasingFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseInOut,
                },
            };

            Storyboard.SetTarget(da, bigRectangleTranslate);
            Storyboard.SetTargetProperty(da, "TranslateTransform.X");

            DoubleAnimation da1 = new DoubleAnimation()
            {
                From = -100,
                To = this.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(1500),
                EnableDependentAnimation = true,
                EasingFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseInOut,
                },
            };

            Storyboard.SetTarget(da1, smallRectangleTranslate);
            Storyboard.SetTargetProperty(da1, "TranslateTransform.X");

            sb.Children.Add(da);
            sb.Children.Add(da1);
            sb.Begin();

        }

        private void StopAnimating()
        {
            //border.Background = new SolidColorBrush(Colors.Red);

            if (sb != null)
                sb.Stop();

            smallRectangleTranslate.X = smallRectangle.ActualWidth * -1;
            bigRectangleTranslate.X = bigRectangle.ActualWidth * -1;
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsActive == false)
                return;

            StopAnimating();
            smallRectangleTranslate.X = smallRectangle.ActualWidth * -1;
            bigRectangleTranslate.X = bigRectangle.ActualWidth * -1;
            BeginAnimating();
        }
    }
}
