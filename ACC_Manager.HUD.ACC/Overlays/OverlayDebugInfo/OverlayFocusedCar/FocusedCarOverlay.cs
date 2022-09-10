using ACCManager.Data.ACC.EntryList;
using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.ACC.EntryList.EntryListTracker;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayFocusedCar
{
    [Overlay(Name = "Focused Car", Description = "Shows information regarding the focused car",
        OverlayType = OverlayType.Debug, Version = 1.00)]
    internal class FocusedCarOverlay : AbstractOverlay
    {
        public FocusedCarOverlay(Rectangle rectangle) : base(rectangle, "Focused Car Overlay")
        {
            this.Width = 600;
            this.Height = 600;
            this.RefreshRateHz = 10;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        private float minX = float.MaxValue, maxX = float.MinValue;
        private float minY = float.MaxValue, maxY = float.MinValue;

        private LinkedList<PointF> _trajectory = new LinkedList<PointF>();
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


                    float x = carData.Value.RealtimeCarUpdate.WorldPosX;
                    float y = carData.Value.RealtimeCarUpdate.WorldPosY;

                    int arrayIndex = -1;
                    for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                    {
                        if (pageGraphics.CarIds[i] == lastFocused)
                        {
                            arrayIndex = i;
                            break;
                        }
                    }

                    x = pageGraphics.CarCoordinates[arrayIndex].X;
                    y = pageGraphics.CarCoordinates[arrayIndex].Z;


                    // x, y > min =  left, top

                    if (x > maxX)
                        maxX = x;
                    if (x < minX)
                        minX = x;

                    if (y > maxY)
                        maxY = y;
                    if (y < minY)
                        minY = y;

                    _trajectory.AddLast(new PointF(x, y));
                }
            }

            if (_trajectory.Count > 0)
            {
                GraphicsPath path = new GraphicsPath(FillMode.Winding);

                int halfWidth = this.Width / 2;
                int halfHeight = this.Height / 2;

                float maxSize = 0;
                if (minX * -1 > maxSize)
                    maxSize = minX * -1;
                if (maxX > maxSize)
                    maxSize = maxX;
                if (minY * -1 > maxSize)
                    maxSize = minY * -1;
                if (maxY > maxSize)
                    maxSize = maxY;

                maxSize *= 1.1f;



                var traj = _trajectory.Select(x =>
                {
                    x.X /= maxSize;
                    x.Y /= maxSize;
                    return new PointF(halfWidth + x.X * halfWidth, halfHeight + x.Y * halfHeight);
                }).ToArray();
                path.AddLines(traj);

                Matrix transformMatrix = new Matrix();
                transformMatrix.RotateAt(-90, new PointF(halfWidth, halfHeight));
                path.Transform(transformMatrix);
                Pen pen = new Pen(Color.OrangeRed, 1.5f);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(pen, path);

            }
        }

        public override bool ShouldRender()
        {
            return true;
        }
    }
}
