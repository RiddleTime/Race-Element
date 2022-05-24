using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCManager.HUD.Overlay.Util;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.ACC.Overlays.OverlayPressureTrace;
using ACCManager.HUD.Overlay.OverlayUtil;
using static ACCManager.Data.SetupConverter;
using static ACCManager.ACCSharedMemory;
using System.Drawing.Text;

namespace ACCManager.HUD.ACC.Overlays.OverlayTyreInfo
{
    internal sealed class TyreInfoOverlay : AbstractOverlay
    {
        private readonly TyreInfoConfig _config = new TyreInfoConfig();
        private class TyreInfoConfig : OverlayConfiguration
        {
            [ToolTip("Displays the percentage of brake pad life above the brake pads.")]
            public bool ShowPadLife { get; set; } = true;

            [ToolTip("Displays the average of front and rear brake temperatures under the brake pads.")]
            public bool ShowBrakeTemps { get; set; } = true;

            [ToolTip("Displays the tyre temperature for each tyre whilst displaying colors." +
                "\nGreen is optimal, Red is too hot, Blue is too cold.")]
            public bool ShowTyreTemps { get; set; } = true;

            public TyreInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        private const double MaxPadLife = 29;
        private readonly Font _fontFamily;
        private readonly Font _fontFamilySmall;
        private readonly int _yMono;
        private readonly int _yMonoSmall;

        public TyreInfoOverlay(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
        {
            _fontFamily = FontUtil.FontUnispace(10);
            _yMono = _fontFamily.Height / 6;
            _fontFamilySmall = FontUtil.FontUnispace(9);
            _yMonoSmall = _fontFamilySmall.Height / 5;
            this.Width = 135;
            this.Height = 200;
            this.RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            DrawPressureBackgrounds(g);

            if (this._config.ShowPadLife)
            {
                DrawPadWearText(g, 68, 29, Position.Front);
                DrawPadWearText(g, 68, 113, Position.Rear);
            }

            if (this._config.ShowBrakeTemps)
            {
                DrawBrakeTemps(g, 68, 81, Position.Front);
                DrawBrakeTemps(g, 68, 166, Position.Rear);
            }

            if (this._config.ShowTyreTemps)
            {
                DrawTyreTemp(g, 28, 55, Wheel.FrontLeft);
                DrawTyreTemp(g, 106, 55, Wheel.FrontRight);
                DrawTyreTemp(g, 28, 139, Wheel.RearLeft);
                DrawTyreTemp(g, 106, 139, Wheel.RearRight);
            }
        }

        private void DrawPressureBackgrounds(Graphics g)
        {
            TyrePressureRange range = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);

            if (range != null)
            {
                DrawPressureBackground(g, 0, 10, Wheel.FrontLeft, range);
                DrawPressureBackground(g, 76, 10, Wheel.FrontRight, range);
                DrawPressureBackground(g, 0, 179, Wheel.RearLeft, range);
                DrawPressureBackground(g, 76, 179, Wheel.RearRight, range);
            }
        }

        private void DrawTyreTemp(Graphics g, int x, int y, Wheel wheel)
        {
            float temp = pagePhysics.TyreCoreTemperature[(int)wheel];

            Brush tyreBrush = Brushes.DarkOliveGreen;


            if (temp > 90)
                tyreBrush = Brushes.DarkRed;
            if (temp < 75)
                tyreBrush = Brushes.DarkCyan;

            if (pageGraphics.TyreCompound == "wet_compound")
            {
                if (temp > 65)
                    tyreBrush = Brushes.DarkRed;
                if (temp < 25)
                    tyreBrush = Brushes.DarkCyan;
            }


            string text = $"{temp:F1}";
            int textWidth = (int)g.MeasureString(text, _fontFamily).Width;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.TextContrast = 1;


            Rectangle backgroundDimension = new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamily.Height);

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(210, 255, 255, 255)), backgroundDimension, 2);
            g.DrawRoundedRectangle(new Pen(tyreBrush), backgroundDimension, 2);

            g.DrawString(text, _fontFamily, tyreBrush, x - textWidth / 2, y + _yMono);

            g.SmoothingMode = previous;
            g.TextRenderingHint = previousHint;
        }

        private void DrawBrakeTemps(Graphics g, int x, int y, Position position)
        {
            double percentage = 0;
            switch (position)
            {
                case Position.Front:
                    {
                        float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.FrontLeft];
                        float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.FrontRight];

                        float averageBrakeTemp = (brakeTempLeft + brakeTempRight) / 2;

                        percentage = averageBrakeTemp / MaxPadLife;
                        break;
                    }
                case Position.Rear:
                    {
                        float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.RearRight];
                        float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.RearRight];

                        float averageBrakeTemp = (brakeTempLeft + brakeTempRight) / 2;

                        percentage = averageBrakeTemp / MaxPadLife;
                        break;
                    }
            }

            string text = $"{percentage * 100:F0} C";
            int textWidth = (int)g.MeasureString(text, _fontFamilySmall).Width;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilySmall.Height), 2);
            g.DrawString(text, _fontFamilySmall, Brushes.White, x - textWidth / 2, y + _yMonoSmall);

            g.SmoothingMode = previous;
        }

        private void DrawPadWearText(Graphics g, int x, int y, Position position)
        {
            double percentage = 0;
            switch (position)
            {
                case Position.Front:
                    {
                        float padLifeLeft = pagePhysics.PadLife[(int)Wheel.FrontLeft];
                        float padLifeRight = pagePhysics.PadLife[(int)Wheel.FrontRight];

                        float averagePadLife = (padLifeLeft + padLifeRight) / 2;

                        percentage = averagePadLife / MaxPadLife;
                        break;
                    }
                case Position.Rear:
                    {
                        float padLifeLeft = pagePhysics.PadLife[(int)Wheel.RearRight];
                        float padLifeRight = pagePhysics.PadLife[(int)Wheel.RearRight];

                        float averagePadLife = (padLifeLeft + padLifeRight) / 2;

                        percentage = averagePadLife / MaxPadLife;
                        break;
                    }
            }

            string text = $"{percentage * 100:F0} %";
            int textWidth = (int)g.MeasureString(text, _fontFamilySmall).Width;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilySmall.Height), 2);
            g.DrawString(text, _fontFamilySmall, Brushes.White, x - textWidth / 2, y + _yMonoSmall / 2);

            g.SmoothingMode = previous;
        }

        private void DrawPressureBackground(Graphics g, int x, int y, Wheel wheel, TyrePressureRange range)
        {
            int alpha = 60;

            Color brushColor = Color.FromArgb(alpha, 0, 255, 0);

            if (pagePhysics.WheelPressure[(int)wheel] >= range.OptimalMaximum)
                brushColor = Color.FromArgb(alpha, 255, 0, 0);

            if (pagePhysics.WheelPressure[(int)wheel] <= range.OptimalMinimum)
                brushColor = Color.FromArgb(alpha, 0, 0, 255);

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(brushColor), new Rectangle(x, y, 58, 20), 3);
            g.SmoothingMode = previous;
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
