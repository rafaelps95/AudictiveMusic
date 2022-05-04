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
                checkMark.HorizontalAlignment = HorizontalAlignment.Left;
                checkMark.VerticalAlignment = VerticalAlignment.Stretch;
                checkMark.Width = 2;
                checkMark.Height = double.NaN;
            }
            else
            {
                contentGrid.Margin = new Thickness(13, 0, 13, 0);
                contentPresenter.TextWrapping = TextWrapping.NoWrap;
                contentPresenter.MaxLines = 1;
                checkMark.HorizontalAlignment = HorizontalAlignment.Stretch;
                checkMark.VerticalAlignment = VerticalAlignment.Bottom;
                checkMark.Height = 2;
                checkMark.Width = double.NaN;
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
            Click?.Invoke(this, new RoutedEventArgs());
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

            Storyboard storyboard = new Storyboard();

            DoubleAnimation da1 = new DoubleAnimation()
            {
                To = 0.1,
                Duration = TimeSpan.FromMilliseconds(300),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da1, Background);
            Storyboard.SetTargetProperty(da1, "Opacity");

            storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, checkMarkScale);
            Storyboard.SetTargetProperty(da2, "ScaleY");

            storyboard.Children.Add(da2);

            DoubleAnimation da3 = new DoubleAnimation()
            {
                To = negative,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da3, iconTranslate);
            Storyboard.SetTargetProperty(da3, "Y");

            storyboard.Children.Add(da3);

            DoubleAnimation da4 = new DoubleAnimation()
            {
                To = positive,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da4, textTranslate);
            Storyboard.SetTargetProperty(da4, "Y");

            storyboard.Children.Add(da4);

            DoubleAnimation da5 = new DoubleAnimation()
            {
                To = 0.6,
                Duration = TimeSpan.FromMilliseconds(100),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da5, contentPresenter);
            Storyboard.SetTargetProperty(da5, "Opacity");

            storyboard.Children.Add(da5);

            storyboard.Begin();
        }


        private void BeginPointerOverAnimation()
        {
            double positive = GetTranslateOffset()[0];
            double negative = GetTranslateOffset()[1];

            Storyboard storyboard = new Storyboard();

            DoubleAnimation da1 = new DoubleAnimation()
            {
                To = 0.1,
                Duration = TimeSpan.FromMilliseconds(300),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da1, Background);
            Storyboard.SetTargetProperty(da1, "Opacity");

            storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, checkMarkScale);
            Storyboard.SetTargetProperty(da2, "ScaleY");

            storyboard.Children.Add(da2);

            DoubleAnimation da3 = new DoubleAnimation()
            {
                To = negative,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da3, iconTranslate);
            Storyboard.SetTargetProperty(da3, "Y");

            storyboard.Children.Add(da3);

            DoubleAnimation da4 = new DoubleAnimation()
            {
                To = positive,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da4, textTranslate);
            Storyboard.SetTargetProperty(da4, "Y");

            storyboard.Children.Add(da4);

            DoubleAnimation da5 = new DoubleAnimation()
            {
                To = 0.6,
                Duration = TimeSpan.FromMilliseconds(100),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da5, contentPresenter);
            Storyboard.SetTargetProperty(da5, "Opacity");

            storyboard.Children.Add(da5);

            storyboard.Begin();

        }

        private void BeginNormalAnimation()
        {
            icon.Opacity = 0.6;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation da1 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da1, Background);
            Storyboard.SetTargetProperty(da1, "Opacity");

            storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, checkMarkScale);
            Storyboard.SetTargetProperty(da2, "ScaleY");

            storyboard.Children.Add(da2);

            DoubleAnimation da3 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da3, iconTranslate);
            Storyboard.SetTargetProperty(da3, "Y");

            storyboard.Children.Add(da3);

            DoubleAnimation da4 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da4, textTranslate);
            Storyboard.SetTargetProperty(da4, "Y");

            storyboard.Children.Add(da4);

            DoubleAnimation da5 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(100),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da5, contentPresenter);
            Storyboard.SetTargetProperty(da5, "Opacity");

            storyboard.Children.Add(da5);


            storyboard.Begin();
        }

        private void BeginPressedAnimation()
        {
            Storyboard storyboard = new Storyboard();

            DoubleAnimation da1 = new DoubleAnimation()
            {
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(300),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da1, Background);
            Storyboard.SetTargetProperty(da1, "Opacity");

            storyboard.Children.Add(da1);

            DoubleAnimation da2 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EnableDependentAnimation = true,
            };

            Storyboard.SetTarget(da2, checkMarkScale);
            Storyboard.SetTargetProperty(da2, "ScaleY");

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
    }
}
