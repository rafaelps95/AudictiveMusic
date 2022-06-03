using BackgroundAudioShared;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ClassLibrary.Control
{
    public class PlayerController
    {
        public delegate void FullPlayerAnimationHandler(bool suppressAnimation);
        public static event FullPlayerAnimationHandler FullPlayerRequested;
        public delegate void PlaylistMessageHandler(List<string> playlist);
        public static event PlaylistMessageHandler PlaylistReceived;
        public delegate void CurrentIndexHandler(int index);
        public static event CurrentIndexHandler IndexReceived;
        public delegate void CurrentStateHandler(MediaPlaybackState state);
        public static event CurrentStateHandler CurrentStateChanged;
        public delegate void BackgroundActionHandler(BackgroundAudioShared.Messages.Action action);
        public static event BackgroundActionHandler BackgroundActionRequested;
        public static event RoutedEventHandler BackgroundTaskStarted;

        private static PlayerController _current;
        public static PlayerController Current
        {
            get
            {
                if (_current == null)
                    _current = new PlayerController();

                return _current;
            }
            private set
            {
                _current = value;
            }
        }
        private AutoResetEvent backgroundAudioTaskStarted;
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
        private CoreDispatcher Dispatcher;

        public void SetDispatcher(CoreDispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
        }

        public void RemoveDispatcher()
        {
            this.Dispatcher = null;
        }


        /// <summary>
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                while (mp == null && --retryCount >= 0)
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            // The foreground app uses RPC to communicate with the background process.
                            // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
                            // is returned when calling Current. We must restart the task, the while loop will retry to set mp.
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (mp == null)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }

                return mp;
            }
        }

        public MediaPlaybackState CurrentState
        {
            get
            {
                if (CurrentPlayer != null)
                    return CurrentPlayer.PlaybackSession.PlaybackState;
                else
                    return MediaPlaybackState.None;
            }
        }

        /// <summary>
        /// The background task did exist, but it has disappeared. Put the foreground back into an initial state. Unfortunately,
        /// any attempts to unregister things on BackgroundMediaPlayer.Current will fail with the RPC error once the background task has been lost.
        /// </summary>
        public void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            backgroundAudioTaskStarted.Reset();
            ApplicationSettings.BackgroundTaskState = BackgroundTaskState.Unknown;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
                else
                {
                    throw;
                }
            }
        }

        public void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var startResult = this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (ApplicationSettings.BackgroundTaskState != BackgroundTaskState.Running)
                {
                    bool result = backgroundAudioTaskStarted.WaitOne(10000);
                    //Send message to initiate playback
                    if (result == true)
                    {

                    }
                    else
                    {
                        throw new Exception("Background Audio Task didn't start in expected time");
                    }
                }
            });
            startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);

            
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
            
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        public void AddMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    ResetAfterLostBackground();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        public void RemoveMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // do nothing
                }
                else
                {
                    throw;
                }
            }
        }

        private PlayerController()
        {
            if (ApplicationSettings.BackgroundTaskState == BackgroundTaskState.Running)
                backgroundAudioTaskStarted = new AutoResetEvent(true);
            else
                backgroundAudioTaskStarted = new AutoResetEvent(false);
            //BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            PlaylistMessage playlistMessage;
            if (MessageService.TryParseMessage(e.Data, out playlistMessage))
            {
                PlaylistReceived?.Invoke(playlistMessage.Playlist);
            }

            CurrentTrackMessage currentTrackMessage;
            if (MessageService.TryParseMessage(e.Data, out currentTrackMessage))
            {
                IndexReceived?.Invoke(currentTrackMessage.Index);
            }

            CurrentStateChangedMessage currentStateChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out currentStateChangedMessage))
            {
                CurrentStateChanged?.Invoke(currentStateChangedMessage.State);
            }

            ActionMessage actionMessage;
            if (MessageService.TryParseMessage(e.Data, out actionMessage))
            {
                BackgroundActionRequested?.Invoke(actionMessage.Action);
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => {
                    BackgroundTaskStarted?.Invoke(this, new RoutedEventArgs());
                });

                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                Debug.WriteLine("BackgroundAudioTask started");
                backgroundAudioTaskStarted.Set();
                return;
            }
        }

        public static void OpenPlayer(bool suppressAnimation = false) => FullPlayerRequested?.Invoke(suppressAnimation);

        public static async void Play(MediaItem mediaItem)
        {
            List<string> list = await Collection.FetchSongs(mediaItem);
            Play(list);
        }

        public static async void Play(List<MediaItem> mediaItems)
        {
            List<string> list = new List<string>();
            foreach (MediaItem mediaItem in mediaItems)
            {
                List<string> tempList = await Collection.FetchSongs(mediaItem);
                list.AddRange(tempList);
            }

            Play(list);
        }

        public static void Play(List<string> list)
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new SetPlaylistMessage(list));
        }

        public static async void AddToQueue(MediaItem mediaItem, bool asNext = false)
        {
            List<string> list = await Collection.FetchSongs(mediaItem);
            AddToQueue(list, asNext);
        }

        public static async void AddToQueue(List<MediaItem> mediaItems, bool asNext = false)
        {
            List<string> list = new List<string>();
            foreach (MediaItem mediaItem in mediaItems)
            {
                List<string> tempList = await Collection.FetchSongs(mediaItem);
                list.AddRange(tempList);
            }

            AddToQueue(list, asNext);
        }

        public static void AddToQueue(List<string> list, bool asNext = false)
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new AddSongsToPlaylist(list, asNext));
        }

        public static void PlayPause()
        {
            if (Current.CurrentPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.None)
            {
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.Resume));

                return;
            }

            MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.PlayPause));
        }

        public static void Previous(bool forceSkip = false)
        {
            if (Current.CurrentPlayer != null)
            {
                if (forceSkip)
                    MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.SkipToPrevious, "skip"));
                else
                    MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.SkipToPrevious));
            }
        }

        public static void Next()
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.SkipToNext));
        }

        public static void ShuffleCollection()
        {
            var list = Ctr_Song.Current.GetAllSongsPaths();
            list.Shuffle();
            if (list.Count > 0)
                Play(list);
        }

        public static void ShuffleCurrentPlaylist()
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.Shuffle));
        }

        public static void RequestPlaylist()
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new ActionMessage(BackgroundAudioShared.Messages.Action.AskPlaylist));
        }

        public static void RemoveIndex(int index)
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new EditPlaylistMessage(Mode.Remove, index));
        }

        public static void SkipToIndex(int index)
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new JumpToIndexMessage(index));
        }

        public static void ClearPlaylist()
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new ClearPlaylistMessage());
        }

        public static void MoveItem(int originalIndex, int newIndex)
        {
            if (Current.CurrentPlayer != null)
                MessageService.SendMessageToBackground(new EditPlaylistMessage(Mode.DragAndDrop, originalIndex, newIndex));
        }
    }
}
