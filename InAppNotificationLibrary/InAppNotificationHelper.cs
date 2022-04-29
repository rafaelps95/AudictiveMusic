using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAppNotificationLibrary
{
    public static class InAppNotificationHelper
    {
        public delegate void InAppNotificationHandler(Notification notification);
        public static event InAppNotificationHandler NotificationReceived;
        public static event InAppNotificationHandler NotificationDismissed;

        public static void ShowNotification(InAppNotification ian)
        {
            Notification notification = new Notification();
            notification.SetNotificationContent(ian);

            NotificationReceived?.Invoke(notification);
        }

        internal static void HideNotification(Notification notification)
        {
            NotificationDismissed?.Invoke(notification);
        }
    }
}
