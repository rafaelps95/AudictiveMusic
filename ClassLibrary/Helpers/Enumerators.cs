using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Helpers
{
    public static class Enumerators
    {
        public enum PageTheme
        {
            Dark = 0,
            Light = 1,
            Default = 2
        }

        public enum MediaItemType
        {
            Song,
            Artist,
            Album,
            Playlist,
            Folder,
            ListOfStrings
        }

        public enum PageContextType
        {
            Album,
            Artist,
            Playlist,
            Settings
        }

        public enum LayerType
        {
            Page,
            Popup
        }

        public enum SettingsPageContent
        {
            Personalization,
            AppInfo,
            Playback,
            Permissions,
            DataManagement,
            Menu,
            Feedback,
            None
        }
    }
}
