using BackgroundAudioShared;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace ClassLibrary.Control
{
    public class NowPlaying
    {
        public enum PlaylistType
        {
            Custom = 0,
            Artist = 1,
            Album = 2,
            Playlist = 3,
            Favorites = 4
        }

        private static NowPlaying nowPlaying;

        public static NowPlaying Current
        {
            get
            {
                if (nowPlaying == null)
                    nowPlaying = new NowPlaying();

                return nowPlaying;
            }
        }

        private NowPlaying()
        {
            PlaylistContextType = PlaylistType.Custom;
            Index = "";
        }

        public PlaylistType PlaylistContextType;
        public int PlaylistContextID;
        public string Index;
        public double TrackPosition;
        public List<string> Songs = new List<string>();





        /// <summary>
        /// Toast notification helper
        /// </summary>
        /// <param name="currentTrackIndex">The index of the now playing track</param>
        public void ToastManager(int currentTrackIndex)
        {
            //deferral = instance.GetDeferral();

            ApplicationSettings.PreviousSong = string.Empty;
            ApplicationSettings.NextSong = string.Empty;

            ResourceLoader res = new ResourceLoader();
            try
            {
                if (NowPlaying.Current.Songs.Count == 1 || currentTrackIndex == NowPlaying.Current.Songs.Count - 1)
                {
                    ToastNotificationHistory n = ToastNotificationManager.History;
                    n.Clear("App");
                }
            }
            catch
            {

            }

            try
            {
                string prev;
                string next;

                if (NowPlaying.Current.Songs.Count > 1)
                {
                    if (currentTrackIndex > 0)
                        prev = NowPlaying.Current.Songs[currentTrackIndex - 1];
                    else
                        prev = NowPlaying.Current.Songs[NowPlaying.Current.Songs.Count - 1];

                    if (currentTrackIndex < NowPlaying.Current.Songs.Count - 1)
                        next = NowPlaying.Current.Songs[currentTrackIndex + 1];
                    else
                        next = "";

                    ApplicationSettings.PreviousSong = prev;
                    ApplicationSettings.NextSong = next;
                }
                else
                {
                    return;
                }

                if (currentTrackIndex < NowPlaying.Current.Songs.Count - 1)
                {

                    ApplicationSettings.NextSong = NowPlaying.Current.Songs[currentTrackIndex + 1];
                    Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = ApplicationSettings.NextSong });

                    if (song != null)
                    {
                        string nextArtist = song.Artist;
                        string nextSong = song.Name;
                        string albumID = song.AlbumID;


                        if (ApplicationSettings.NextSongInActionCenterEnabled == false)
                            return;


                        bool suppressPopup = ApplicationSettings.NextSongInActionCenterSuppressPopup;

                        string artistImagePath = "ms-appdata:///local/Artists/artist_" + StringHelper.RemoveSpecialChar(nextArtist) + ".jpg";
                        string albumImagePath = "ms-appdata:///local/Covers/cover_" + albumID + ".jpg";

                        string toastXML = "";
                        toastXML += "<toast launch=\"action=none\" duration=\"short\">";
                        if (suppressPopup == false)
                            toastXML += "<audio silent=\"true\" />";

                        toastXML += "<visual>";
                        toastXML += "<binding template=\"ToastGeneric\">";
                        toastXML += "<text hint-wrap=\"false\">" + res.GetString("WhatsNext") + "</text>";
                        toastXML += "<text>" + StringHelper.EscapeString(nextSong) + "</text>";
                        toastXML += "<text>" + StringHelper.EscapeString(nextArtist) + "</text>";
                        toastXML += "<image placement=\"appLogoOverride\" hint-crop=\"circle\" src=\"" + albumImagePath + "\" />";
                        //toastXML += "<image placement=\"hero\" src=\"" + artistImagePath + "\" />";
                        toastXML += "</binding>";
                        toastXML += "</visual>";
                        toastXML += "</toast>";

                        XmlDocument toastXmlDoc = new XmlDocument();
                        toastXmlDoc.LoadXml(toastXML);

                        ToastNotification toast = new ToastNotification(toastXmlDoc);
                        toast.Tag = "next";
                        toast.SuppressPopup = suppressPopup;
                        ToastNotificationManager.CreateToastNotifier("App").Show(toast);
                    }
                }
            }
            catch
            {

            }
            //}
        }

    }
}
