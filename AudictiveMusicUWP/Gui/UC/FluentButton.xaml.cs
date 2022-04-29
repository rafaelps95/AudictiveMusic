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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class FluentButton : UserControl
    {
        public event RoutedEventHandler Click;

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(FluentButton), new PropertyMetadata(""));





        public bool IsMainAction
        {
            get { return (bool)GetValue(IsMainActionProperty); }
            set { SetValue(IsMainActionProperty, value);
                if (value)
                {
                    button.Background = MainActionBackground;
                    if (MainActionBackground.Color.IsDarkColor())
                        root.RequestedTheme = ElementTheme.Dark;
                    else
                        root.RequestedTheme = ElementTheme.Light;

                    if (IsMainAction)
                    {
                        button.BorderBrush = null;
                        button.BorderBrush = MainActionBorder;
                    }
                    button.Style = this.Resources["MainActionButtonStyle"] as Style;
                }
                else
                    button.Style = this.Resources["DefaultButtonStyle"] as Style;
            }
        }

        // Using a DependencyProperty as the backing store for IsMainAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMainActionProperty =
            DependencyProperty.Register("IsMainAction", typeof(bool), typeof(FluentButton), new PropertyMetadata(false));



        public SolidColorBrush MainActionBackground
        {
            get { return (SolidColorBrush)GetValue(MainActionBackgroundProperty); }
            set { SetValue(MainActionBackgroundProperty, value);
                if (IsMainAction)
                {
                    button.Background = null;
                    button.Background = value;
                    if (MainActionBackground.Color.IsDarkColor())
                        root.RequestedTheme = ElementTheme.Dark;
                    else
                        root.RequestedTheme = ElementTheme.Light;
                }
            }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainActionBackgroundProperty =
            DependencyProperty.Register("MainActionBackground", typeof(SolidColorBrush), typeof(FluentButton), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        public SolidColorBrush MainActionBorder
        {
            get { return (SolidColorBrush)GetValue(MainActionBorderProperty); }
            set
            {
                SetValue(MainActionBorderProperty, value);
                if (IsMainAction)
                {
                    button.BorderBrush = null;
                    button.BorderBrush = value;
                }
            }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainActionBorderProperty =
            DependencyProperty.Register("MainActionBorder", typeof(SolidColorBrush), typeof(FluentButton), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));



        private Visibility TextVisibility
        {
            get { return (Visibility)GetValue(TextVisibilityProperty); }
            set { SetValue(TextVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextVisibility.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty TextVisibilityProperty =
            DependencyProperty.Register("TextVisibility", typeof(Visibility), typeof(FluentButton), new PropertyMetadata(Visibility.Visible));



        public bool IconOnly
        {
            get { return (bool)GetValue(IconOnlyProperty); }
            set
            {
                SetValue(IconOnlyProperty, value);
                TextVisibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        // Using a DependencyProperty as the backing store for IconOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconOnlyProperty =
            DependencyProperty.Register("IconOnly", typeof(bool), typeof(FluentButton), new PropertyMetadata(false));




        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FluentButton), new PropertyMetadata(""));


        public FluentButton()
        {
            this.Loaded += FluentButton_Loaded;
            this.InitializeComponent();
        }

        private void FluentButton_Loaded(object sender, RoutedEventArgs e)
        {
            TextVisibility = IconOnly ? Visibility.Collapsed : Visibility.Visible;
            //if (IsMainAction)
            //{
            //    button.Background = MainActionBackground;
            //    if (MainActionBackground.Color.IsDarkColor())
            //        root.RequestedTheme = ElementTheme.Dark;
            //    else
            //        root.RequestedTheme = ElementTheme.Light;
            //}

            if (IsMainAction)
            {
                button.Background = MainActionBackground;
                if (MainActionBackground.Color.IsDarkColor())
                    root.RequestedTheme = ElementTheme.Dark;
                else
                    root.RequestedTheme = ElementTheme.Light;

                button.BorderBrush = MainActionBorder;
                button.Style = this.Resources["MainActionButtonStyle"] as Style;
            }
            else
                button.Style = this.Resources["DefaultButtonStyle"] as Style;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
    }



    public class IconSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double size = (double)value;
            size = size * 0.8;

            return size;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
