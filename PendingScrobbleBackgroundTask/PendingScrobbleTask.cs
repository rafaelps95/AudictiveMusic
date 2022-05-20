using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace PendingScrobbleBackgroundTask
{
    public sealed class PendingScrobbleTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            List<PendingScrobble> list = GetPendingScrobbles();

            if (list.Count > 0)
            {
                if (LastFm.Current.IsAuthenticated)
                    await Ctr_PendingScrobble.Current.SendScrobbles(list);
            }

            _deferral.Complete();
        }

        private List<PendingScrobble> GetPendingScrobbles()
        {
            return Ctr_PendingScrobble.Current.GetPendingScrobbles();
        }


    }
}
