using RaceElement.Data.ACC.EntryList;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayFocusedCar;

[Overlay(Name = "Focused Car", Description = "Shows information regarding the focused car",
    OverlayType = OverlayType.Pitwall, Version = 1.00)]
internal class FocusedCarOverlay : AbstractOverlay
{
    private FocusedCarConfig _config = new();
    private class FocusedCarConfig : OverlayConfiguration
    {
        public FocusedCarConfig()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    public FocusedCarOverlay(Rectangle rectangle) : base(rectangle, "Focused Car")
    {
        this.Width = 600;
        this.Height = 600;
        this.RefreshRateHz = 10;
    }

    private float minX = float.MaxValue, maxX = float.MinValue;
    private float minY = float.MaxValue, maxY = float.MinValue;

    private List<PointF> _trajectory = new();
    private int lastFocused = -1;

    public override void Render(Graphics g)
    {

        foreach (KeyValuePair<int, CarData> carData in EntryListTracker.Instance.Cars)
        {
            if (carData.Key == broadCastRealTime.FocusedCarIndex)
            {
                if (lastFocused != carData.Key)
                {
                    _trajectory.Clear();
                    lastFocused = carData.Key;
                    minX = float.MaxValue; maxX = float.MinValue;
                    minY = float.MaxValue; maxY = float.MinValue;
                }

                int arrayIndex = -1;
                for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                {
                    if (pageGraphics.CarIds[i] == lastFocused)
                    {
                        arrayIndex = i;
                        break;
                    }
                }

                if (arrayIndex != -1)
                {
                    float x = pageGraphics.CarCoordinates[arrayIndex].X;
                    float y = pageGraphics.CarCoordinates[arrayIndex].Z;

                    // x, y > min =  left, top

                    if (x > maxX)
                        maxX = x;
                    if (x < minX)
                        minX = x;

                    if (y > maxY)
                        maxY = y;
                    if (y < minY)
                        minY = y;

                    _trajectory.Add(new PointF(x, y));
                }
            }
        }

        if (_trajectory.Count > 0)
        {
            float maxSize = 0;
            if (minX * -1 > maxSize)
                maxSize = minX * -1;
            if (maxX > maxSize)
                maxSize = maxX;
            if (minY * -1 > maxSize)
                maxSize = minY * -1;
            if (maxY > maxSize)
                maxSize = maxY;

            maxSize *= 1.05f;

            int halfWidth = (int)(this.Width / 2 / this.Scale);
            int halfHeight = (int)(this.Height / 2 / this.Scale);

            var traj = _trajectory.Select(x =>
            {
                x.X /= maxSize;
                x.Y /= maxSize;
                return new PointF(halfWidth + x.X * halfWidth, halfHeight + x.Y * halfHeight);
            }).ToArray();
            GraphicsPath path = new(FillMode.Winding);
            path.AddLines(traj);

            Matrix transformMatrix = new();
            transformMatrix.RotateAt(-90, new PointF(halfWidth, halfHeight));
            path.Transform(transformMatrix);

            Pen pen = new(Color.OrangeRed, 2f);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawPath(pen, path);

        }
    }

    public sealed override bool ShouldRender() => true;
}
