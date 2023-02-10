using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using MediaManager.Platforms.Android.MediaSession;
using MediaManager.Playback;
using MediaManager.Player;

namespace AndroidPlayerSample;

[Service(Exported = true, Enabled = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
[IntentFilter(new[] { global::Android.Service.Media.MediaBrowserService.ServiceInterface })]
public class CustomMediaBrowserService : MediaBrowserService
{
    public override void OnCreate()
    {
        base.OnCreate();

        this.MediaManager.StateChanged += MediaManagerOnStateChanged;
    }

    private void MediaManagerOnStateChanged(object sender, StateChangedEventArgs e)
    {
        if (e.State == MediaPlayerState.Playing)
        {
            ActivityManager manager = (ActivityManager)GetSystemService(Context.ActivityService);
            foreach (ActivityManager.RunningServiceInfo service in manager.GetRunningServices(int.MaxValue))
            {
                var name = service.Service?.ClassName;

            }

        }
    }

    public override void OnDestroy()
    {
        MediaManager.StateChanged -= MediaManagerOnStateChanged;
        try
        {
            base.OnDestroy();
        }
        catch (Exception ex)
        {

        }
    }
}
