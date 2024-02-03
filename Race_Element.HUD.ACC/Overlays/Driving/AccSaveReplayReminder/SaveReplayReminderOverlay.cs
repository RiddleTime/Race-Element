using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.AccSaveReplayReminder;

[Overlay(Name = "Save Replay Reminder",
Description = "Shows an overlay that will remind you to save the replay.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.All,
Version = 1.00)]
internal sealed class SaveReplayReminderOverlay : AbstractOverlay
{

    private ReplaySettings _replaySettings;

    public SaveReplayReminderOverlay(Rectangle rectangle) : base(rectangle, "Save Replay Reminder")
    {

    }

    public sealed override void BeforeStart()
    {
        _replaySettings = new();
        var settings = _replaySettings.Get();

        int maxReplayDuration = settings.AutoSaveMinTimeSeconds;


        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

    }

    private void Instance_OnNewSessionStarted(object sender, DbRaceSession e)
    {
        Debug.WriteLine(e.UtcStart.ToString());
    }

    public sealed override void BeforeStop()
    {
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
    }

    public override sealed void Render(Graphics g)
    {
    }
}
