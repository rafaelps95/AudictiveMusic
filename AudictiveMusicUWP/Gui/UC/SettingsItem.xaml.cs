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
    public enum SettingsItemContentType
    {
        None,
        CheckBox,
        ToggleSwitch
    }

    public sealed partial class SettingsItem : UserControl
    {
        public delegate void SettingsItemClickEventArgs(object sender, RoutedEventArgs e);

        public event RoutedEventHandler Checked;
        public event RoutedEventHandler Unchecked;
        public event RoutedEventHandler Toggled;
        public event SettingsItemClickEventArgs ItemClick;

        private ToggleSwitch _toggleSwitch = null;
        private CheckBox _checkBox = null;

        private bool _clicked = false;
        private bool _ignoreToggleEvent = false;
        private bool _ignoreCheckEvent = false;

        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(SettingsItem), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));




        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set
            {
                SetValue(IsCheckedProperty, value);

                if (this.ContentType == SettingsItemContentType.CheckBox)
                {
                    _ignoreCheckEvent = true;
                    _checkBox.IsChecked = value;
                }
            }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(SettingsItem), new PropertyMetadata(false));



        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set
            {
                SetValue(IsOnProperty, value);

                if (this.ContentType == SettingsItemContentType.ToggleSwitch)
                {
                    if (_toggleSwitch != null)
                    {
                        _ignoreToggleEvent = true;
                        _toggleSwitch.IsOn = value;
                        _ignoreToggleEvent = false;
                    }
                }
            }
        }

        // Using a DependencyProperty as the backing store for IsOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(SettingsItem), new PropertyMetadata(false));




        public SettingsItemContentType ContentType
        {
            get { return (SettingsItemContentType)GetValue(ContentTypeProperty); }
            set
            {
                SetValue(ContentTypeProperty, value);
                if (this.IsClickable == false)
                    SetContent();
            }
        }

        // Using a DependencyProperty as the backing store for ContentType. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentTypeProperty =
            DependencyProperty.Register("ContentType", typeof(SettingsItemContentType), typeof(SettingsItem), new PropertyMetadata(SettingsItemContentType.None));

        private void SetContent()
        {
            switch (this.ContentType)
            {
                case SettingsItemContentType.CheckBox:
                    _checkBox = new CheckBox();
                    _checkBox.IsTabStop = false;
                    Item.Children.Clear();
                    Item.Children.Add(_checkBox);
                    _checkBox.IsChecked = this.IsChecked;
                    _checkBox.Checked += _checkBox_Checked;
                    _checkBox.Unchecked += _checkBox_Unchecked;
                    break;
                case SettingsItemContentType.ToggleSwitch:
                    _toggleSwitch = new ToggleSwitch();
                    _toggleSwitch.IsTabStop = false;
                    Item.Children.Clear();
                    Item.Children.Add(_toggleSwitch);
                    _toggleSwitch.IsOn = this.IsOn;
                    _toggleSwitch.Toggled += _toggleSwitch_Toggled;

                    break;
                case SettingsItemContentType.None:

                    break;
            }
        }

        private void _checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_ignoreCheckEvent)
            {
                _ignoreCheckEvent = false;
                return;
            }

            if (this.ContentType == SettingsItemContentType.CheckBox)
                Unchecked?.Invoke(this, e);
        }

        public SettingsItem()
        {
            this.Loaded += SettingsItem_Loaded;
            this.GotFocus += SettingsItem_GotFocus;
            this.LostFocus += SettingsItem_LostFocus;
            this.InitializeComponent();
        }

        private void SettingsItem_LostFocus(object sender, RoutedEventArgs e)
        {
            //focusRectangle.Visibility = Visibility.Collapsed;
        }

        private void SettingsItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.FocusState == FocusState.Keyboard || this.FocusState == FocusState.Programmatic)
                actionButton.Focus(FocusState.Keyboard);
        }

        private void SettingsItem_Loaded(object sender, RoutedEventArgs e)
        {

            //actionButton.Visibility = this.IsClickable ? Visibility.Visible : Visibility.Collapsed;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsClickable)
                ItemClick?.Invoke(this, e);
            else
            {
                if (this.ContentType == SettingsItemContentType.CheckBox)
                {
                    if (_checkBox != null)
                    {
                        _ignoreCheckEvent = false;
                        _clicked = true;
                        if (_checkBox.IsChecked == true)
                            this.IsChecked = false;
                        else
                            this.IsChecked = true;
                    }
                }
                else if (this.ContentType == SettingsItemContentType.ToggleSwitch)
                {
                    if (_toggleSwitch != null)
                    {
                        _ignoreToggleEvent = false;
                        _clicked = true;
                        if (_toggleSwitch.IsOn)
                            this.IsOn = false;
                        else
                            this.IsOn = true;
                    }
                }
            }
        }

        private void _checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_ignoreCheckEvent && _clicked == false)
            {
                _ignoreCheckEvent = false;
                return;
            }

            this.IsChecked = _checkBox.IsChecked;
            _clicked = false;
            Checked?.Invoke(this, e);
        }

        private void _toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (_ignoreToggleEvent && _clicked == false)
            {
                _ignoreToggleEvent = false;
                return;
            }
            this.IsOn = _toggleSwitch.IsOn;
            _clicked = false;
            Toggled?.Invoke(this, e);
        }
    }
}
