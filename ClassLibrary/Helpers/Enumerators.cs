using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Helpers.Enumerators
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

    public enum Theme
    {
        Clean = 0,
        Modern = 1,
        Blur = 2,
        Neon = 3,
        Material = 4,
    }

    public enum ThemeColorSource
    {
        AlbumColor = 0,
        AccentColor = 1,
        CustomColor = 2,
        NoColor = 3
    }

    public enum ThemeBackgroundSource
    {
        AlbumCover = 0,
        ArtistImage = 1
    }
}
