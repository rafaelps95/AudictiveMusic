using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace InAppNotificationLibrary
{
    public sealed partial class Notification : UserControl
    {
        private InAppNotification NotificationContent = null;

        internal void SetNotificationContent(InAppNotification ian)
        {
            this.NotificationContent = ian;
            this.NotificationContent.PrimaryButtonIsEnabledChanged += NotificationContent_PrimaryButtonIsEnabledChanged;

            this.Title = ian.Title;
            this.Subtitle = ian.Message;
            this.Icon = ian.Icon;
            if (ian.PrimaryButtonContent != null)
                PrimaryActionContent = ian.PrimaryButtonContent.ToString();
            if (ian.SecondaryButtonContent != null)
                SecondaryActionContent = ian.SecondaryButtonContent.ToString();

            if (ian.ExternalControl != null)
            {
                customContentArea.Visibility = Visibility.Visible;
                customContentArea.Children.Clear();
                customContentArea.Children.Add(ian.ExternalControl);
            }
            else
                customContentArea.Visibility = Visibility.Collapsed;

            action1.IsEnabled = ian.PrimaryButtonEnabled;
            defaultButtonsArea.Visibility = Visibility.Visible;
        }

        private void NotificationContent_PrimaryButtonIsEnabledChanged(object sender, RoutedEventArgs e)
        {
            action1.IsEnabled = this.NotificationContent.PrimaryButtonEnabled;
        }

        internal bool IsVisible
        {
            get;
            set;
        }

        internal bool IsRevealEnabled
        {
            get { return ((bool)GetValue(IsRevealEnabledProperty)); }
            set
            {
                SetValue(IsRevealEnabledProperty, value);
            }
        }

        internal static readonly DependencyProperty IsRevealEnabledProperty =
            DependencyProperty.Register("IsRevealEnabled", typeof(bool), typeof(Notification), new PropertyMetadata(false));

        internal bool IsPrimaryActionEnabled
        {
            get { return ((bool)GetValue(IsPrimaryActionEnabledProperty)); }
            set
            {
                SetValue(IsPrimaryActionEnabledProperty, value);
            }
        }

        internal static readonly DependencyProperty IsPrimaryActionEnabledProperty =
            DependencyProperty.Register("IsPrimaryActionEnabled", typeof(bool), typeof(Notification), new PropertyMetadata(true));

        internal bool IsSecondaryActionEnabled
        {
            get { return ((bool)GetValue(IsSecondaryActionEnabledProperty)); }
            set
            {
                SetValue(IsSecondaryActionEnabledProperty, value);
            }
        }

        internal static readonly DependencyProperty IsSecondaryActionEnabledProperty =
            DependencyProperty.Register("IsSecondaryActionEnabled", typeof(bool), typeof(Notification), new PropertyMetadata(true));

        internal string PrimaryActionContent
        {
            get { return ((string)GetValue(PrimaryActionContentProperty)); }
            set
            {
                SetValue(PrimaryActionContentProperty, value);
                action1.Visibility = Visibility.Visible;
            }
        }

        internal static readonly DependencyProperty PrimaryActionContentProperty =
            DependencyProperty.Register("PrimaryActionContent", typeof(string), typeof(Notification), new PropertyMetadata(string.Empty));

        internal string SecondaryActionContent
        {
            get { return ((string)GetValue(SecondaryActionContentProperty)); }
            set
            {
                SetValue(SecondaryActionContentProperty, value);
                action2.Visibility = Visibility.Visible;
            }
        }

        internal static readonly DependencyProperty SecondaryActionContentProperty =
            DependencyProperty.Register("SecondaryActionContent", typeof(string), typeof(Notification), new PropertyMetadata(string.Empty));

        internal string Icon
        {
            get { return ((string)GetValue(IconProperty)); }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        internal static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(Notification), new PropertyMetadata(""));

        internal string Title
        {
            get { return ((string)GetValue(TitleProperty)); }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        internal static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Notification), new PropertyMetadata(string.Empty));

        internal string Subtitle
        {
            get { return ((string)GetValue(SubtitleProperty)); }
            set
            {
                SetValue(SubtitleProperty, value);
            }
        }

        internal static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(Notification), new PropertyMetadata(string.Empty));

        public void Show()
        {
            if (this.NotificationContent == null)
                return;
            this.IsHitTestVisible = true;
            IsVisible = true;

            Storyboard sb = this.Resources["showAnimation"] as Storyboard;
            sb.Begin();
        }

        public void Hide()
        {
            if (this.NotificationContent == null)
                return;
            this.IsHitTestVisible = false;
            IsVisible = false;

            Storyboard sb = this.Resources["hideAnimation"] as Storyboard;
            sb.Begin();
        }

        internal Notification()
        {
            this.IsHitTestVisible = false;
            IsVisible = false;
            this.Opacity = 0;
            this.InitializeComponent();
        }

        private void action1_Click(object sender, RoutedEventArgs e)
        {
            this.NotificationContent.PrimaryButtonClick();
            Hide();
        }

        private void action2_Click(object sender, RoutedEventArgs e)
        {
            this.NotificationContent.SecondaryButtonClick();
            Hide();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void hideAnimation_Completed(object sender, object e)
        {
            this.NotificationContent.Hide(this);
        }

        private void Rectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Hide();
        }
    }
}
