using BackgroundAudioShared;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Dao;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.UserProfile;
using Windows.UI.Notifications;

namespace BackgroundAudioAgent
{
    public sealed class AudictiveMusicMediaPlayerAgent : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private SystemMediaTransportControls smtc;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private bool IsLoadingPlaylist;

        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            IsLoadingPlaylist = false;
            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += smtc_ButtonPressed;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.PropertyChanged += smtc_PropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;

            ToastNotificationManager.History.Clear("App");

            ApplicationSettings.PlaybackTimerStartTime = ApplicationSettings.PlaybackTimerDuration = 0;

            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.Current.MediaEnded += Current_MediaEnded;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            deferral = taskInstance.GetDeferral();
            backgroundTaskStarted.Set();

            taskInstance.Task.Completed += Task_Completed;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            ApplicationSettings.BackgroundTaskState = BackgroundTaskState.Running;
            // Send information to foreground that background task has been started if app is active
            if (ApplicationSettings.AppState != AppState.Suspended)
                MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

            //NotifyUser();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                backgroundTaskStarted.Reset();

                ApplicationSettings.BackgroundTaskState = BackgroundTaskState.Canceled;
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                BackgroundMediaPlayer.Current.CurrentStateChanged -= Current_CurrentStateChanged;
                BackgroundMediaPlayer.Current.MediaEnded -= Current_MediaEnded;
                smtc.ButtonPressed -= smtc_ButtonPressed;
                smtc.PropertyChanged -= smtc_PropertyChanged;
                smtc.DisplayUpdater.ClearAll();
                smtc.IsEnabled = false;

                SavePlaybackState();

                BackgroundMediaPlayer.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            deferral.Complete();
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            SavePlaybackState();
            deferral.Complete();
        }

        private void SavePlaybackState()
        {
            ToastNotificationManager.History.Remove("next");

            if (BackgroundMediaPlayer.Current.PlaybackSession.Position.TotalMilliseconds == BackgroundMediaPlayer.Current.PlaybackSession.NaturalDuration.TotalMilliseconds)
                ApplicationSettings.PlaybackLastPosition = 0;
            else
                ApplicationSettings.PlaybackLastPosition = BackgroundMediaPlayer.Current.PlaybackSession.Position.TotalMilliseconds;

            Dao_NowPlaying.SavePlaylist();

            if (NowPlaying.Current.Songs.Count > 0)
                NotifyUser();
        }

        private void NotifyUser()
        {
            if (ApplicationSettings.TapToResumeNotificationEnabled)
            {
                ResourceLoader res = new ResourceLoader();

                string toastXML = "";
                toastXML += "<toast launch=\"action=resumePlayback\" duration =\"short\">";
                toastXML += "<visual>";
                toastXML += "<binding template=\"ToastGeneric\">";
                toastXML += "<text>" + res.GetString("PlaybackStoppedToastHeader") + "</text>";
                toastXML += "<text>" + res.GetString("PlaybackStoppedToastContent") + "</text>";
                toastXML += "</binding>";
                toastXML += "</visual>";
                toastXML += "<actions>";
                toastXML += "<action content=\"" + res.GetString("SettingsString") + "\" arguments=\"action=navigate&amp;target=settings&amp;path=playback\" activationType=\"foreground\" />";
                toastXML += "</actions>";
                toastXML += "</toast>";

                XmlDocument toastXmlDoc = new XmlDocument();
                toastXmlDoc.LoadXml(toastXML);

                ToastNotification toast = new ToastNotification(toastXmlDoc)
                {
                    Tag = "tapToResume",
                    SuppressPopup = true
                };
                ToastNotificationManager.CreateToastNotifier("App").Show(toast);
            }
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["PlaybackState"] = sender.PlaybackSession.PlaybackState.ToString();

                if (ApplicationSettings.AppState == AppState.Active)
                {
                    MessageService.SendMessageToForeground(new CurrentStateChangedMessage(sender.PlaybackSession.PlaybackState));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //UpdateBadge(sender.PlaybackSession.PlaybackState);
        }

        private void UpdateBadge(MediaPlaybackState playbackState)
        {
            string badgeGlyphValue;

            if (playbackState == MediaPlaybackState.Playing)
                badgeGlyphValue = "playing";
            else if (playbackState == MediaPlaybackState.Paused)
                badgeGlyphValue = "paused";
            else
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication("App").Clear();
                return;
            }

            // Get the blank badge XML payload for a badge glyph
            XmlDocument badgeXml =
                BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);

            // Set the value of the badge in the XML to our glyph value
            Windows.Data.Xml.Dom.XmlElement badgeElement =
                badgeXml.SelectSingleNode("/badge") as Windows.Data.Xml.Dom.XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);

            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);

            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication("App");

            // And update the badge
            badgeUpdater.Update(badge);
        }

        private async void Current_MediaEnded(MediaPlayer sender, object args)
        {
            string s = ApplicationSettings.CurrentTrackPath;

            Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = s });

            if (LastFm.Current.IsAuthenticated && ApplicationSettings.IsScrobbleEnabled)
            {
                if (ApplicationInfo.Current.HasInternetConnection)
                {
                    Scrobble scrobble = new Scrobble(song.Artist, song.Album, song.Name, DateTimeOffset.Now.ToUniversalTime());
                    var response = await LastFm.Current.Client.Scrobbler.ScrobbleAsync(scrobble);
                }
                else
                {
                    PendingScrobble pendingScrobble = new PendingScrobble(song, DateTimeOffset.Now.ToUniversalTime());
                    Ctr_PendingScrobble.Current.Add(pendingScrobble);
                }
            }

            if (ApplicationSettings.IsPlaybackTimerEnabled)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (now >= ApplicationSettings.PlaybackTimerEndTime)
                {
                    ApplicationSettings.PlaybackTimerStartTime = ApplicationSettings.PlaybackTimerDuration = 0;

                    BackgroundMediaPlayer.Current.Pause();

                    //deferral.Complete();

                    return;
                }
            }

            try
            {
                if (ApplicationSettings.CurrentTrackIndex == NowPlaying.Current.Songs.Count - 1)
                {
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey("RepeatMode"))
                    {
                        if (ApplicationData.Current.LocalSettings.Values["RepeatMode"].ToString() == "All")
                        {
                            JumpToIndex(0, 0);
                        }
                    }
                }
                else
                {
                    if (ApplicationSettings.CurrentTrackIndex < NowPlaying.Current.Songs.Count - 1)
                    {
                        JumpToIndex(ApplicationSettings.CurrentTrackIndex + 1, 0);
                    }
                    else
                    {
                        BackgroundMediaPlayer.Current.PlaybackSession.Position = TimeSpan.FromMilliseconds(0);
                    }
                }

                Debug.WriteLine("MEDIA ENDED - SEM ERROS");
            }
            catch
            {
                Debug.WriteLine("MEDIA ENDED - ERRO");
            }
        }

        private void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            
        }

        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:

                    if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                        BackgroundMediaPlayer.Current.Play();
                    else
                        CreateMediaPlayback(false);

                    break;

                case SystemMediaTransportControlsButton.Pause:
                    BackgroundMediaPlayer.Current.Pause();
                    break;

                case SystemMediaTransportControlsButton.Next:
                    SkipToNext();
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    SkipToPrevious(false);
                    break;

                case SystemMediaTransportControlsButton.FastForward:

                    break;

                case SystemMediaTransportControlsButton.Rewind:

                    break;
            }
        }

        private async void CreateMediaPlayback(bool ignoreLastPlayback)
        {
            bool resume;

            if (ignoreLastPlayback)
                resume = false;
            else
                resume = ResumeLastPlayback();

            if (resume == false)
            {
                var songs = Ctr_Song.Current.GetAllSongsPaths();

                songs.Shuffle();
                if (songs.Count > 0)
                    SetPlaylist(songs);
            }
        }

        private void SkipToNext()
        {
            try
            {
                // VERIFICA SE É A ÚLTIMA MÚSICA DA LISTA
                // SE NÃO FOR, VAI PARA A PRÓXIMA
                // SE FOR, PULA PARA A PRIMEIRA
                if (ApplicationSettings.CurrentTrackIndex < NowPlaying.Current.Songs.Count - 1)
                {
                    JumpToIndex(ApplicationSettings.CurrentTrackIndex + 1, 0);
                }
                else
                {
                    JumpToIndex(0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SkipToPrevious(bool skipTimeCheck)
        {
            try
            {
                bool goToPrevious = false;
                // VERIFICA O ESTADO DA REPRODUÇÃO
                // SE ESTIVER PAUSADO, PULA PARA A FAIXA ANTERIOR
                // SENÃO, VOLTA À 0 SE A POSIÇÃO DA MÚSICA FOR MAIOR QUE 3 SEGUNDOS
                // SENÃO, PULA PARA ANTERIOR
                goToPrevious = BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Paused || BackgroundMediaPlayer.Current.PlaybackSession.Position.TotalSeconds < 3;

                if (goToPrevious || skipTimeCheck)
                {
                    if (ApplicationSettings.CurrentTrackIndex == 0)
                    {
                        JumpToIndex(NowPlaying.Current.Songs.Count - 1, 0);
                    }
                    else
                    {
                        JumpToIndex(ApplicationSettings.CurrentTrackIndex - 1, 0);
                    }
                }
                else
                    BackgroundMediaPlayer.Current.PlaybackSession.Position = TimeSpan.FromMilliseconds(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            AddSongsToPlaylist addSongsToPlaylist;
            if (MessageService.TryParseMessage(e.Data, out addSongsToPlaylist))
            {
                if (addSongsToPlaylist.SongsToAdd.Count == 0)
                    return;

                // Quantidade de músicas ANTES de adicionar as músicas.
                int playlistCount = NowPlaying.Current.Songs.Count;

                string prev, next = string.Empty;

                if (addSongsToPlaylist.AsNext == false)
                {
                    NowPlaying.Current.Songs.AddRange(addSongsToPlaylist.SongsToAdd);
                }
                else
                {
                    if (NowPlaying.Current.Songs.Count == 0)
                    {
                        NowPlaying.Current.Songs.InsertRange(0, addSongsToPlaylist.SongsToAdd);
                    }
                    else
                    {
                        NowPlaying.Current.Songs.InsertRange(ApplicationSettings.CurrentTrackIndex + 1, addSongsToPlaylist.SongsToAdd);
                    }                    
                }

                if (NowPlaying.Current.Songs.Count == 1)
                {
                    ApplicationSettings.PreviousSong = "";
                    ApplicationSettings.NextSong = "";
                }
                else
                {
                    if (ApplicationSettings.CurrentTrackIndex > 0)
                        prev = NowPlaying.Current.Songs[ApplicationSettings.CurrentTrackIndex - 1];
                    else
                        prev = NowPlaying.Current.Songs[NowPlaying.Current.Songs.Count - 1];

                    ApplicationSettings.PreviousSong = prev;

                    if (ApplicationSettings.CurrentTrackIndex < NowPlaying.Current.Songs.Count - 1)
                        next = NowPlaying.Current.Songs[ApplicationSettings.CurrentTrackIndex + 1];
                    else
                        next = "";

                    ApplicationSettings.NextSong = next;
                }

                // Se não havia músicas na lista, deve-se iniciar a reprodução
                if (playlistCount == 0)
                {
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didnt initialize in time");

                    JumpToIndex(0, 0);
                }
                // Se já havia uma reprodução em andamento, apenas atualiza a notificação na central de ações
                else
                {
                    NowPlaying.Current.ToastManager(ApplicationSettings.CurrentTrackIndex);
                }

                if (ApplicationSettings.AppState == AppState.Active)
                    MessageService.SendMessageToForeground(new PlaylistMessage(NowPlaying.Current.Songs));
            }

            SetPlaylistMessage setPlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out setPlaylistMessage))
            {
                SetPlaylist(setPlaylistMessage.Playlist);
            }

            ActionMessage actionMessage;
            if (MessageService.TryParseMessage(e.Data, out actionMessage))
            {
                switch (actionMessage.Action)
                {
                    case BackgroundAudioShared.Messages.Action.AskCurrentTrack:

                        break;

                    case BackgroundAudioShared.Messages.Action.AskPlaylist:
                        Debug.WriteLine("ACTION MESSAGE... ASK PLAYLIST...\nAppState: " + ApplicationSettings.AppState.ToString());

                        if (ApplicationSettings.AppState == AppState.Active
                            )
                            MessageService.SendMessageToForeground(new PlaylistMessage(NowPlaying.Current.Songs));
                        break;

                    case BackgroundAudioShared.Messages.Action.Resume:

                        CreateMediaPlayback(false);

                        break;

                    case BackgroundAudioShared.Messages.Action.PlayEverything:

                        CreateMediaPlayback(true);

                        break;

                    case BackgroundAudioShared.Messages.Action.Shuffle:
                        if (NowPlaying.Current.Songs.Count == 0)
                            return;

                        string playingSong = NowPlaying.Current.Songs[ApplicationSettings.CurrentTrackIndex];

                        List<string> ShuffleList = new List<string>();

                        foreach (string file in NowPlaying.Current.Songs)
                        {
                            if (file != playingSong)
                                ShuffleList.Add(file);
                        }

                        NowPlaying.Current.Songs.Clear();

                        //ShuffleToggleButton.IsChecked = true;
                        ShuffleList.Shuffle();

                        NowPlaying.Current.Songs.Add(playingSong);

                        ApplicationData.Current.LocalSettings.Values["CurrentTrackIndex"] = 0;

                        foreach (string s in ShuffleList)
                        {
                            NowPlaying.Current.Songs.Add(s);
                        }

                        NowPlaying.Current.ToastManager(0);

                        if (ApplicationSettings.AppState == AppState.Active)
                        {
                            MessageService.SendMessageToForeground(new PlaylistMessage(NowPlaying.Current.Songs));
                            MessageService.SendMessageToForeground(new CurrentTrackMessage(playingSong, 0));
                        }

                        break;

                    case BackgroundAudioShared.Messages.Action.SkipToPrevious:
                        SkipToPrevious(actionMessage.Parameter == "skip");
                        break;

                    case BackgroundAudioShared.Messages.Action.PlayPause:
                        if (BackgroundMediaPlayer.Current.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                            BackgroundMediaPlayer.Current.Pause();
                        else
                            BackgroundMediaPlayer.Current.Play();
                        break;

                    case BackgroundAudioShared.Messages.Action.SkipToNext:
                        SkipToNext();
                        break;

                    case BackgroundAudioShared.Messages.Action.Stop:

                        break;
                }
            }

            EditPlaylistMessage editPlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out editPlaylistMessage))
            {
                var item = NowPlaying.Current.Songs[editPlaylistMessage.TrackIndex];
                int i = ApplicationSettings.CurrentTrackIndex;

                switch (editPlaylistMessage.Mode)
                {
                    case Mode.Remove:

                        int playlistCount = NowPlaying.Current.Songs.Count;
                        NowPlaying.Current.Songs.RemoveAt(editPlaylistMessage.TrackIndex);

                        // SE A MÚSICA REMOVIDA FOR ANTES DA MÚSICA QUE ESTÁ TOCANDO -- PS: SE A MÚSICA REMOVIDA FOR DEPOIS DA ATUAL, NADA A FAZER
                        if (ApplicationSettings.CurrentTrackIndex > editPlaylistMessage.TrackIndex)
                        {
                            ApplicationSettings.CurrentTrackIndex = ApplicationSettings.CurrentTrackIndex - 1;
                        }
                        // SE A MÚSICA REMOVIDA FOR A MÚSICA EM REPRODUÇÃO
                        else if (ApplicationSettings.CurrentTrackIndex == editPlaylistMessage.TrackIndex)
                        {
                            // CHECA SE EXISTE MAIS DE UMA MÚSICA NA LISTA
                            if (playlistCount > 1)
                            {
                                // SE A MÚSICA FOR A ÚLTIMA DA LISTA
                                if (ApplicationSettings.CurrentTrackIndex == playlistCount - 1)
                                {
                                    ApplicationSettings.CurrentTrackIndex = ApplicationSettings.CurrentTrackIndex - 1;
                                    i = ApplicationSettings.CurrentTrackIndex;
                                    Debug.Write(i);
                                }
                                // SE A MÚSICA FOR A PRIMEIRA DA LISTA
                                else if (ApplicationSettings.CurrentTrackIndex == 0)
                                {
                                    ApplicationSettings.CurrentTrackIndex = 0;
                                }
                                // SE A MÚSICA ESTIVER NO MEIO - NADA A FAZER
                                else if (ApplicationSettings.CurrentTrackIndex < playlistCount - 1)
                                {

                                }
                                

                                JumpToIndex(ApplicationSettings.CurrentTrackIndex, 0);
                            }
                            // SE NÃO HOUVER OUTRA MÚSICA ALÉM DA REMOVIDA, MATA A REPRODUÇÃO
                            else
                            {
                                ClearPlayback();

                                return;
                            }
                        }

                        break;

                    case Mode.DragAndDrop:


                        if (editPlaylistMessage.TrackIndex > ApplicationSettings.CurrentTrackIndex
                            && editPlaylistMessage.NewIndex <= ApplicationSettings.CurrentTrackIndex)
                        {
                            ApplicationSettings.CurrentTrackIndex = ApplicationSettings.CurrentTrackIndex + 1;
                        }
                        else if (editPlaylistMessage.TrackIndex < ApplicationSettings.CurrentTrackIndex
                            && editPlaylistMessage.NewIndex >= ApplicationSettings.CurrentTrackIndex)
                        {
                            ApplicationSettings.CurrentTrackIndex = ApplicationSettings.CurrentTrackIndex - 1;
                        }
                        else if (editPlaylistMessage.TrackIndex == ApplicationSettings.CurrentTrackIndex)
                        {
                            ApplicationSettings.CurrentTrackIndex = editPlaylistMessage.NewIndex;
                        }

                        if (ApplicationSettings.CurrentTrackIndex < NowPlaying.Current.Songs.Count - 1)
                            ApplicationSettings.NextSong = NowPlaying.Current.Songs[ApplicationSettings.CurrentTrackIndex + 1];
                        else
                            ApplicationSettings.NextSong = "";

                        string song = NowPlaying.Current.Songs[editPlaylistMessage.TrackIndex];
                        NowPlaying.Current.Songs.RemoveAt(editPlaylistMessage.TrackIndex);
                        NowPlaying.Current.Songs.Insert(editPlaylistMessage.NewIndex, song);

                        break;
                }

                NowPlaying.Current.ToastManager(ApplicationSettings.CurrentTrackIndex);

                if (ApplicationSettings.AppState == AppState.Active)
                {
                    i = ApplicationSettings.CurrentTrackIndex;

                    MessageService.SendMessageToForeground(new CurrentTrackMessage(NowPlaying.Current.Songs[ApplicationSettings.CurrentTrackIndex], ApplicationSettings.CurrentTrackIndex));

                }
            }

            JumpToIndexMessage jumpToIndexMessage;
            if (MessageService.TryParseMessage(e.Data, out jumpToIndexMessage))
            {
                JumpToIndex(jumpToIndexMessage.Index, 0);
            }

            AppStateMessage appStateMessage;
            if (MessageService.TryParseMessage(e.Data, out appStateMessage))
            {
                ApplicationSettings.AppState = appStateMessage.State;
            }

            ClearPlaylistMessage clearPlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out clearPlaylistMessage))
            {
                int i = ApplicationSettings.CurrentTrackIndex;
                if (NowPlaying.Current.Songs.Count == 0)
                    return;

                string currentSong = NowPlaying.Current.Songs[i];

                NowPlaying.Current.Songs.Clear();

                NowPlaying.Current.Songs.Add(currentSong);
                ApplicationSettings.CurrentTrackIndex = 0;

                NowPlaying.Current.ToastManager(ApplicationSettings.CurrentTrackIndex);

                if (ApplicationSettings.AppState == AppState.Active)
                {
                    i = ApplicationSettings.CurrentTrackIndex;

                    if (ApplicationSettings.AppState == AppState.Active)
                        MessageService.SendMessageToForeground(new PlaylistMessage(NowPlaying.Current.Songs));
                    MessageService.SendMessageToForeground(new CurrentTrackMessage(NowPlaying.Current.Songs[ApplicationSettings.CurrentTrackIndex], ApplicationSettings.CurrentTrackIndex));

                }
            }
        }

        private async void ClearPlayback()
        {
            NowPlaying.Current.Songs.Clear();

            smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
            smtc.DisplayUpdater.MusicProperties.Artist = string.Empty;
            smtc.DisplayUpdater.MusicProperties.AlbumTitle = string.Empty;
            smtc.DisplayUpdater.Thumbnail = null;
            smtc.DisplayUpdater.Update();

            BackgroundMediaPlayer.Current.Source = null;

            ApplicationSettings.CurrentTrackIndex = 0;
            ApplicationSettings.CurrentTrackPath = string.Empty;
            ApplicationSettings.PlaybackLastPosition = 0;

            await PlaylistHelper.SaveCurrentPlaylist();

            if (ApplicationSettings.AppState == AppState.Active)
                MessageService.SendMessageToForeground(new ActionMessage(BackgroundAudioShared.Messages.Action.ClearPlayback));
        }

        private bool ResumeLastPlayback()
        {
            IsLoadingPlaylist = true;

            bool result;
            List<string> list = CustomPlaylistsHelper.GetLastPlaylistFromFile();
            int index = ApplicationSettings.CurrentTrackIndex;
            double milliseconds = ApplicationSettings.PlaybackLastPosition;

            if (list != null)
                if (list.Count > 0)
                {
                    result = true;
                    SetPlaylist(list, index, milliseconds);
                }
                else
                    result = false;
            else
                result = false;

            return result;
        }

        private void SetPlaylist(List<string> playlist, int index = 0, double position = 0)
        {
            bool result = backgroundTaskStarted.WaitOne(5000);
            if (!result)
                throw new Exception("Background Task didnt initialize in time");

            if (playlist.Count == 0)
                return;

            NowPlaying.Current.Songs.Clear();

            NowPlaying.Current.Songs.AddRange(playlist);

            JumpToIndex(index, position);

            IsLoadingPlaylist = false;
            if (ApplicationSettings.AppState == AppState.Active)
                MessageService.SendMessageToForeground(new PlaylistMessage(NowPlaying.Current.Songs));
        }

        private void JumpToIndex(int index, double position)
        {
            try
            {
                ApplicationSettings.CurrentTrackIndex = index;
                ApplicationSettings.CurrentTrackPath = NowPlaying.Current.Songs[index];
                if (ApplicationSettings.AppState == AppState.Active)
                {
                    MessageService.SendMessageToForeground(new CurrentTrackMessage(NowPlaying.Current.Songs[index], index));
                }
                SetMediaSource(index, position);

                string prev, next = string.Empty;

                if (index > 0)
                    prev = NowPlaying.Current.Songs[index - 1];
                else
                    prev = NowPlaying.Current.Songs[NowPlaying.Current.Songs.Count - 1];

                ApplicationData.Current.LocalSettings.Values["PreviousSong"] = prev;

                if (index < NowPlaying.Current.Songs.Count - 1)
                    next = NowPlaying.Current.Songs[index + 1];
                else
                    next = "";

                ApplicationData.Current.LocalSettings.Values["NextSong"] = next;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void UpdateTile(Song song)
        {
            if (song == null)
            {
                TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();

                return;
            }

            try
            {
                string artistFileName = StringHelper.RemoveSpecialChar(song.Artist);

                #region tileCode

                var tileContent1 = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.None,
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = "ms-appdata:///local/Covers/cover_" + song.AlbumID + ".jpg",
                                    HintOverlay = 40
                                },
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = "ms-appx:///Assets/Logos/Fluent/Square150.png"
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.None,
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = "ms-appdata:///local/Artists/artist_" + artistFileName + ".jpg",
                                    HintOverlay = 40
                                },
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = "ms-appx:///Assets/Logos/Fluent/Square150.png"
                                    }
                                }
                            }
                        }
                    }
                };

                var tileContent2 = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileSmall = new TileBinding()
                        {
                            Branding = TileBranding.None,
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = "ms-appdata:///local/Covers/cover_" + song.AlbumID + ".jpg",
                                    HintOverlay = 30
                                },
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = "ms-appx:///Assets/Logos/Fluent/Square71.png"
                                    }
                                }
                            }
                        },
                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            //DisplayName = ApplicationInfo.Current.Resources.GetString("Playing"),
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = song.Name,
                                        HintMaxLines = 2,
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.Body,
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = song.Artist,
                                        HintWrap = true,
                                        HintMaxLines = 2,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.None,
                            Content = new TileBindingContentAdaptive()
                            {
                                TextStacking = TileTextStacking.Top,
                                Children =
                                {
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 3,
                                                Children =
                                                {
                                                    new AdaptiveImage()
                                                    {
                                                        Source = "ms-appdata:///local/Covers/cover_" + song.AlbumID + ".jpg",
                                                    }
                                                }
                                            },
                                            new AdaptiveSubgroup(){ HintWeight = 7 },
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 2,
                                                Children =
                                                {
                                                    new AdaptiveImage()
                                                    {
                                                        Source = "ms-appx:///Assets/Logos/Fluent/Square44.png",
                                                    },
                                                }
                                            }
                                        }
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = song.Name,
                                        HintWrap = false,
                                        HintStyle = AdaptiveTextStyle.Base,
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = song.Artist,
                                        HintWrap = false,
                                        HintStyle = AdaptiveTextStyle.Caption,
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = "ms-appdata:///local/Covers/cover_" + song.AlbumID + ".jpg",
                                    HintOverlay = 70,
                                },
                                Children =
                                {
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup() { HintWeight = 2 },
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 6,
                                                Children =
                                                {
                                                    new AdaptiveText() { HintStyle = AdaptiveTextStyle.CaptionSubtle, HintMinLines = 1},
                                                    new AdaptiveImage()
                                                    {
                                                        Source = "ms-appdata:///local/Artists/artist_" + artistFileName + ".jpg",
                                                        HintCrop = AdaptiveImageCrop.Circle,
                                                    },

                                                }
                                            },
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 2,
                                                Children =
                                                {
                                                    new AdaptiveImage()
                                                    {
                                                        Source = "ms-appx:///Assets/Logos/Fluent/Square44.png",
                                                    },
                                                }
                                            },
                                        }
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = song.Name,
                                        HintStyle = AdaptiveTextStyle.Body,
                                        HintAlign = AdaptiveTextAlign.Center
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = song.Artist,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintAlign = AdaptiveTextAlign.Center,
                                    }
                                }
                            }
                        }

                    }
                };

                #endregion

                var notification1 = new TileNotification(tileContent1.GetXml());
                notification1.Tag = "playingUpdate1";
                var notification2 = new TileNotification(tileContent2.GetXml());
                notification2.Tag = "playingUpdate2";

                TileUpdateManager.CreateTileUpdaterForApplication("App").EnableNotificationQueueForSquare150x150(true);
                TileUpdateManager.CreateTileUpdaterForApplication("App").EnableNotificationQueueForWide310x150(true);
                TileUpdateManager.CreateTileUpdaterForApplication("App").EnableNotificationQueueForSquare310x310(false);

                TileUpdateManager.CreateTileUpdaterForApplication("App").Update(notification1);
                TileUpdateManager.CreateTileUpdaterForApplication("App").Update(notification2);

                UpdateLockScreen(song.AlbumID, artistFileName);

                NowPlaying.Current.ToastManager(ApplicationSettings.CurrentTrackIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }




        private async void UpdateLockScreen(string albumID, string artistFileName)
        {
            try
            {
                if (ApplicationSettings.LockscreenEnabled && ApplicationInfo.Current.GetDeviceFormFactorType() == ApplicationInfo.DeviceFormFactorType.Phone)
                {
                    StorageFolder coversFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artists", CreationCollisionOption.OpenIfExists);

                    string imageFileName = string.Empty;

                    imageFileName = "artist_" + artistFileName + ".jpg";

                    try
                    {
                        IStorageItem storageItem = await coversFolder.TryGetItemAsync(imageFileName);

                        if (storageItem != null)
                        {
                            StorageFile imageFile = storageItem as StorageFile;

                            if (UserProfilePersonalizationSettings.IsSupported())
                            {
                                UserProfilePersonalizationSettings settings = UserProfilePersonalizationSettings.Current;
                                await settings.TrySetLockScreenImageAsync(imageFile);
                            }
                        }
                        //sampleFile = null;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void SetMediaSource(int index, double position)
        {
            try
            {
                StorageFile mediaFile = await StorageFile.GetFileFromPathAsync(NowPlaying.Current.Songs[index]);

                MediaSource source = MediaSource.CreateFromStorageFile(mediaFile);
                BackgroundMediaPlayer.Current.Source = source;
                BackgroundMediaPlayer.Current.PlaybackSession.Position = TimeSpan.FromMilliseconds(position);

                UpdateUVCOnNewTrack(NowPlaying.Current.Songs[index]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void UpdateUVCOnNewTrack(string songPath)
        {
            try
            {
                Song song = Ctr_Song.Current.GetSong(new Song() { SongURI = songPath });

                if (song != null)
                {
                    
                    smtc.DisplayUpdater.MusicProperties.Title = song.Name;
                    smtc.DisplayUpdater.MusicProperties.Artist = song.Artist;
                    smtc.DisplayUpdater.MusicProperties.AlbumTitle = song.Album;

                    if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") == false)
                    {
                        try
                        {
                            smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Album() { ID = song.AlbumID }.GetCoverUri());
                        }
                        catch
                        {
                            smtc.DisplayUpdater.Thumbnail = null;
                        }
                    }
                }
                else
                {
                    StorageFile musicFile = await StorageFile.GetFileFromPathAsync(songPath);
                    await smtc.DisplayUpdater.CopyFromFileAsync(MediaPlaybackType.Music, musicFile);
                }

                smtc.DisplayUpdater.Update();

                UpdateTile(song);

                if (song != null)
                {
                    if (LastFm.Current.IsAuthenticated && ApplicationSettings.IsScrobbleEnabled && ApplicationInfo.Current.HasInternetConnection)
                    {
                        Scrobble scrobble = new Scrobble(song.Artist, song.Album, song.Name, DateTimeOffset.Now.ToUniversalTime());
                        LastResponse response = await LastFm.Current.Client.Track.UpdateNowPlayingAsync(scrobble);
                        Debug.WriteLine("UPDATING LASTFM NOW PLAYING:\n" + song.Name + "\n" + response.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

}
