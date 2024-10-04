using Race_Element.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util.SystemExtensions;
using System.Collections.Concurrent;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.InputTrace;

[Overlay(
    Name = "Input Trace",
    Description = "Live graph of steering, throttle and brake inputs.",
    Version = 1.00,
    Authors = ["Reinier Klarenberg, Dirk Wolf"],
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Inputs
)]
internal sealed class InputTraceOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Input Trace")
{
    private readonly InputTraceConfiguration _config = new();
    private readonly ConcurrentQueue<InputsData> _dataQueue = [];
    private DataCollector _dataCollector;
    private InputGraph? _graph;

    public sealed override void BeforeStart()
    {
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
        this.RefreshRateHz = _config.Chart.HudRefreshRate;
        this.RefreshRateHz.ClipMax(_config.Data.Herz);

        _graph = new InputGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, this._config);

        for (int i = 0; i < _config.Chart.Width - 1; i++) _dataQueue.Enqueue(new(0, 0, 50));

        if (!IsPreviewing)
        {
            _dataCollector = new() { IntervalMillis = (int)(1000f / _config.Data.Herz) };
            _dataCollector.OnCollected += OnDataCollected;
            _dataCollector.Run();
        }
    }

    private void OnDataCollected(object? sender, InputsData e)
    {
        if (_dataQueue.Count >= _config.Chart.Width - 1)
            _dataQueue.TryDequeue(out InputsData _);

        _dataQueue.Enqueue(e);
    }

    public sealed override void BeforeStop()
    {
        if (!IsPreviewing)
        {
            _dataCollector.OnCollected -= OnDataCollected;
            _dataCollector.CancelJoin();
        }

        _graph?.Dispose();
    }

    public sealed override void Render(Graphics g) => _graph?.Draw(g, _dataQueue);
}

internal readonly record struct InputsData(int Throttle, int Brake, int Steering);
internal sealed class DataCollector : AbstractDataJob<InputsData>
{
    public sealed override InputsData Collect => new()
    {
        Throttle = (int)(SimDataProvider.LocalCar.Inputs.Throttle * 100f),
        Brake = (int)(SimDataProvider.LocalCar.Inputs.Brake * 100),
        Steering = (int)((SimDataProvider.LocalCar.Inputs.Steering + 1.0f) / (2 * 100)),
    };
}
