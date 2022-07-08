using BackgroundAudioShared;
using ClassLibrary.Control;
using ClassLibrary.Dao;
using ClassLibrary.Entities;
using ClassLibrary.Themes;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using ClassLibrary.Helpers.Enumerators;

namespace ClassLibrary.Helpers
{
    public static class ApplicationSettings
    {
        public static void LoadServiceSettings()
        {
            var resources = ResourceLoader.GetForCurrentView("Keys");
            LastFmAPIKey = resources.GetString("LastFmKey");
            LastFmSecret = resources.GetString("LastFmSecret");
            SpotifyApiId = resources.GetString("SpotifyId");
            SpotifyApiSecret = resources.GetString("SpotifySecret");
        }

        public static AppState AppState
        {
            get
            {
                object value = ReadSettingsValue("AppState");
                if (value == null)
                    return AppState.Unknown;
                else
                {
                    try
                    {
                        return (AppState)Enum.Parse(typeof(AppState), value.ToString());
                    }
                    catch
                    {
                        return AppState.Active;
                    }
                }
            }
            set
            {
                SaveSettingsValue("AppState", value.ToString());
            }
        }

        public static BackgroundTaskState BackgroundTaskState
        {
            get
            {
                object value = ReadSettingsValue("BackgroundTaskState");
                if (value == null)
                    return BackgroundTaskState.Unknown;
                else
                    return (BackgroundTaskState)Enum.Parse(typeof(BackgroundTaskState), value.ToString());
            }
            set
            {
                SaveSettingsValue("BackgroundTaskState", value.ToString());
            }
        }

        public static string CurrentTrackPath
        {
            get
            {
                object value = ReadSettingsValue("CurrentTrackPath");
                if (value == null)
                    return string.Empty;
                else
                    return (string)value;
            }
            set
            {
                SaveSettingsValue("CurrentTrackPath", value);
            }
        }

        public static double PlaybackLastPosition
        {
            get
            {
                object value = ReadSettingsValue("PlaybackLastPosition");
                if (value == null)
                    return 0;
                else
                    return Convert.ToDouble(value);
            }
            set
            {
                SaveSettingsValue("PlaybackLastPosition", value);
            }
        }

        public static bool IsCollectionLoaded
        {
            get
            {
                object setting = ReadSettingsValue("CollectionLoaded");
                if (setting == null)
                    return false;
                else
                    return (bool)setting;
            }
            set
            {
                SaveSettingsValue("CollectionLoaded", value);
            }
        }

        //public static bool IsBackgroundAudioTaskSuspended
        //{
        //    get
        //    {
        //        object setting = ReadSettingsValue("BackgroundAudioTaskSuspended");
        //        if (setting == null)
        //            return false;
        //        else
        //            return (bool)setting;
        //    }
        //    set
        //    {
        //        SaveSettingsValue("BackgroundAudioTaskSuspended", value);
        //    }
        //}


        public static Song CurrentSong
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentTrackPath))
                    return null;

                return SongDao.GetSong(new Song() { SongURI = CurrentTrackPath });
            }
        }

        public static int CurrentTrackIndex
        {
            get
            {
                object value = ReadSettingsValue("CurrentTrackIndex");
                if (value == null)
                    return 0;
                else
                    return (int)value;
            }
            set
            {
                SaveSettingsValue("CurrentTrackIndex", value);
            }
        }

        public static string LastFmSessionToken
        {
            get
            {
                object value = ReadSettingsValue("LastFmSessionToken");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("LastFmSessionToken", value);
            }
        }

        public static string LastFmSessionUsername
        {
            get
            {
                object value = ReadSettingsValue("LastFmSessionUsername");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("LastFmSessionUsername", value);
            }
        }

        public static string LastFmSessionUserImageUri
        {
            get
            {
                object value = ReadSettingsValue("LastFmSessionUserImageUri");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("LastFmSessionUserImageUri", value);
            }
        }

        public static string LastFmAPIKey
        {
            get
            {
                object value = ReadSettingsValue("LastFmAPIKey");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("LastFmAPIKey", value);
            }
        }

        public static string LastFmSecret
        {
            get
            {
                object value = ReadSettingsValue("LastFmSecret");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("LastFmSecret", value);
            }
        }

        public static string SpotifyApiId
        {
            get
            {
                object value = ReadSettingsValue("SpotifyApiId");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("SpotifyAPIId", value);
            }
        }

        public static string SpotifyApiSecret
        {
            get
            {
                object value = ReadSettingsValue("SpotifyApiSecret");
                if (value == null)
                    return string.Empty;
                else
                    return value.ToString();
            }
            set
            {
                SaveSettingsValue("SpotifyApiSecret", value);
            }
        }


        public static bool IsScrobbleEnabled
        {
            get
            {
                object value = ReadSettingsValue("ScrobbleEnabled");
                if (value == null)
                    return true;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("ScrobbleEnabled", value);
            }
        }

        public static bool IsPlaybackTimerEnabled
        {
            get => PlaybackTimerDuration != 0;
        }

        public static int PlaybackTimerDuration
        {
            get
            {
                object value = ReadSettingsValue("PlaybackTimerDuration");
                if (value == null)
                    return 0;
                else
                    return (int)value;
            }
            set
            {
                SaveSettingsValue("PlaybackTimerDuration", value);
            }
        }

        public static int PlaybackTimerDurationInSeconds
        {
            get => PlaybackTimerDuration * 60;
        }

        public static long PlaybackTimerEndTime
        {
            get => PlaybackTimerStartTime + PlaybackTimerDurationInSeconds;
        }

        public static long PlaybackTimerStartTime
        {
            get
            {
                object value = ReadSettingsValue("PlaybackTimerStartTime");
                if (value == null)
                    return 0;
                else
                    return (long)value;
            }
            set
            {
                SaveSettingsValue("PlaybackTimerStartTime", value);
            }
        }

        public static bool ThemesUserAware
        {
            get
            {
                object value = ReadSettingsValue("ThemesUserAware", SettingsContainerType.RoamingSettings);
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("ThemesUserAware", value, SettingsContainerType.RoamingSettings);
            }
        }

        public static bool PlaylistReorderUserAware
        {
            get
            {
                object value = ReadSettingsValue("PlaylistReorderUserAware", SettingsContainerType.RoamingSettings);
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("PlaylistReorderUserAware", value, SettingsContainerType.RoamingSettings);
            }
        }

        public static string PreviousSong
        {
            get
            {
                object value = ReadSettingsValue("PreviousSong");
                if (value == null)
                    return "";
                else
                    return (string)value;
            }
            set
            {
                SaveSettingsValue("PreviousSong", value);
            }
        }

        public static string NextSong
        {
            get
            {
                object value = ReadSettingsValue("NextSong");
                if (value == null)
                    return "";
                else
                    return (string)value;
            }
            set
            {
                SaveSettingsValue("NextSong", value);
            }
        }


        public static bool LockscreenEnabled
        {
            get
            {
                object value = ReadSettingsValue("Lockscreen");
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("Lockscreen", value);
            }
        }

        public static bool CellularDownloadEnabled
        {
            get
            {
                object value = ReadSettingsValue("CelullarDownload");
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("CelullarDownload", value);
            }
        }

        public static bool DownloadEnabled
        {
            get
            {
                object value = ReadSettingsValue("Download");
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("Download", value);
            }
        }


        public static bool NextSongInActionCenterEnabled
        {
            get
            {
                object value = ReadSettingsValue("WhatsNextNotification");
                if (value == null)
                    return true;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("WhatsNextNotification", value);
            }
        }

        public static bool NextSongInActionCenterSuppressPopup
        {
            get
            {
                //object value = ReadSettingsValue("WhatsNextNotificationSuppressPopup");
                //if (value == null)
                //    return true;
                //else
                //    return (bool)value;
                return true;
            }
            set
            {
                //SaveSettingsValue("WhatsNextNotificationSuppressPopup", value);
            }
        }

        public static bool TapToResumeNotificationEnabled
        {
            get
            {
                object value = ReadSettingsValue("DisplayTapToResumeToast");
                if (value == null)
                    return true;
                else
                    return (bool)value;
            }
            set
            {
                SaveSettingsValue("DisplayTapToResumeToast", value);
            }
        }



        /// <summary>
        /// Function to read a setting value
        /// </summary>
        public static object ReadSettingsValue(string key, SettingsContainerType type = SettingsContainerType.LocalSettings)
        {
            ApplicationDataContainer dataContainer;

            if (type == SettingsContainerType.LocalSettings)
                dataContainer = ApplicationData.Current.LocalSettings;
            else
                dataContainer = ApplicationData.Current.RoamingSettings;

            if (!dataContainer.Values.ContainsKey(key))
            {
                Debug.WriteLine("null returned");
                return null;
            }
            else
            {
                var value = dataContainer.Values[key];
                Debug.WriteLine("value found " + value.ToString());
                return value;
            }
        }

        /// <summary>
        /// Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveSettingsValue(string key, object value, SettingsContainerType type = SettingsContainerType.LocalSettings)
        {
            Debug.WriteLine(key + ":" + (value == null ? "null" : value.ToString()));

            ApplicationDataContainer dataContainer;

            if (type == SettingsContainerType.LocalSettings)
                dataContainer = ApplicationData.Current.LocalSettings;
            else
                dataContainer = ApplicationData.Current.RoamingSettings;

            if (!dataContainer.Values.ContainsKey(key))
            {
                dataContainer.Values.Add(key, value);
            }
            else
            {
                dataContainer.Values[key] = value;
            }
        }

        public enum SettingsContainerType
        {
            LocalSettings,
            RoamingSettings
        }

    }
}
