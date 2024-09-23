using Newtonsoft.Json;
using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Diagnostics;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.AccSaveReplayReminder;

#if DEBUG
[Overlay(Name = "Save Replay Reminder",
Description = "Shows an overlay that will remind you to save the replay.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.All,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
#endif
internal sealed class SaveReplayReminderOverlay : AbstractOverlay
{
    private ReplaySettings _replaySettings;

    private DateTime _nextManualSaveUTC = DateTime.MinValue;

    private NextSave _nextSaveType = NextSave.Automatic;

    private enum NextSave
    {
        Automatic,
        Manual
    }

    private InfoPanel _panel;

    public SaveReplayReminderOverlay(Rectangle rectangle) : base(rectangle, "Save Replay Reminder")
    {
        Width = 300;
        Height = Width / 4;
    }

    public sealed override void BeforeStart()
    {
        _replaySettings = new();

        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

        _panel = new InfoPanel(10, 300);
    }

    private void Instance_OnNewSessionStarted(object sender, DbRaceSession e)
    {
        Debug.WriteLine(e.UtcStart.ToString());
        var settings = _replaySettings.Get();
        Debug.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));

        if (settings.AutoSaveEnabled == 0)
            _nextSaveType = NextSave.Manual;

        if (settings.MaxTimeReplaySeconds > 0)
        {
            _nextManualSaveUTC = DateTime.UtcNow.AddSeconds(settings.MaxTimeReplaySeconds);
            Debug.WriteLine($"Replay time limit at {_nextManualSaveUTC.ToLocalTime():HH\\:mm\\:ss}");
        }
    }

    public sealed override void BeforeStop()
    {
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
    }

    public sealed override void Render(Graphics g)
    {
        if (_nextManualSaveUTC != DateTime.MinValue)
            _panel.AddLine("Save replay before", $"{_nextManualSaveUTC.Subtract(DateTime.UtcNow):hh\\:mm\\:ss}");

        _panel.Draw(g);

    }
}
