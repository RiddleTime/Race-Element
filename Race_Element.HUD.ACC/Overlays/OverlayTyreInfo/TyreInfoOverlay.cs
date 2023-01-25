using RaceElement.HUD.Overlay.Internal;
using System.Drawing;
using System.Drawing.Drawing2D;
using RaceElement.HUD.Overlay.Util;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.ACC.Overlays.OverlayPressureTrace;
using RaceElement.HUD.Overlay.OverlayUtil;
using static RaceElement.Data.SetupConverter;
using System.Drawing.Text;
using System;

namespace RaceElement.HUD.ACC.Overlays.OverlayTyreInfo
{
    [Overlay(Name = "Tyre Info", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "Shows tyre pressures. Additionally shows tyre temperature, brake temps and pad life. Overlays the in-game tyre info.")]
    internal sealed class TyreInfoOverlay : AbstractOverlay
    {
        private readonly TyreInfoConfig _config = new TyreInfoConfig();
        private class TyreInfoConfig : OverlayConfiguration
        {
            [ConfigGrouping("Info", "Show additional information about the condition of the tyres.")]
            public InfoGrouping Information { get; set; } = new InfoGrouping();
            public class InfoGrouping
            {
                [ToolTip("Displays the percentage of brake pad life above the brake pads.")]
                public bool PadLife { get; set; } = true;

                [ToolTip("Displays the average of front and rear brake temperatures under the brake pads.")]
                public bool BrakeTemps { get; set; } = true;

                [ToolTip("Displays the tyre temperature for each tyre whilst displaying colors." +
                    "\nGreen is optimal, Red is too hot, Blue is too cold.")]
                public bool TyreTemps { get; set; } = true;

                [ToolTip("Defines the amount of decimals for the tyre pressure text.")]
                [IntRange(1, 2, 1)]
                public int Decimals { get; set; } = 1;
            }

            public TyreInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        private const int InitialWidth = 135;
        private const int InitialHeight = 190;

        private const double MaxPadLife = 29;
        private readonly Font _fontFamilyLarge;
        private readonly Font _fontFamily;
        private readonly Font _fontFamilySmall;
        private readonly int _yMono;
        private readonly int _yMonoSmall;

        public TyreInfoOverlay(Rectangle rectangle) : base(rectangle, "Tyre Info")
        {
            _fontFamilyLarge = FontUtil.FontUnispace(12);
            _fontFamily = FontUtil.FontUnispace(10);
            _yMono = _fontFamily.Height / 6;
            _fontFamilySmall = FontUtil.FontUnispace(9);
            _yMonoSmall = _fontFamilySmall.Height / 5;
            this.Width = InitialWidth;
            this.Height = InitialHeight;
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
            if (this.IsRepositioning)
            {
                Pen repositionLinePen = new Pen(Brushes.Red, 2 * this.Scale);
                g.DrawLine(repositionLinePen, new Point(InitialWidth / 2, 0), new Point(InitialWidth / 2, InitialHeight));
                g.DrawLine(repositionLinePen, new Point(0, InitialHeight / 2), new Point(InitialWidth, InitialHeight / 2));
            }

            DrawTyrePressures(g);

            if (this._config.Information.PadLife)
            {
                DrawPadWearText(g, 68, 19, Position.Front);
                DrawPadWearText(g, 68, 156, Position.Rear);
            }

            if (this._config.Information.BrakeTemps)
            {
                DrawBrakeTemps(g, 68, 71, Position.Front);
                DrawBrakeTemps(g, 68, 103, Position.Rear);
            }

            if (this._config.Information.TyreTemps)
            {
                DrawTyreTemp(g, 28, 60, Wheel.FrontLeft);
                DrawTyreTemp(g, 106, 60, Wheel.FrontRight);
                DrawTyreTemp(g, 28, 114, Wheel.RearLeft);
                DrawTyreTemp(g, 106, 114, Wheel.RearRight);
            }
        }

        private void DrawTyrePressures(Graphics g)
        {
            TyrePressureRange range = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);

            if (range != null)
            {
                DrawTyrePressure(g, 0, 0, Wheel.FrontLeft, range);
                DrawTyrePressure(g, 76, 0, Wheel.FrontRight, range);
                DrawTyrePressure(g, 0, 169, Wheel.RearLeft, range);
                DrawTyrePressure(g, 76, 169, Wheel.RearRight, range);
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
                tyreBrush = Brushes.DarkOliveGreen;
                if (temp > 65)
                    tyreBrush = Brushes.DarkRed;
                if (temp < 25)
                    tyreBrush = Brushes.DarkCyan;
            }


            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.TextContrast = 1;


            string text = $"{temp:F1}";
            int textWidth = (int)g.MeasureString(text, _fontFamily).Width;

            Rectangle backgroundDimension = new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamily.Height);

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(210, 255, 255, 255)), backgroundDimension, 2);
            g.DrawRoundedRectangle(new Pen(tyreBrush), backgroundDimension, 2);

            g.DrawStringWithShadow(text, _fontFamily, tyreBrush, new PointF(x - textWidth / 2, y + _yMono - 1));
        }

        private void DrawBrakeTemps(Graphics g, int x, int y, Position position)
        {
            float averageBrakeTemps = 0;
            switch (position)
            {
                case Position.Front:
                    {
                        float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.FrontLeft];
                        float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.FrontRight];
                        averageBrakeTemps = (brakeTempLeft + brakeTempRight) / 2;
                        break;
                    }
                case Position.Rear:
                    {
                        float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.RearLeft];
                        float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.RearRight];
                        averageBrakeTemps = (brakeTempLeft + brakeTempRight) / 2;
                        break;
                    }
            }

            string text = $"{averageBrakeTemps:F0} C";
            int textWidth = (int)g.MeasureString(text, _fontFamilySmall).Width;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.TextContrast = 1;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilySmall.Height), 2);
            g.DrawStringWithShadow(text, _fontFamilySmall, Brushes.White, new PointF(x - textWidth / 2, y + _yMonoSmall));

            g.SmoothingMode = previous;
            g.TextRenderingHint = previousHint;
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
            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.TextContrast = 1;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilySmall.Height), 2);

            g.DrawStringWithShadow(text, _fontFamilySmall, Brushes.White, new PointF(x - textWidth / 2, y + _yMonoSmall / 2));

            g.SmoothingMode = previous;
            g.TextRenderingHint = previousHint;
        }

        private void DrawTyrePressure(Graphics g, int x, int y, Wheel wheel, TyrePressureRange range)
        {
            int alpha = 255;

            Color brushColor = Color.FromArgb(alpha, 0, 255, 0);

            if (pagePhysics.WheelPressure[(int)wheel] >= range.OptimalMaximum)
                brushColor = Color.FromArgb(alpha, 255, 0, 0);

            if (pagePhysics.WheelPressure[(int)wheel] <= range.OptimalMinimum)
                brushColor = Color.FromArgb(alpha, 0, 0, 255);

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = 58;
            g.FillRoundedRectangle(new SolidBrush(brushColor), new Rectangle(x, y, width, 20), 3);


            CompositingQuality previousComposingQuality = g.CompositingQuality;
            TextRenderingHint previousTextRenderingHint = g.TextRenderingHint;
            int previousTextContrast = g.TextContrast;

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.TextContrast = 1;
            DrawTextWithOutline(g, Color.White, pagePhysics.WheelPressure[(int)wheel].ToString($"F{_config.Information.Decimals}"), x + width / 2, y + 1);

            g.SmoothingMode = previous;
            g.CompositingQuality = previousComposingQuality;
            g.TextContrast = previousTextContrast;
            g.TextRenderingHint = previousTextRenderingHint;
        }

        private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
        {
            int textWidth = (int)g.MeasureString(text, _fontFamilyLarge).Width;
            Rectangle backgroundDimension = new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilyLarge.Height);
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(210, 0, 0, 0)), backgroundDimension, 2);
            g.DrawRoundedRectangle(new Pen(Color.FromArgb(135, 0, 0, 0), 0.6f * this.Scale), backgroundDimension, 2);
            g.DrawStringWithShadow(text, _fontFamilyLarge, textColor, new PointF(x - textWidth / 2, y + _fontFamilyLarge.GetHeight(g) / 11f), 1.3f * this.Scale);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
