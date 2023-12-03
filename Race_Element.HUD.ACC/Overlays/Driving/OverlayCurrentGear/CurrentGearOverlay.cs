using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using Unglide;

namespace RaceElement.HUD.ACC.Overlays.OverlayCurrentGear
{
    [Overlay(Name = "Current Gear", Version = 1.00, OverlayType = OverlayType.Drive,
        OverlayCategory = OverlayCategory.Driving,
    Description = "Shows the selected gear.")]
    internal sealed class CurrentGearOverlay : AbstractOverlay
    {
        private readonly CurrentGearConfiguration _config = new CurrentGearConfiguration();
        private sealed class CurrentGearConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Gear", "Options for the Current Gear HUD")]
            public GearGrouping Gear { get; set; } = new GearGrouping();
            public class GearGrouping
            {
                [ToolTip("Show the current gear HUD when spectating")]
                public bool Spectator { get; set; } = true;
            }

            [ConfigGrouping("Colors", "Adjust colors")]
            public ColorsGrouping Colors { get; set; } = new ColorsGrouping();
            public class ColorsGrouping
            {
                public Color TextColor { get; set; } = Color.FromArgb(255, 255, 255, 255);
                [IntRange(75, 255, 1)]
                public int TextOpacity { get; set; } = 255;
            }

            public CurrentGearConfiguration() { AllowRescale = true; }
        }

        private const int InitialWidth = 80;
        private const int InitialHeight = 72;
        private readonly List<CachedBitmap> _gearBitmaps = new List<CachedBitmap>();

        private int _lastGear = -2;
        private const float MaxOpacity = 1f;
        private float _opacity = MaxOpacity;
        private Tweener _gearTweener;
        private DateTime _gearTweenerStart = DateTime.Now;


        public CurrentGearOverlay(Rectangle rectangle) : base(rectangle, "Current Gear")
        {
            Width = InitialWidth;
            Height = InitialHeight;
            RefreshRateHz = 30;
        }

        public override void SetupPreviewData()
        {
            _lastGear = 3;
            pagePhysics.Gear = 3;
        }

        public override void BeforeStart()
        {
            Font font = FontUtil.FontConthrax(50 * this.Scale);
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.FromArgb(225, Color.Black), Color.FromArgb(185, Color.Black));

            Rectangle renderRectangle = new Rectangle(0, 0, (int)(InitialWidth * this.Scale), (int)(InitialHeight * this.Scale));
            for (int i = 0; i <= 8; i++)
            {
                string gear = i switch
                {
                    0 => "R",
                    1 => "N",
                    _ => $"{i - 1}",
                };

                _gearBitmaps.Add(new CachedBitmap((int)(InitialWidth * this.Scale) + 1, (int)(InitialHeight * this.Scale) + 1, g =>
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.TextContrast = 1;

                    g.FillRoundedRectangle(hatchBrush, renderRectangle, (int)(6 * this.Scale));

                    int textWidth = (int)g.MeasureString(gear, font).Width;
                    g.DrawStringWithShadow(gear, font, Color.FromArgb(_config.Colors.TextOpacity, _config.Colors.TextColor), new Point(renderRectangle.Width / 2 - textWidth / 2, (int)(renderRectangle.Height / 2 - font.Height / 2.18)), 1.5f * this.Scale);
                }));
            }

            font.Dispose();
            hatchBrush.Dispose();

            _gearTweener = new Tweener();
        }

        public override void BeforeStop()
        {
            foreach (CachedBitmap cachedBitmap in _gearBitmaps)
                cachedBitmap?.Dispose();
        }

        public override bool ShouldRender()
        {
            if (_config.Gear.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        private int GetCurrentGear()
        {
            int currentGear = pagePhysics.Gear;

            if (_config.Gear.Spectator)
            {
                int focusedIndex = broadCastRealTime.FocusedCarIndex;

                if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, focusedIndex))
                    lock (EntryListTracker.Instance.Cars)
                        if (EntryListTracker.Instance.Cars.Any())
                        {
                            var car = EntryListTracker.Instance.Cars.First(car => car.Key == focusedIndex);
                            currentGear = car.Value.RealtimeCarUpdate.Gear + 2;
                        }
            }

            return currentGear;
        }

        public override void Render(Graphics g)
        {
            int currentGear = GetCurrentGear();

            if (_lastGear != currentGear)
            {
                _opacity = 0.8f;
                _gearTweenerStart = DateTime.Now;
                _gearTweener.Tween(this, new { _opacity = MaxOpacity }, 0.8f);
                _lastGear = currentGear;
            }

            if (_opacity < MaxOpacity)
                _gearTweener.Update(secondsElapsed: (float)DateTime.Now.Subtract(_gearTweenerStart).TotalSeconds);

            CachedBitmap bitmap = _gearBitmaps[currentGear];
            bitmap.Opacity = _opacity;
            bitmap?.Draw(g, InitialWidth, InitialHeight);
        }
    }
}
