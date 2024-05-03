using Gma.System.MouseKeyHook;
using RaceElement.Data.ACC.EntryList;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;

namespace RaceElement.HUD.ACC.Overlays.Pitwall;

[Overlay(Name = "Positions Timestamp", Description = "Shows the positions at a timestamp when the hotkey was pressed.\nPress hotkey to reset and hide hud again.")]
internal sealed class PositionsTimestamp : AbstractOverlay
{

    private readonly PositionsTimestampConfiguration _config = new();
    private sealed class PositionsTimestampConfiguration : OverlayConfiguration
    {
        public PositionsTimestampConfiguration() => this.GenericConfiguration.AllowRescale = false;
    }

    private IKeyboardMouseEvents m_GlobalHook;
    private DateTime TimeStamped;
    private List<KeyValuePair<int, CarData>> TimeStampedCars = new();

    public PositionsTimestamp(Rectangle rectangle) : base(rectangle, "Positions Timestamp")
    {
        RefreshRateHz = 2;
        TimeStamped = DateTime.MinValue;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        m_GlobalHook = Hook.GlobalEvents();
        m_GlobalHook.OnCombination(new Dictionary<Combination, Action> {
        {
            Combination.FromString("Control+S"), () => {
                if (broadCastRealTime.SessionType == Broadcast.RaceSessionType.Race)
                    if (TimeStamped == DateTime.MinValue){
                        TimeStampedCars = EntryListTracker.Instance.Cars;
                        TimeStamped = DateTime.UtcNow;
                    } else {
                        TimeStamped = DateTime.MinValue;
                        TimeStampedCars.Clear();
                    }
            }
        }});
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;
        m_GlobalHook?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        List<KeyValuePair<int, CarData>> cars = EntryListTracker.Instance.Cars;

        if (cars.Count == 0)
            return;

        cars = cars.OrderBy(x => x.Value.RealtimeCarUpdate.Position).ToList();
        for (int i = 0; i < cars.Count; i++)
        {
            Debug.WriteLine($"");
        }
    }
}
