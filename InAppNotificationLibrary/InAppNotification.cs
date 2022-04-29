using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InAppNotificationLibrary
{
    public class InAppNotification
    {
        internal delegate void ClosedNotificationHandler(Notification notification);
        public event RoutedEventHandler PrimaryButtonClicked;
        public event RoutedEventHandler SecondaryButtonClicked;
        public event RoutedEventHandler PrimaryButtonIsEnabledChanged;
        internal event ClosedNotificationHandler Closed;

        internal void PrimaryButtonClick() => PrimaryButtonClicked?.Invoke(this, new RoutedEventArgs());
        internal void SecondaryButtonClick() => SecondaryButtonClicked?.Invoke(this, new RoutedEventArgs());
        internal void Close(Notification notification) => Closed?.Invoke(notification);

        public string Title;
        public string Message;
        public string Icon;
        public object PrimaryButtonContent;
        public object SecondaryButtonContent;

        private bool primaryButtonEnabled = true;
        public bool PrimaryButtonEnabled
        {
            get { return primaryButtonEnabled; }
            set { primaryButtonEnabled = value; PrimaryButtonIsEnabledChanged?.Invoke(this, new RoutedEventArgs()); }
        }
        public FrameworkElement ExternalControl { get; private set; }

        public InAppNotification()
        {

        }

        /// <summary>
        /// Creates an instance of an InAppNotification with parameters
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="icon">An icon that uses the Segoe Fluent Icon font</param>
        public InAppNotification(string title, string message, string icon, object primaryButtonContent, object secondaryButtonContent = null)
        {
            this.Title = title;
            this.Message = message;
            this.Icon = icon;
            this.PrimaryButtonContent = primaryButtonContent;
            this.SecondaryButtonContent = secondaryButtonContent;
        }

        public void SetCustomContent(TextBox frameworkElement)
        {
            this.ExternalControl = frameworkElement;
        }

        public void SetCustomContent(ComboBox frameworkElement)
        {
            this.ExternalControl = frameworkElement;
        }

        internal void Hide(Notification notification) => InAppNotificationHelper.HideNotification(notification);
    }
}
