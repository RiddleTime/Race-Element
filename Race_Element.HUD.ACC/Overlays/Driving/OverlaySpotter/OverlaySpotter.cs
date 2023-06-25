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
#if DEBUG
    [Overlay(Name = "Spotter",
        Description = "TODO (spots things?)",
        Version = 1.00,
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Driving
    )]
#endif
    internal sealed class SpotterOverlay : AbstractOverlay
    {
        private readonly SpotterConfiguration _config = new SpotterConfiguration();
        private class SpotterConfiguration : OverlayConfiguration
        {
            public SpotterConfiguration() => AllowRescale = true;
        }

        public SpotterOverlay(Rectangle rectangle) : base(rectangle, "Spotter")
        {
            Width = 200;
            Height = 200;
            RefreshRateHz = 10;
        }

        public override bool ShouldRender()
        {

            if (/*_config.Delta.Spectator &&*/ RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
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
                float playerAngle = (float)(playerHeading * 180 / Math.PI);


                List<SpottableCar> spottables = new List<SpottableCar>();

                for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                {
                    if (pageGraphics.CarIds[i] == playerID)
                    {
                        playerX = pageGraphics.CarCoordinates[i].Z;
                        playerY = -pageGraphics.CarCoordinates[i].X;
                    }
                }

                for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                {
                    if (pageGraphics.CarIds[i] != playerID && pageGraphics.CarIds[i] != 0)
                    {
                        float x = pageGraphics.CarCoordinates[i].Z;
                        float y = -pageGraphics.CarCoordinates[i].X;

                        float distance = DistanceBetween(playerX, playerY, x, y);

                        if (distance < 100)
                        {
                            var spottable = new SpottableCar()
                            {
                                Index = pageGraphics.CarIds[i],
                                X = x,
                                Y = y,
                                //Heading = heading,
                                Distance = distance,
                            };

                            if (!spottables.Contains(spottable))
                                spottables.Add(spottable);
                        }
                    }
                }

                if (!spottables.Any())
                    return;

                int rectHeight = 8;
                int rectWidth = 4;
                float centerX = Width / 2f;
                float centerY = Height / 2f;

                RectangleF localCar = new RectangleF(centerX - rectWidth / 2, centerY - rectHeight / 2, rectWidth, rectHeight);
                g.FillRectangle(Brushes.White, localCar);


                foreach (SpottableCar car in spottables)
                {
                    float xOffset = (playerX - car.X);
                    float yOffset = (playerY - car.Y);

                    float heading = -1.5f + playerHeading;
                    float newX = (float)(xOffset * Math.Cos(heading) + yOffset * Math.Sin(heading));
                    float newY = (float)(-xOffset * Math.Sin(heading) + yOffset * Math.Cos(heading));

                    RectangleF otherCar = new RectangleF(centerX - rectWidth / 2, centerY - rectHeight / 2, rectWidth, rectHeight);
                    otherCar.Offset(newX, newY);


                    g.FillRectangle(Brushes.Red, otherCar);
                    g.ResetTransform();

                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

        }

        private float DistanceBetween(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }
}
