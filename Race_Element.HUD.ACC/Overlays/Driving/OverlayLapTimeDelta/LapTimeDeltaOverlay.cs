using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Util;
using System.Drawing.Text;
using System.Diagnostics;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.EntryList;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapDelta
{
    [Overlay(Name = "Laptime Delta", Description = "A customizable Laptime Delta Bar", OverlayType = OverlayType.Release, Version = 1,
        OverlayCategory = OverlayCategory.Lap)]
    internal class LapTimeDeltaOverlay : AbstractOverlay
    {
        private readonly LapTimeDeltaConfiguration _config = new LapTimeDeltaConfiguration();

        private float _deltaStringWidth = -1;
        private readonly Font _font;

        private CachedBitmap _cachedBackground;
        private CachedBitmap _cachedPositiveDelta;
        private CachedBitmap _cachedNegativeDelta;

        public LapTimeDeltaOverlay(Rectangle rectangle) : base(rectangle, "Laptime Delta")
        {
            this.Width = _config.Bar.Width + 1;
            this.Height = _config.Bar.Height + 1;

            _font = FontUtil.FontSegoeMono(_config.Delta.FontSize);
            this.Height += _font.Height * 1;

            this.RefreshRateHz = 5;
        }

        public override void BeforeStart()
        {
            try
            {
                int cornerRadius = (int)(_config.Bar.Roundness * this.Scale);

                _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
                {
                    Color bgColor = Color.FromArgb(185, 0, 0, 0);
                    HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, bgColor, Color.FromArgb(bgColor.A - 50, bgColor));
                    g.FillRoundedRectangle(hatchBrush, new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
                    g.DrawRoundedRectangle(new Pen(Color.Black, 1 * this.Scale), new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
                });

                _cachedPositiveDelta = new CachedBitmap((int)(_config.Bar.Width / 2 * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
                {
                    Rectangle rect = new Rectangle(0, 0, (int)(_config.Bar.Width / 2 * this.Scale), (int)(_config.Bar.Height * this.Scale));
                    using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, cornerRadius, 0, 0, cornerRadius);
                    g.FillPath(new SolidBrush(Color.FromArgb(_config.Colors.SlowerOpacity, _config.Colors.SlowerColor)), path);
                });

                _cachedNegativeDelta = new CachedBitmap((int)(_config.Bar.Width / 2 * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
                {
                    Rectangle rect = new Rectangle(0, 0, (int)(_config.Bar.Width / 2 * this.Scale), (int)(_config.Bar.Height * this.Scale));
                    using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, 0, cornerRadius, cornerRadius, 0);
                    g.FillPath(new SolidBrush(Color.FromArgb(_config.Colors.FasterOpacity, _config.Colors.FasterColor)), path);
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public override void BeforeStop()
        {
            _cachedBackground?.Dispose();
            _cachedPositiveDelta?.Dispose();
            _cachedNegativeDelta?.Dispose();

            _font?.Dispose();
        }

        public override bool ShouldRender()
        {
            if (_config.Delta.HideForRace && !this.IsRepositioning && pageGraphics.SessionType == ACCSharedMemory.AcSessionType.AC_RACE)
                return false;

            if (_config.Delta.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g, 0, 0, _config.Bar.Width, _config.Bar.Height);

            DrawDeltaBar(g);

            DrawDeltaText(g);
        }


        private float GetDelta()
        {
            float delta = (float)pageGraphics.DeltaLapTimeMillis / 1000;

            if (_config.Delta.Spectator)
            {
                int focusedIndex = broadCastRealTime.FocusedCarIndex;
                if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, focusedIndex))
                    lock (EntryListTracker.Instance.Cars)
                    {
                        if (EntryListTracker.Instance.Cars.Any())
                        {
                            var car = EntryListTracker.Instance.Cars.First(car => car.Value.RealtimeCarUpdate.CarIndex == focusedIndex);
                            delta = car.Value.RealtimeCarUpdate.Delta / 1000f;
                        }
                    }
            }

            return delta;
        }

        private void DrawDeltaBar(Graphics g)
        {
            float delta = GetDelta();
            delta.Clip(-_config.Delta.MaxDelta, _config.Delta.MaxDelta);

            float halfBarWidth = _config.Bar.Width / 2f;

            if (delta > 0)
            {
                float fillPercent = delta / _config.Delta.MaxDelta;
                float drawWidth = halfBarWidth * fillPercent;
                drawWidth.ClipMin(1);

                g.SetClip(new Rectangle((int)(halfBarWidth - drawWidth), 0, (int)drawWidth, _config.Bar.Height));
                _cachedPositiveDelta?.Draw(g, 0, 0, (int)halfBarWidth, _config.Bar.Height);
                g.ResetClip();
            }
            if (delta < 0)
            {
                float fillPercent = delta / -_config.Delta.MaxDelta;
                float drawWidth = halfBarWidth * fillPercent;
                drawWidth.ClipMin(1);

                g.SetClip(new Rectangle((int)(halfBarWidth), 0, (int)drawWidth, _config.Bar.Height));
                _cachedNegativeDelta?.Draw(g, (int)halfBarWidth, 0, (int)halfBarWidth, _config.Bar.Height);
                g.ResetClip();
            }
        }

        private void DrawDeltaText(Graphics g)
        {
            float delta = GetDelta();
            delta.Clip(-9.0f, 9.0f);

            string currentDelta = $"{delta.ToString($"F{_config.Delta.Decimals}")}";
            if (delta >= 0) currentDelta = "+" + currentDelta;

            currentDelta.FillStart(_config.Delta.Decimals + 3, ' '); // (+3) = ('-' or '+') plus "0."

            if (_deltaStringWidth < 0)
                _deltaStringWidth = g.MeasureString(currentDelta, _font).Width;

            int x = _config.Bar.Width / 2;
            int y = _config.Bar.Height + 2;
            DrawTextWithOutline(g, pageGraphics.IsValidLap ? Color.White : Color.Red, currentDelta, x, y);
        }

        private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
        {
            Rectangle backgroundDimension = new Rectangle((int)(x - _deltaStringWidth / 2), y, (int)(_deltaStringWidth), (int)(_font.Height * 0.9));

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), backgroundDimension, (int)(2 * this.Scale));

            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;

            g.DrawStringWithShadow(text, _font, textColor, new PointF(x - _deltaStringWidth / 2, y), 1.3f);
        }
    }
}
