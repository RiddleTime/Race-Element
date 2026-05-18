using Race_Element.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util.SystemExtensions;
using System.Collections.Concurrent;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Flight.ControlsTrace;

[Overlay(
    Name = "Controls Trace",
    Description = "Live graph of Aileron, Elevator and Rudder Controls.",
    Version = 1.00,
    Authors = ["Reinier Klarenberg"],
    OverlayCategory = OverlayCategory.Inputs,
    SupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024
)]
internal sealed class ControlsTraceOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Controls Trace")
{
    private readonly ControlsTraceConfiguration _config = new();
    private readonly ConcurrentQueue<ControlsData> _dataQueue = [];
    private DataCollector? _dataCollector;
    private ControlsGraph? _graph;

    public sealed override void BeforeStart()
    {
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
        this.RefreshRateHz = _config.Chart.HudRefreshRate;
        this.RefreshRateHz.ClipMax(_config.Data.Herz);

        _graph = new ControlsGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, this._config);

        for (int i = 0; i < _config.Chart.Width - 1; i++) _dataQueue.Enqueue(new(71, 50, 39));

        if (!IsPreviewing)
        {
            _dataCollector = new() { IntervalMillis = (int)(1000f / _config.Data.Herz) };
            _dataCollector.OnCollected += OnNewData;
            _dataCollector.Run();
        }
    }

    private void OnNewData(object? sender, ControlsData e)
    {
        if (_dataQueue.Count >= _config.Chart.Width - 1)
            _dataQueue.TryDequeue(out ControlsData _);

        _dataQueue.Enqueue(e);
    }

    public sealed override void BeforeStop()
    {
        if (!IsPreviewing && _dataCollector != null)
        {
            _dataCollector.OnCollected -= OnNewData;
            _dataCollector.CancelJoin();
        }

        _graph?.Dispose();
    }

    public sealed override void Render(Graphics g) => _graph?.Draw(g, _dataQueue);
}

internal readonly record struct ControlsData(double Aileron, double Elevator, double Rudder);
internal sealed class DataCollector : AbstractCollectionJob<ControlsData>
{
    public sealed override ControlsData Collect => new()
    {
        Aileron = (SimDataProvider.LocalPlane.Controls.AileronPosition + 1.0) / 2.0 * 100.0,
        Elevator = (SimDataProvider.LocalPlane.Controls.ElevatorPosition + 1.0) / 2.0 * 100.0,
        Rudder = (SimDataProvider.LocalPlane.Controls.RudderPosition + 1.0) / 2.0 * 100.0,
    };
}
