using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlaySpotter
{
    [Overlay(Name = "Radar",
        Description = "Alpha! (A basic car spotter) (no rescaling for now)",
        Version = 1.00,
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Driving
    )]
    internal sealed class RadarOverlay : AbstractOverlay
    {
        private readonly RadarConfiguration _config = new RadarConfiguration();
        private class RadarConfiguration : OverlayConfiguration
        {
            public RadarConfiguration() => AllowRescale = true;


            public readonly RadarGrouping Radar = new RadarGrouping();
            public class RadarGrouping
            {
                [ToolTip("Display cars inside of the pits.")]
                public bool ShowPitted { get; set; } = true;
            }
        }

        private struct SpottableCar
        {
            public int Index;
            public float Y;
            public float X;
            public float Heading;
            public float Distance;
        }
        private const int InitialWidth = 150, InitialHeight = 250;

        private readonly List<SpottableCar> _spottables = new List<SpottableCar>();

        private float _playerY;
        private float _playerX;
        private float _playerHeading;

        private CachedBitmap _cachedBackground;

        public RadarOverlay(Rectangle rectangle) : base(rectangle, "Radar")
        {
            Width = InitialWidth;
            Height = InitialHeight;
            RefreshRateHz = 10;
        }

        public override bool ShouldRender()
        {
            if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        public override void SetupPreviewData()
        {
            _playerY = 0; _playerX = 0; _playerHeading = 0.12f;

            var car1 = new SpottableCar { Index = 2, X = 4, Y = 5.7f, Heading = -0.1f };
            car1.Distance = DistanceBetween(_playerX, _playerY, car1.X, car1.Y);
            _spottables.Add(car1);

            var car2 = new SpottableCar { Index = 3, X = -3f, Y = -3.7f, Heading = 0.1f };
            car2.Distance = DistanceBetween(_playerX, _playerY, car2.X, car2.Y);
            _spottables.Add(car2);

            var car3 = new SpottableCar { Index = 4, X = 3.5f, Y = -3.7f, Heading = 0.1f };
            car3.Distance = DistanceBetween(_playerX, _playerY, car3.X, car3.Y);
            _spottables.Add(car3);

            var car4 = new SpottableCar { Index = 5, X = 0f, Y = -13.7f, Heading = 0.2f };
            car4.Distance = DistanceBetween(_playerX, _playerY, car4.X, car4.Y);
            _spottables.Add(car4);
        }

        public override void BeforeStart()
        {
            int scaledWidth = (int)(Width * Scale);
            int scaledHeight = (int)(Height * Scale);
            _cachedBackground = new CachedBitmap(scaledWidth, scaledHeight, g =>
            {
                GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(0, 0, (int)(InitialWidth * Scale), (int)(InitialHeight * Scale));
                PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.CenterColor = Color.FromArgb(220, 0, 0, 0);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(60, 0, 0, 0) };
                g.FillRoundedRectangle(pthGrBrush, new Rectangle(0, 0, scaledWidth - 1, scaledHeight - 1), 3);
            });
        }

        public override void Render(Graphics g)
        {
            try
            {
                int playerID = broadCastRealTime.FocusedCarIndex;
                var playerCar = EntryListTracker.Instance.Cars.FirstOrDefault(car => car.Key == playerID);
                if (playerCar.Value == null && EntryListTracker.Instance.Cars.Any()) return;


                // find local player id
                for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                {
                    if (pageGraphics.CarIds[i] == playerID)
                    {
                        _playerY = pageGraphics.CarCoordinates[i].Z;
                        _playerX = -pageGraphics.CarCoordinates[i].X;
                        var car = EntryListTracker.Instance.Cars.FirstOrDefault(x => x.Key == playerID);
                        if (car.Value != null)
                            _playerHeading = car.Value.RealtimeCarUpdate.Heading;
                    }
                }

                CollectSpottableCars(playerID);

                if (!_spottables.Any())
                    return;

                //Debug.WriteLine($"{_spottables.Count}");

                _cachedBackground?.Draw(g, Width, Height);

                // draw spottable cars
                int rectHeight = 20;
                int rectWidth = 8;
                float centerX = Width / 2f;
                float centerY = Height / 2f;

                RectangleF localCar = new RectangleF(centerX - rectWidth / 2, centerY - rectHeight / 2, rectWidth, rectHeight);
                g.FillRectangle(Brushes.LimeGreen, localCar);
                g.DrawRectangle(Pens.White, centerX - rectWidth / 2, centerY - rectHeight / 2, rectWidth, rectHeight);

                Brush brush;
                foreach (SpottableCar car in _spottables)
                {
                    float multiplier = 5f;
                    float xOffset = (_playerY - car.X) * multiplier;
                    float yOffset = (_playerX - car.Y) * multiplier;

                    float heading = (float)(-Math.PI / 2 + _playerHeading);
                    float newX = (float)(xOffset * Math.Cos(heading) + yOffset * Math.Sin(heading));
                    float newY = (float)(-xOffset * Math.Sin(heading) + yOffset * Math.Cos(heading));

                    RectangleF otherCar = new RectangleF(centerX - rectWidth / 2f, centerY - rectHeight / 2f, rectWidth, rectHeight);
                    otherCar.Offset(newX, newY);
                    var transform = g.Transform;
                    transform.RotateAt((float)(-(_playerHeading - car.Heading) * 180f / Math.PI), new PointF(otherCar.Left + rectWidth / 2f, otherCar.Top + rectHeight / 2f));

                    g.Transform = transform;
                    brush = Brushes.White;
                    if (car.Distance < 6)
                        brush = Brushes.Red;
                    else if (car.Distance < 8)
                        brush = Brushes.Yellow;

                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    g.FillRectangle(brush, otherCar);

                    g.ResetTransform();
                }

                _spottables.Clear();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

        }

        private void CollectSpottableCars(int playerID)
        {
            // find spottable cars
            for (int i = 0; i < pageGraphics.CarIds.Length; i++)
            {
                if (pageGraphics.CarIds[i] != playerID && pageGraphics.CarIds[i] != 0)
                {
                    float x = pageGraphics.CarCoordinates[i].Z;
                    float y = -pageGraphics.CarCoordinates[i].X;

                    float distance = DistanceBetween(_playerY, _playerX, x, y);

                    if (distance < 12)
                    {
                        var spottable = new SpottableCar()
                        {
                            Index = pageGraphics.CarIds[i],
                            X = x,
                            Y = y,
                            Distance = distance,
                        };

                        var car = EntryListTracker.Instance.Cars.FirstOrDefault(x => x.Key == spottable.Index);
                        if (car.Value != null)
                        {
                            spottable.Heading = car.Value.RealtimeCarUpdate.Heading;
                            if (!_config.Radar.ShowPitted && car.Value.RealtimeCarUpdate.CarLocation == Broadcast.CarLocationEnum.Pitlane)
                                continue;
                        }
                        if (!_spottables.Contains(spottable))
                            _spottables.Add(spottable);
                    }
                }
            }
        }

        private float DistanceBetween(float x1, float y1, float x2, float y2) => (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}
