using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util.SystemExtensions;
using ScottPlot;
using System;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaTrace;

[Overlay(Name = "Lap Delta Trace",
    Description = "Shows a graph of the lap delta.",
    OverlayCategory = OverlayCategory.Lap,
    OverlayType = OverlayType.Drive,
    Version = 1,
Authors = ["Reinier Klarenberg"])]
internal sealed class LapDeltaTraceOverlay : AbstractOverlay
{
    private readonly LapDeltaTraceConfiguration _config = new();
    private readonly LapDeltaDataCollector _collector;
    private readonly LapDeltaGraph _graph;

    public LapDeltaTraceOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Trace")
    {
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
        this.RefreshRateHz = _config.Chart.Herz;
        _collector = new LapDeltaDataCollector(_config.Chart.Width - 1)
        {
            MaxDelta = _config.Chart.MaxDelta,
        };
        _graph = new LapDeltaGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _collector, _config);
    }

    public sealed override void SetupPreviewData()
    {
        if (_collector != null)
        {
            _collector.PositiveDeltaData.Clear();
            _collector.NegativeDeltaData.Clear();

            var rand = new Random(_config.Preview.Seed);
            int walkingMultiplier = 100 / _config.Chart.Herz * 20;
            float[] data = DataGen.RandomWalk(rand, _config.Chart.Width * walkingMultiplier, 0.08f / 20, -1.8f / 20);
            for (int i = 0; i < data.Length; i += walkingMultiplier)
            {
                float dataPoint = data[i];
                dataPoint *= -0.5f;
                dataPoint.Clip(-_config.Chart.MaxDelta, _config.Chart.MaxDelta);
                _collector.Collect(dataPoint);
            }
        }
    }

    public sealed override void BeforeStop() => _graph?.Dispose();

    public sealed override bool ShouldRender()
    {
        if (_config.Chart.HideForRace && !this.IsRepositioning && pageGraphics.SessionType == ACCSharedMemory.AcSessionType.AC_RACE)
            return false;

        if (_config.Chart.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
            return true;

        return base.ShouldRender();
    }

    public sealed override void Render(Graphics g)
    {
        _collector?.Collect(GetDelta());
        _graph?.Draw(g);
    }

    private float GetDelta()
    {
        float delta = (float)pageGraphics.DeltaLapTimeMillis / 1000;

        if (_config.Chart.Spectator)
        {
            int focusedIndex = broadCastRealTime.FocusedCarIndex;
            if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, focusedIndex))
                lock (EntryListTracker.Instance.Cars)
                {
                    if (EntryListTracker.Instance.Cars.Count > 0)
                    {
                        var car = EntryListTracker.Instance.Cars.First(car => car.Key == focusedIndex);
                        delta = car.Value.RealtimeCarUpdate.Delta / 1000f;
                    }
                }
        }

        delta.Clip(-_config.Chart.MaxDelta, _config.Chart.MaxDelta);

        return delta;
    }
}
