using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Diagnostics.Metrics;

namespace RaceElement.HUD.ACC.Overlays.OverlayAccelerometer;

[Overlay(Name = "Accelerometer", Version = 1.00, OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Physics,
    Description = "G-meter showing lateral and longitudinal g-forces.")]
internal sealed class AccelerometerOverlay : AbstractOverlay
{
    private readonly AccelleroConfig _config = new();
    private sealed class AccelleroConfig : OverlayConfiguration
    {
        [ConfigGrouping("Accelerometer", "Additional options for the Accelerometer")]
        public AccelerometerGrouping Accelerometer { get; init; } = new();
        public class AccelerometerGrouping
        {
            [ToolTip("Displays fading dots representing history of the g-forces.")]
            public bool HistoryTrace { get; init; } = true;

            [ToolTip("Displays the lateral and longitudinal g-forces as text.")]
            public bool GText { get; init; } = false;
        }

        public AccelleroConfig()
        {
            this.AllowRescale = true;
        }
    }

    private CachedBitmap _cachedBackground;
    private InfoPanel _panel;
    private const int MaxG = 3;
    private int _gMeterX = 0;
    private int _gMeterY = 0;
    private readonly int _gMeterSize = 200;
    private readonly LinkedList<Point> _trace = new();

    public AccelerometerOverlay(Rectangle rectangle) : base(rectangle, "Accelerometer") { }


    public override void SetupPreviewData()
    {
        pagePhysics.AccG[0] = 0f;
        pagePhysics.AccG[2] = 0f;
    }

    public sealed override void BeforeStart()
    {
        this.RefreshRateHz = 20;

        this.Width = _gMeterSize;
        this.Height = this.Width;

        _panel = new InfoPanel(10, 62) { DrawBackground = false, DrawRowLines = false, X = _gMeterSize / 2 - 28, Y = _gMeterSize - 34 };

        RenderBackgroundBitmap();
    }

    private void RenderBackgroundBitmap()
    {
        int size = _gMeterSize;

        if (Scale != 1)
            size = (int)Math.Floor(size * Scale);

        _cachedBackground = new CachedBitmap(size, size, g =>
        {
            int x = 0;
            int y = 0;

            float partSize = (float)Math.Floor(size / 3f);

            using GraphicsPath gradientPath = new();
            gradientPath.AddEllipse(new Rectangle(0, 0, size, size));
            using PathGradientBrush pthGrBrush = new(gradientPath);
            pthGrBrush.CenterPoint = new Point(size / 2, size / 2);
            pthGrBrush.CenterColor = Color.FromArgb(40, Color.Black);

            Rectangle inner = new((int)(x + (partSize * 1.5f) - partSize / 4), (int)(y + (partSize * 1.5f) - partSize / 4), (int)(size - partSize * 2 - partSize / 2), (int)(size - partSize * 2 - partSize / 2));
            pthGrBrush.SurroundColors = [Color.FromArgb(225, 10, 10, 25)];
            g.DrawArc(new Pen(pthGrBrush, partSize / 2 + 1), inner, 0, 360);
            g.DrawArc(new Pen(Color.FromArgb(15, Color.DarkOliveGreen), partSize / 2), inner, 60, 60);

            Rectangle middle = new((int)(x + partSize - partSize / 4), (int)(y + partSize - partSize / 4), (int)(size - partSize - partSize / 2), (int)(size - partSize - partSize / 2));
            pthGrBrush.SurroundColors = [Color.FromArgb(185, 10, 20, 10)];
            g.DrawArc(new Pen(pthGrBrush, partSize / 2), middle, 0, 360);
            g.DrawArc(new Pen(Color.FromArgb(9, Color.DarkRed), partSize / 2), middle, 250, 40);
            g.DrawArc(new Pen(Color.FromArgb(15, Color.LimeGreen), partSize / 2), middle, 60, 60);

            Rectangle outer = new((int)(x + partSize / 4), (int)(y + partSize / 4), (int)(size - partSize / 2), (int)(size - partSize / 2));
            outer.X += 1;
            outer.Y += 1;
            outer.Width -= 2;
            outer.Height -= 2;
            pthGrBrush.SurroundColors = [Color.FromArgb(225, 15, 10, 10)];
            g.DrawArc(new Pen(pthGrBrush, partSize / 2), outer, 120, 300);
            g.DrawArc(new Pen(Color.FromArgb(15, Color.Red), partSize / 2), outer, 250, 40);
        });
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g, _gMeterX, _gMeterY, _gMeterSize, _gMeterSize);

        if (this._config.Accelerometer.GText)
        {
            string x = $"{pagePhysics.AccG[0]:F2}".FillStart(5, ' ');
            string y = $"{pagePhysics.AccG[2]:F2}".FillStart(5, ' ');
            _panel.AddLine("X ", x);
            _panel.AddLine("Y ", y);

            _panel.Draw(g);
        }

        DrawGMeter(_gMeterX, _gMeterY, _gMeterSize, g);
    }

    /// <summary>
    /// Returns a percentage, mininum -100% and max 100%
    /// </summary>
    /// <param name="max"></param>
    /// <param name="value"></param>
    /// <returns>a value between -1 and 1 (inclusive)</returns>
    public float GetPercentage(float max, float value)
    {
        float percentage = value * 100 / max / 100;
        percentage.Clip(-1, 1);
        return percentage;
    }

    private void DrawGMeter(int x, int y, int size, Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        //Draws the 'dot'
        int gDotSize = 14;

        double xPercentage = GetPercentage(MaxG, pagePhysics.AccG[0]);
        double yPercentage = GetPercentage(MaxG, pagePhysics.AccG[2]);

        double direction = Math.Atan2(xPercentage, yPercentage);
        double magnitude = Math.Sqrt(xPercentage * xPercentage + yPercentage * yPercentage);
        magnitude.Clip(-1, 1);

        double horizontalPlacement = Math.Sin(direction) * magnitude;
        double verticalPlacement = Math.Cos(direction) * magnitude;
        verticalPlacement.Clip(-1, 1);
        horizontalPlacement.Clip(-1, 1);

        PointF middle = new(x + size / 2, y + size / 2);
        int gDotPosX = (int)(middle.X + (size / 2 * horizontalPlacement) - (gDotSize / 2));
        int gDotPosY = (int)(middle.Y + (size / 2 * verticalPlacement) - (gDotSize / 2));


        g.FillEllipse(new SolidBrush(Color.FromArgb(20, 255, 255, 255)), new Rectangle(gDotPosX, gDotPosY, gDotSize, gDotSize));

        if (_config.Accelerometer.HistoryTrace)
        {
            lock (_trace)
            {
                _trace.AddFirst(new Point(gDotPosX, gDotPosY));
                if (_trace.Count > 10)
                    _trace.RemoveLast();


                for (int i = 0; i < _trace.Count; i++)
                {
                    Point traceItem = _trace.ElementAt(i);
                    g.FillEllipse(new SolidBrush(Color.FromArgb(90 - i * 5, 85, 85, 85)), new Rectangle(traceItem.X, traceItem.Y, gDotSize, gDotSize));
                }
            }
        }

        g.FillEllipse(new SolidBrush(Color.FromArgb(170, 255, 255, 255)), new Rectangle(gDotPosX, gDotPosY, gDotSize, gDotSize));

    }

}
