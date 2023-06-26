using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlaySpotter
{
    [Overlay(Name = "Spotter",
        Description = "Alpha! (A basic car spotter) (no rescaling for now)",
        Version = 1.00,
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Driving
    )]
    internal sealed class SpotterOverlay : AbstractOverlay
    {
        private readonly SpotterConfiguration _config = new SpotterConfiguration();
        private class SpotterConfiguration : OverlayConfiguration
        {
            public SpotterConfiguration() => AllowRescale = false;
        }

        public SpotterOverlay(Rectangle rectangle) : base(rectangle, "Spotter")
        {
            Width = 200;
            Height = 400;
            RefreshRateHz = 10;
        }

        public override bool ShouldRender()
        {

            if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        private struct SpottableCar
        {
            public int Index;
            public float X;
            public float Y;
            public float Heading;
            public float Distance;
        }

        public override void Render(Graphics g)
        {

            try
            {
                if (!EntryListTracker.Instance.Cars.Any())
                    return;

                int playerID = broadCastRealTime.FocusedCarIndex;
                var playerCar = EntryListTracker.Instance.Cars.FirstOrDefault(car => car.Key == playerID);
                if (playerCar.Value == null) return;

                float playerX = 0;
                float playerY = 0;
                float playerHeading = playerCar.Value.RealtimeCarUpdate.Heading;

                List<SpottableCar> spottables = new List<SpottableCar>();

                // find local player id
                for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                {
                    if (pageGraphics.CarIds[i] == playerID)
                    {
                        playerX = pageGraphics.CarCoordinates[i].Z;
                        playerY = -pageGraphics.CarCoordinates[i].X;
                    }
                }

                // find spottable cars
                for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                {
                    if (pageGraphics.CarIds[i] != playerID && pageGraphics.CarIds[i] != 0)
                    {
                        float x = pageGraphics.CarCoordinates[i].Z;
                        float y = -pageGraphics.CarCoordinates[i].X;

                        float distance = DistanceBetween(playerX, playerY, x, y);

                        if (distance < 40)
                        {
                            var spottable = new SpottableCar()
                            {
                                Index = pageGraphics.CarIds[i],
                                X = x,
                                Y = y,
                                //Heading = heading,
                                Distance = distance,
                            };

                            var car = EntryListTracker.Instance.Cars.FirstOrDefault(x => x.Key == spottable.Index);
                            if (car.Value != null)
                                spottable.Heading = car.Value.RealtimeCarUpdate.Heading;

                            if (!spottables.Contains(spottable))
                                spottables.Add(spottable);
                        }
                    }
                }

                if (!spottables.Any())
                    return;

                g.FillRectangle(new SolidBrush(Color.FromArgb(110, Color.Black)), new Rectangle(0, 0, Width, Height));


                // draw spottable cars
                int rectHeight = 20;
                int rectWidth = 8;
                float centerX = Width / 2f;
                float centerY = Height / 2f;

                RectangleF localCar = new RectangleF(centerX - rectWidth / 2, centerY - rectHeight / 2, rectWidth, rectHeight);
                g.FillRectangle(Brushes.LimeGreen, localCar);
                g.DrawRectangle(Pens.White, centerX - rectWidth / 2, centerY - rectHeight / 2, rectWidth, rectHeight);

                Brush brush;
                foreach (SpottableCar car in spottables)
                {
                    float multiplier = 5f;
                    float xOffset = (playerX - car.X) * multiplier;
                    float yOffset = (playerY - car.Y) * multiplier;

                    float heading = (float)(-Math.PI / 2 + playerHeading);
                    float newX = (float)(xOffset * Math.Cos(heading) + yOffset * Math.Sin(heading));
                    float newY = (float)(-xOffset * Math.Sin(heading) + yOffset * Math.Cos(heading));

                    RectangleF otherCar = new RectangleF(centerX - rectWidth / 2f, centerY - rectHeight / 2f, rectWidth, rectHeight);
                    otherCar.Offset(newX, newY);
                    var transform = g.Transform;
                    transform.RotateAt((float)(-(playerHeading - car.Heading) * 180f / Math.PI), new PointF(otherCar.Left + rectWidth / 2f, otherCar.Top + rectHeight / 2f));

                    g.Transform = transform;
                    brush = Brushes.White;
                    if (car.Distance < 10)
                        brush = Brushes.Red;

                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    g.FillRectangle(brush, otherCar);

                    g.ResetTransform();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

        }

        private float DistanceBetween(float x1, float y1, float x2, float y2) => (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}
