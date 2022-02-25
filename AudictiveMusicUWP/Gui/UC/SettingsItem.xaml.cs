using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    [ContentProperty(Name = "Children")]
    public sealed partial class SettingsItem : UserControl
    {
        public delegate void SettingsItemClickEventArgs(object sender, RoutedEventArgs e);

        public event SettingsItemClickEventArgs ItemClick;

        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(SettingsItem), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));


        public SettingsItem()
        {
            this.Loaded += SettingsItem_Loaded;
            this.GotFocus += SettingsItem_GotFocus;
            this.LostFocus += SettingsItem_LostFocus;
            this.InitializeComponent();
            Children = Item.Children;
        }

        private void SettingsItem_LostFocus(object sender, RoutedEventArgs e)
        {
            focusRectangle.Visibility = Visibility.Collapsed;
        }

        private void SettingsItem_GotFocus(object sender, RoutedEventArgs e)
        {
            focusRectangle.Visibility = Visibility.Visible;
        }

        private void SettingsItem_Loaded(object sender, RoutedEventArgs e)
        {
            UIElement elem = Item.Children[0];

            if (elem as ToggleSwitch != null)
                ((ToggleSwitch)elem).Style = this.Resources["ToggleSwitchStyle"] as Style;

            if (elem as Button != null)
                ((Button)elem).Style = this.Resources["ButtonStyle"] as Style;

            if (elem as CheckBox != null)
                ((CheckBox)elem).Style = this.Resources["CheckBoxStyle"] as Style;

            if (elem as ComboBox != null)
                ((ComboBox)elem).Style = this.Resources["ComboBoxStyle"] as Style;

            actionButton.Visibility = this.IsClickable ? Visibility.Visible : Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(this.Icon))
                icon.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(this.AdditionalInfo))
            {
                Grid.SetRowSpan(text, 2);
                additionalInfoTB.Visibility = Visibility.Collapsed;
            }
            else
            {
                Grid.SetRowSpan(text, 1);
                additionalInfoTB.Visibility = Visibility.Visible;
            }
        }



        public bool IsClickable
        {
            get { return (bool)GetValue(IsClickableProperty); }
            set { SetValue(IsClickableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsClickable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register("IsClickable", typeof(bool), typeof(SettingsItem), new PropertyMetadata(false));



        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(SettingsItem), new PropertyMetadata(string.Empty));

        public int IconSize
        {
            get { return ((int)GetValue(IconSizeProperty)); }
            set
            {
                SetValue(IconSizeProperty, value);
            }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(SettingsItem), new PropertyMetadata(30));


        public static readonly DependencyProperty textProperty =
            DependencyProperty.Register("Text",
                typeof(string),
                typeof(SettingsItem),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)this.GetValue(textProperty); }
            set
            {
                this.SetValue(textProperty, value);

                NotifyPropertyChanged("Text");
            }
        }



        public string AdditionalInfo
        {
            get { return (string)GetValue(AdditionalInfoProperty); }
            set { SetValue(AdditionalInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdditionalInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdditionalInfoProperty =
            DependencyProperty.Register("AdditionalInfo", typeof(string), typeof(SettingsItem), new PropertyMetadata(string.Empty));



        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            "Children",
            typeof(UIElementCollection),
            typeof(SettingsItem),
            null);

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty); }
            private set
            {
                SetValue(ChildrenProperty, value);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public void Add(FrameworkElement elem)
        {
            Item.Children.Add(elem);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }
    }
}
