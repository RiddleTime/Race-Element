using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Diagnostics;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.AccSaveReplayReminder;

[Overlay(Name = "Save Replay Reminder",
Description = "Shows an overlay that will remind you to save the replay.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.All,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class SaveReplayReminderOverlay : AbstractOverlay
{
    private ReplaySettings _replaySettings;

    public SaveReplayReminderOverlay(Rectangle rectangle) : base(rectangle, "Save Replay Reminder")
    {
        Width = 200;
        Height = Width / 4;
    }

    public sealed override void BeforeStart()
    {
        _replaySettings = new();

        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
    }

    private void Instance_OnNewSessionStarted(object sender, DbRaceSession e)
    {
        Debug.WriteLine(e.UtcStart.ToString());
        var settings = _replaySettings.Get();
        int maxReplayDuration = settings.AutoSaveMinTimeSeconds;
        if (maxReplayDuration > 0)
        {
            DateTime targetTime = DateTime.UtcNow.AddSeconds(settings.AutoSaveMinTimeSeconds);
            Debug.WriteLine($"");
        }
    }

    public sealed override void BeforeStop()
    {
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
    }

    public override sealed void Render(Graphics g)
    {
    }
}
