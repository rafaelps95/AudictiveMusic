using AudictiveMusicUWP.Gui.Util;
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
    public sealed partial class NavigationBarItem : UserControl
    {
        public event RoutedEventHandler Click;
        //public event RoutedEventHandler Checked;
        //public event RoutedEventHandler Unchecked;
        private object clickOriginalSource;


        public NavigationBarItem()
        {
            this.Loaded += NavigationBarItem_Loaded;
            this.InitializeComponent();
        }

        private void NavigationBarItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();

            SetLayout(this.Orientation);

            if (this.IsChecked)
                BeginCheckedAnimation();
        }



        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value);
                SetLayout(value);
            }
        }

        private void SetLayout(Orientation value)
        {
            if (value == Orientation.Vertical)
            {
                contentGrid.Margin = new Thickness(0, 13, 0, 13);
                contentPresenter.TextWrapping = TextWrapping.WrapWholeWords;
                contentPresenter.MaxLines = 2;
                //checkMark.HorizontalAlignment = HorizontalAlignment.Left;
                //checkMark.VerticalAlignment = VerticalAlignment.Stretch;
                //checkMark.Width = 2;
                //checkMark.Height = double.NaN;
            }
            else
            {
                contentGrid.Margin = new Thickness(13, 0, 13, 0);
                contentPresenter.TextWrapping = TextWrapping.NoWrap;
                contentPresenter.MaxLines = 1;
                //checkMark.HorizontalAlignment = HorizontalAlignment.Stretch;
                //checkMark.VerticalAlignment = VerticalAlignment.Bottom;
                //checkMark.Height = 2;
                //checkMark.Width = double.NaN;
            }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(NavigationBarItem), new PropertyMetadata(Orientation.Horizontal));




        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value);
                SetCheckState(value);
            }
        }

        private void SetCheckState(bool value)
        {
            if (value)
                BeginCheckedAnimation();
            else
                BeginNormalAnimation();
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(NavigationBarItem), new PropertyMetadata(false));



        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(NavigationBarItem), new PropertyMetadata(""));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NavigationBarItem), new PropertyMetadata(""));

        private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            clickOriginalSource = e.OriginalSource;
            BeginPressedAnimation();
        }

        private void RootGrid_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (this.IsChecked)
                BeginCheckedAnimation();
            else
                BeginNormalAnimation();
        }

        private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BeginPointerOverAnimation();
        }

        private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (this.IsChecked)
                BeginCheckedAnimation();
            else
                BeginNormalAnimation();
        }

        private void RootGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Click?.Invoke(this, new RoutedEventArgs());
        }

        private void RootGrid_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (this.IsChecked)
                BeginCheckedAnimation();
            else
                BeginNormalAnimation();
        }

        private void BeginCheckedAnimation()
        {
            double positive = GetTranslateOffset()[0];
            double negative = GetTranslateOffset()[1];

            Animation animation = new Animation();
            animation.AddDoubleAnimation(0.2, 300, Background, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(negative, 200, iconTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(positive, 200, textTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(1, 100, contentPresenter, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }


        private void BeginPointerOverAnimation()
        {
            double positive = GetTranslateOffset()[0];
            double negative = GetTranslateOffset()[1];

            Animation animation = new Animation();
            animation.AddDoubleAnimation(negative, 200, iconTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(positive, 200, textTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(1, 100, contentPresenter, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void BeginNormalAnimation()
        {
            icon.Opacity = 1;

            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 300, Background, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0, 200, iconTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0, 200, textTranslate, "Y", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));
            animation.AddDoubleAnimation(0, 100, contentPresenter, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void BeginPressedAnimation()
        {

        }

        private double[] GetTranslateOffset()
        {
            double d = contentPresenter.ActualHeight / 2;
            double nd = d * -1;

            return new double[]{ d, nd };
        }

        private void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();

            if (this.IsChecked)
                BeginCheckedAnimation();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
    }
}
