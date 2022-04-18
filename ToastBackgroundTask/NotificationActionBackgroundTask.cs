using ClassLibrary.Control;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace ToastBackgroundTask
{
    public sealed class NotificationActionBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;

            if (details != null)
            {
                string arguments = details.Argument;
                if (arguments == "removeNextSong")
                {
                    Debug.WriteLine("MÚSICAS NA LISTA: " + NowPlaying.Current.Songs.Count.ToString());
                    Debug.WriteLine("POSIÇÃO ATUAL: " + ApplicationSettings.CurrentTrackIndex);
                    if (NowPlaying.Current.Songs.Count > ApplicationSettings.CurrentTrackIndex)
                    {
                        NowPlaying.Current.Songs.RemoveAt(ApplicationSettings.CurrentTrackIndex + 1);
                        NowPlaying.Current.ToastManager(ApplicationSettings.CurrentTrackIndex);
                    }
                }
                // Perform tasks
            }
        }
    }
}
