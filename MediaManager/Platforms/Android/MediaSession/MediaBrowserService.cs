﻿using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Media;
using AndroidX.Media.Session;
using Com.Google.Android.Exoplayer2.UI;
using MediaManager.Platforms.Android.Media;

namespace MediaManager.Platforms.Android.MediaSession
{
    [Service(Exported = true, Enabled = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    [IntentFilter(new[] { global::Android.Service.Media.MediaBrowserService.ServiceInterface })]
    public class MediaBrowserService : MediaBrowserServiceCompat
    {
        protected MediaManagerImplementation MediaManager => CrossMediaManager.Android;
        protected MediaDescriptionAdapter MediaDescriptionAdapter { get; set; }
        protected PlayerNotificationManager PlayerNotificationManager
        {
            get => (MediaManager.Notification as Notifications.NotificationManager).PlayerNotificationManager;
            set => (MediaManager.Notification as Notifications.NotificationManager).PlayerNotificationManager = value;
        }
        protected MediaControllerCompat MediaController => MediaManager.MediaController;

        protected NotificationListener NotificationListener { get; set; }

        public readonly string ChannelId = "audio_channel";
        public readonly int ForegroundNotificationId = 1;
        public bool IsForeground = false;

        public MediaBrowserService()
        {
        }

        protected MediaBrowserService(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            try
            {
                PrepareMediaSession();
                PrepareNotificationManager();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            MediaManager.StateChanged += MediaManager_StateChanged;
        }

        private void MediaManager_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            switch (e.State)
            {
                case global::MediaManager.Player.MediaPlayerState.Failed:
                case global::MediaManager.Player.MediaPlayerState.Stopped:
                    if (IsForeground && MediaController.PlaybackState.State == PlaybackStateCompat.StateNone)
                    {
                        //ServiceCompat.StopForeground(this, ServiceCompat.StopForegroundRemove);
                        MediaManager.Logger?.LogInfo($"Stopping foreground service, {e.State}");
                        StopForeground(StopForegroundFlags.Remove);
                        StopSelf();
                        IsForeground = false;
                    }
                    break;
                case global::MediaManager.Player.MediaPlayerState.Paused:
                    if (IsForeground)
                    {
                        //ServiceCompat.StopForeground(this, ServiceCompat.StopForegroundDetach);
                        MediaManager.Logger?.LogInfo("Stopping foreground service, MediaPlayerState.Paused");
                        StopForeground(StopForegroundFlags.Detach);
                        //PlayerNotificationManager?.SetOngoing(false);
                        PlayerNotificationManager?.Invalidate();
                        IsForeground = false;
                    }
                    break;
                default:
                    break;
            }
        }

        protected virtual void PrepareMediaSession()
        {
            var mediaSession = MediaManager.MediaSession = new MediaSessionCompat(this, nameof(MediaBrowserService));
            mediaSession.SetSessionActivity(MediaManager.SessionActivityPendingIntent);
            mediaSession.Active = true;

            SessionToken = mediaSession.SessionToken;

            mediaSession.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons |
                                   MediaSessionCompat.FlagHandlesTransportControls);
        }

        protected virtual void PrepareNotificationManager()
        {
            MediaDescriptionAdapter = new MediaDescriptionAdapter();

            // Create notification channel for media controls.
            // if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            // {
            //     var channel = new NotificationChannel(ChannelId, MediaManager.NotificationChannel, NotificationImportance.Low);
            //     var nm = (NotificationManager)GetSystemService(NotificationService);
            //     nm.CreateNotificationChannel(channel);
            // }


            //Needed for enabling the notification as a mediabrowser.
            NotificationListener = new NotificationListener();
            NotificationListener.OnNotificationCancelledImpl = (notificationId, dismissedByUser) =>
            {
                MediaManager.Logger?.LogInfo($"Stopping foreground service, Notification cancelled. DismissedByUser: {dismissedByUser}");
                StopForeground(dismissedByUser ? StopForegroundFlags.Remove : StopForegroundFlags.Detach);
                //ServiceCompat.StopForeground(this, ServiceCompat.StopForegroundRemove);

                StopSelf();
                IsForeground = false;
            };
            NotificationListener.OnNotificationPostedImpl = (notificationId, notification, ongoing) =>
            {
                if (ongoing && !IsForeground)
                {
                    ContextCompat.StartForegroundService(ApplicationContext, new Intent(ApplicationContext, Java.Lang.Class.FromType(typeof(MediaBrowserService))));

                    // In case of Android 9 and below, just call StartForeground without ForegroundService.TypeMediaPlayback,
                    // since that is available from version 10 (API Level 29).
                    if (Build.VERSION.SdkInt <= BuildVersionCodes.P)
                    {
                        StartForeground(notificationId, notification);
                    }
                    else
                    {
                        StartForeground(notificationId, notification, ForegroundService.TypeMediaPlayback);
                    }

                    MediaManager.Logger?.LogInfo("Starting foreground service");
                    IsForeground = true;
                }
            };
            
            PlayerNotificationManager = new Com.Google.Android.Exoplayer2.UI.PlayerNotificationManager.Builder(
                this,
                ForegroundNotificationId,
                ChannelId)
                .SetChannelNameResourceId(Resource.String.XamarinMediaManagerDescription)
                .SetChannelDescriptionResourceId(Resource.String.XamarinMediaManagerDescription)

                .SetMediaDescriptionAdapter(MediaDescriptionAdapter)
                .SetNotificationListener(NotificationListener)
                .Build();

            

            //PlayerNotificationManager.SetFastForwardIncrementMs((long)MediaManager.StepSizeForward.TotalMilliseconds);
            //PlayerNotificationManager.SetRewindIncrementMs((long)MediaManager.StepSizeBackward.TotalMilliseconds);

            PlayerNotificationManager.SetMediaSessionToken(SessionToken);
            //PlayerNotificationManager.SetOngoing(true);
            PlayerNotificationManager.SetUsePlayPauseActions(MediaManager.Notification.ShowPlayPauseControls);
            //PlayerNotificationManager.SetUseNavigationActions(MediaManager.Notification.ShowNavigationControls);
            PlayerNotificationManager.SetSmallIcon(MediaManager.NotificationIconResource);
            PlayerNotificationManager.SetPriority(NotificationCompat.PriorityLow);

            //Must be called to start the connection
            (MediaManager.Notification as Notifications.NotificationManager).Player = MediaManager.Player;
            //PlayerNotificationManager.SetPlayer(MediaManager.AndroidMediaPlayer.Player);
        }

        public override StartCommandResult OnStartCommand(Intent startIntent, StartCommandFlags flags, int startId)
        {
            if (startIntent != null)
            {
                MediaButtonReceiver.HandleIntent(MediaManager.MediaSession, startIntent);
            }
            return StartCommandResult.Sticky;
        }

        public override async void OnTaskRemoved(Intent rootIntent)
        {
            MediaManager.Logger?.LogInfo($"Stopping foreground service, Task removed");
            StopForeground(StopForegroundFlags.Remove);
            await MediaManager.Stop();
            base.OnTaskRemoved(rootIntent);
        }

        public override void OnDestroy()
        {
            //ServiceCompat.StopForeground(this, ServiceCompat.StopForegroundDetach);

            MediaManager.Logger?.LogInfo($"Stopping foreground service, MediaBrowserService destroyed");
            StopForeground(StopForegroundFlags.Remove);
            MediaManager.StateChanged -= MediaManager_StateChanged;

            (MediaManager.Notification as Notifications.NotificationManager).Player = null;

            MediaDescriptionAdapter.Dispose();
            MediaDescriptionAdapter = null;

            // Service is being killed, so make sure we release our resources

            //TODO: Enable again
            //PlayerNotificationManager.SetNotificationListener(null);

            PlayerNotificationManager.SetPlayer(null);
            PlayerNotificationManager.Dispose();
            PlayerNotificationManager = null;

            NotificationListener.Dispose();
            NotificationListener = null;

            MediaManager.MediaSession.Active = false;
            MediaManager.MediaSession.Release();
            //MediaManager.MediaSession.Dispose();
            MediaManager.MediaSession = null;

            IsForeground = false;
            base.OnDestroy();
        }

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            return new BrowserRoot(nameof(ApplicationContext.ApplicationInfo.Name), null);
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            var mediaItems = new JavaList<MediaBrowserCompat.MediaItem>();

            foreach (var item in MediaManager.Queue)
                mediaItems.Add(item.ToMediaBrowserMediaItem());

            result.SendResult(mediaItems);
        }
    }
}
