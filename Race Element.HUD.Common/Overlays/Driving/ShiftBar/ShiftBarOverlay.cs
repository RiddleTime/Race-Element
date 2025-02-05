using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;

[Overlay(
    Name = "Shift Bar",
    Description = "A lightweight RPM Bar. Can render up to 200 Hz for some simulators.",
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class ShiftBarOverlay : CommonAbstractOverlay
{
    private readonly ShiftBarConfiguration _config = new();
    private readonly InfoPanel _upshiftDataPanel;

    private CachedBitmap _cachedBackground;
    private CachedBitmap _cachedRpmLines;
    private CachedBitmap _cachedFlashBar;
    private CachedBitmap[] _cachedPitLimiter;

    private readonly List<CachedBitmap> _cachedColorBars = [];
    private readonly List<(float percentage, Color color)> _colors = [];

    private readonly Rectangle WorkingSpace;
    private RectangleF BarSpace;

    private record struct ShiftBarDataModel(int Rpm, int MaxRpm);
    private ShiftBarDataModel _model = new(0, 0);

    private MaxRpmDetectionJob _maxRpmDetectionJob;

    private TimeSpan _redlineTime = TimeSpan.FromMilliseconds(64);
    private TimeSpan _flashTime = TimeSpan.FromMilliseconds(32);

    private readonly TimeSpan _pimiterLimiterFlipTime = TimeSpan.FromMilliseconds(128);
    public ShiftBarOverlay(Rectangle rectangle) : base(rectangle, "Shift Bar")
    {
        RefreshRateHz = _config.Bar.RefreshRate;
        WorkingSpace = new(0, 0, _config.Bar.Width, _config.Bar.Height);
        Width = (int)WorkingSpace.Width + 1;
        Height = (int)WorkingSpace.Height + 1;

        // increase base height and clipmin width to support upshift data panel
        if (_config.Upshift.DrawUpshiftData)
        {
            int panelMinWidth = 200;
            if (Width < panelMinWidth)
                Width = panelMinWidth;
            _upshiftDataPanel = new(11, panelMinWidth) { Y = Height + 1, FirstRowLine = 0 };
            Height += 3 * _upshiftDataPanel.FontHeight + 1;
        }
    }

    public sealed override void SetupPreviewData() => _model = new(_config.Upshift.PreviewRpm, _config.Upshift.MaxPreviewRpm);

    private (float earlyPercentage, float redlinePercentage) GetUpShiftPercentages()
    {
        // configured percentages (0-100%)
        float earlyPercentage = _config.Upshift.EarlyPercentage;
        float upshiftPercentage = _config.Upshift.RedlinePercentage;

        if (GameWhenStarted.HasFlag(Game.RaceRoom | Game.iRacing))
        {
            float maxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;
            float upshiftRpm = SimDataProvider.LocalCar.Engine.ShiftUpRpm;
            if (maxRpm > 0 && upshiftRpm > 0)
            {
                upshiftPercentage = upshiftRpm * 100 / maxRpm;
                earlyPercentage = upshiftPercentage * 0.96f;
            }
        }
        return (earlyPercentage, upshiftPercentage);
    }

    private void UpdateColorDictionary()
    {
        var percentages = GetUpShiftPercentages();

        _colors.Clear();
        _colors.Add((0f, Color.FromArgb(255, _config.Colors.NormalColor)));
        _colors.Add((percentages.earlyPercentage / 100f, Color.FromArgb(255, _config.Colors.EarlyColor)));
        _colors.Add((percentages.redlinePercentage / 100f, Color.FromArgb(255, _config.Colors.RedlineColor)));
    }
    public sealed override void BeforeStart()
    {
        UpdateColorDictionary();

        // config settings 
        if (_config.RedlineFlash.Enabled)
        {
            int redlineMs = _config.RedlineFlash.MillisecondsRedline;
            int flashMs = _config.RedlineFlash.MillisecondsFlash;
            redlineMs.Clip(16, 700);
            flashMs.Clip(16, 800);
            _redlineTime = TimeSpan.FromMilliseconds(redlineMs);
            _flashTime = TimeSpan.FromMilliseconds(flashMs);
        }

        int horizontalBarPadding = 2;
        int verticalBarPadding = 2;
        BarSpace = new(horizontalBarPadding, verticalBarPadding, WorkingSpace.Width - horizontalBarPadding * 2, WorkingSpace.Height - verticalBarPadding * 2);

        _cachedBackground = ShiftBarShapes.CreateBackground(WorkingSpace);

        _cachedColorBars.Clear();
        for (int i = 0; i < _colors.Count; i++)
            _cachedColorBars.Add(ShiftBarShapes.CreateColoredBar(WorkingSpace, BarSpace, _colors.ElementAt(i).color));

        _cachedFlashBar = ShiftBarShapes.CreateFlashBar(WorkingSpace, BarSpace, _config);
        _cachedRpmLines = new(WorkingSpace.Width, WorkingSpace.Height, g =>
        {
            var model = _model;
            if (model.MaxRpm <= 0) return;
            _config.Data.HideRpm = model.MaxRpm - _config.Data.VisibleRpmAmount;
            _config.Data.HideRpm.ClipMin(_config.Data.MinVisibleRpm);

            int totalRpm = model.MaxRpm - _config.Data.HideRpm;
            totalRpm.ClipMin(_config.Data.MinVisibleRpm);

            int lineCount = (int)Math.Floor(totalRpm / 1000d);

            int leftOver = model.MaxRpm % 1000;
            if (leftOver < 70 && leftOver != 0)
                lineCount--;

            //Debug.WriteLine($"leftover: {leftOver}, LC: {lineCount}, visible RPM: {totalRpm}");
            lineCount.ClipMin(0);
            if (lineCount == 0) return;
            using SolidBrush brush = new(Color.FromArgb(220, Color.Black));
            using Pen linePen = new(brush, 1.6f);

            for (int i = 1; i <= lineCount; i++)
            {
                int targetRpm = model.MaxRpm - (i * 1000) - leftOver;
                double adjustedPercent = GetAdjustedPercentToHideRpm(targetRpm, model.MaxRpm, _config.Data.HideRpm, _config.Data.MinVisibleRpm);
                float x = (float)(BarSpace.X + (BarSpace.Width * adjustedPercent));
                g.DrawLine(linePen, x, 2, x, _config.Bar.Height - 2);
            }

            if (_config.Data.RedlineMarker)
            {
                var (earlyPercentage, redlinePercentage) = GetUpShiftPercentages();
                double adjustedPercent = GetAdjustedPercentToHideRpm((int)(model.MaxRpm * redlinePercentage / 100), model.MaxRpm, _config.Data.HideRpm, _config.Data.MinVisibleRpm);
                float x = (float)(BarSpace.X + (BarSpace.Width * adjustedPercent));
                g.DrawLine(Pens.Red, x, 4, x, _config.Bar.Height - 4);
            }
        });

        if (_config.Pitlimiter.Enabled)
            _cachedPitLimiter = ShiftBarShapes.CreatePitLimiter(WorkingSpace, BarSpace);

        if (!IsPreviewing)
        {
            _maxRpmDetectionJob = new(this) { IntervalMillis = 1000 };
            _maxRpmDetectionJob.Run();
        }
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _cachedRpmLines?.Dispose();
        _cachedFlashBar?.Dispose();
        if (_cachedPitLimiter != null)
            foreach (var item in _cachedPitLimiter) item.Dispose();

        foreach (var item in _cachedColorBars) item.Dispose();
        _cachedColorBars.Clear();

        _maxRpmDetectionJob?.CancelJoin();
        _upshiftDataPanel?.Dispose();
    }

    // demo stuff
    private int shiftsDone = 0;
    private bool up = true;
    // demo stuff

    public sealed override void Render(Graphics g)
    {

        if (!IsPreviewing)
        {   // SET MODEL: Before release, uncomment line below and remove everything in-between the test data. it emulates the rpm going up 
            _model.Rpm = SimDataProvider.LocalCar.Engine.Rpm;
            _model.MaxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;

            // test data    ------------
            //_model.MaxRpm = 10000;
            //if (_model.Rpm < _model.MaxRpm / 3) _model.Rpm = _model.MaxRpm / 3;
            //int increment = Random.Shared.Next(0, 2) == 1 ? Random.Shared.Next(0, 43) : -7;
            //if (!up) increment *= -4;
            //_model.Rpm = _model.Rpm + increment;
            //if (up && _model.Rpm > _model.MaxRpm)
            //{
            //    _model.Rpm = _model.MaxRpm - _model.MaxRpm / 4;
            //    shiftsDone++;
            //}
            //if (!up && _model.Rpm < _model.MaxRpm * _config.Upshift.EarlyPercentage / 100f - _model.MaxRpm / 5f)
            //{
            //    _model.Rpm = _model.MaxRpm;
            //    shiftsDone++;
            //}
            //if (shiftsDone > 3)
            //{
            //    up = !up;
            //    shiftsDone = 0;
            //}

            // test data    ------------

            if (_model.Rpm < 0) _model.Rpm = 0;
            if (_model.Rpm > _model.MaxRpm) _model.Rpm = _model.MaxRpm;
        }

        _cachedBackground?.Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);
        DrawBar(g);
        _cachedRpmLines.Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);


        if (_config.Pitlimiter.Enabled && SimDataProvider.LocalCar.Engine.IsPitLimiterOn)
            DrawPitLimiter(g);

        if (_config.Upshift.DrawUpshiftData)
        {
            _upshiftDataPanel.AddLine("Early", $"{_model.MaxRpm * _config.Upshift.EarlyPercentage / 100d:F1}");
            _upshiftDataPanel.AddLine("Redline", $"{_model.MaxRpm * _config.Upshift.RedlinePercentage / 100d:F1}");
            _upshiftDataPanel.AddLine("Max", $"{_model.MaxRpm:F1}");
            _upshiftDataPanel.Draw(g);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentRpm"></param>
    /// <param name="maxRpm"></param>
    /// <param name="hideRpm"></param>
    /// <permission cref="<see cref="Reinier Klarenberg"/>"
    /// <returns></returns>
    private static double GetAdjustedPercentToHideRpm(int currentRpm, int maxRpm, int hideRpm, int minVisibleRpm)
    {
        if (hideRpm > maxRpm - minVisibleRpm)
            hideRpm = maxRpm - minVisibleRpm;

        return (double)(currentRpm - hideRpm) / (maxRpm - hideRpm);
    }

    private bool _flashFlip;
    private long _lastFlash;
    private void DrawBar(Graphics g)
    {
        double rpmPercentage = 0;
        if (_model.Rpm > 0 && _model.MaxRpm > 0) rpmPercentage = (double)_model.Rpm / _model.MaxRpm;
        if (rpmPercentage < 0.02f) return;

        double adjustedPercent = GetAdjustedPercentToHideRpm(_model.Rpm, _model.MaxRpm, _config.Data.HideRpm, _config.Data.MinVisibleRpm);
        adjustedPercent.Clip(0.02f, 1);
        RectangleF percented = new(BarSpace.ToVector4());
        percented.Width = (float)(BarSpace.Width * adjustedPercent);

        int barIndex = GetCurrentColorBarIndex(rpmPercentage);
        var cachedColorSpan = CollectionsMarshal.AsSpan(_cachedColorBars);
        g.SetClip(percented);
        if (barIndex < _colors.Count - 1)
            cachedColorSpan[barIndex].Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);
        else
        {
            (_flashFlip && _config.RedlineFlash.Enabled ? _cachedFlashBar : cachedColorSpan[barIndex]).Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);

            if (_config.RedlineFlash.Enabled && TimeProvider.System.GetElapsedTime(_lastFlash) > (_flashFlip ? _redlineTime : _flashTime))
            {
                _flashFlip = !_flashFlip;
                _lastFlash = TimeProvider.System.GetTimestamp();
            }
        }
        g.ResetClip();
    }

    private int GetCurrentColorBarIndex(double percentage)
    {
        int barIndex = 0;
        var colorSpan = CollectionsMarshal.AsSpan(_colors);
        for (int i = 0; i < colorSpan.Length; i++)
        {
            if (percentage < colorSpan[i].percentage) break;
            barIndex = i;
        }
        return barIndex;
    }

    private bool _pitLimiterFlip;
    private long _lastPitLimiterFlip;
    private void DrawPitLimiter(Graphics g)
    {
        _cachedPitLimiter.AsSpan()[_pitLimiterFlip ? 0 : 1].Draw(g, 0, 0, WorkingSpace.Width, WorkingSpace.Height);

        if (TimeProvider.System.GetElapsedTime(_lastPitLimiterFlip) > _pimiterLimiterFlipTime)
        {
            _pitLimiterFlip = !_pitLimiterFlip;
            _lastPitLimiterFlip = TimeProvider.System.GetTimestamp();
        }
    }

    private sealed class MaxRpmDetectionJob(ShiftBarOverlay shiftBarOverlay) : AbstractLoopJob
    {
        private int _lastMaxRpm = -1;
        public sealed override void RunAction()
        {
            var model = shiftBarOverlay._model;
            if (_lastMaxRpm != model.MaxRpm)
            {
                shiftBarOverlay?._cachedRpmLines.Render();
                shiftBarOverlay?.UpdateColorDictionary();
                _lastMaxRpm = model.MaxRpm;
            }
        }
    }
}
