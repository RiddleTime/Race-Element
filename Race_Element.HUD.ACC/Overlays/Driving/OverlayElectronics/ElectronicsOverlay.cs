using RaceElement.Data.ACC.Cars;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayElectronics
{
    [Overlay(Name = "Electronics",
        Description = "Shows current Brake Bias, ABS, TC1, TC2 and engine map setting.",
        OverlayCategory = OverlayCategory.Car,
        OverlayType = OverlayType.Release)]
    internal sealed class ElectronicsOverlay : AbstractOverlay
    {
        private readonly ElectronicsConfiguration _config = new ElectronicsConfiguration();
        private sealed class ElectronicsConfiguration : OverlayConfiguration
        {
            public ElectronicsConfiguration() => AllowRescale = true;
        }

        private Font _font;

        private PanelText _brakeBiasHeader;
        private PanelText _brakeBiasValue;
        private PanelText _tc1Header;
        private PanelText _tc1Value;
        private PanelText _tc2Header;
        private PanelText _tc2Value;
        private PanelText _absHeader;
        private PanelText _absValue;
        private PanelText _mapHeader;
        private PanelText _mapValue;

        public ElectronicsOverlay(Rectangle rectangle) : base(rectangle, "Electronics")
        {
            RefreshRateHz = 2;
        }

        public override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(11f * this.Scale);

            int lineHeight = _font.Height;
            int unscaledHeaderWidth = 36;
            int unscaledValueWidth = 44;

            int headerWidth = (int)(unscaledHeaderWidth * this.Scale);
            int valueWidth = (int)(unscaledValueWidth * this.Scale);
            int roundingRadius = (int)(6 * this.Scale);

            RectangleF headerRect = new RectangleF(0, 0, headerWidth, lineHeight);
            RectangleF valueRect = new RectangleF(headerWidth, 0, valueWidth, lineHeight);
            StringFormat headerFormat = new StringFormat() { Alignment = StringAlignment.Near };
            StringFormat valueFormat = new StringFormat() { Alignment = StringAlignment.Center };

            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            CachedBitmap headerBackground = new CachedBitmap(headerWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, headerWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, roundingRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 10, 10, 10)), path);
                g.DrawLine(new Pen(accentColor), 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
            });
            CachedBitmap valueBackground = new CachedBitmap(valueWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, valueWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 0, 0, 0)), path);
                g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
            });

            _brakeBiasHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _brakeBiasValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            _absHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _absValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            _tc1Header = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _tc1Value = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            _tc2Header = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _tc2Value = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            _mapHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _mapValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            this.Width = unscaledHeaderWidth + unscaledValueWidth;
            this.Height = (int)(headerRect.Top / this.Scale);
        }

        public override void BeforeStop()
        {
            _brakeBiasHeader?.Dispose();
            _brakeBiasValue?.Dispose();
            _tc1Header?.Dispose();
            _tc1Value?.Dispose();
            _tc2Header?.Dispose();
            _tc2Value?.Dispose();
            _absHeader?.Dispose();
            _absValue?.Dispose();
            _mapHeader?.Dispose();
            _mapValue?.Dispose();

            _font?.Dispose();
        }

        public override void Render(Graphics g)
        {
            float brakeBias = pagePhysics.BrakeBias * 100 + BrakeBias.Get(pageStatic.CarModel);
            if (brakeBias < 0)
                brakeBias = 0;
            _brakeBiasHeader.Draw(g, "BB", this.Scale);
            _brakeBiasValue.Draw(g, $"{brakeBias:F1}", this.Scale);

            _absHeader.Draw(g, "ABS", this.Scale);
            _absValue.Draw(g, $"{pageGraphics.ABS}", this.Scale);

            _tc1Header.Draw(g, "TC1", this.Scale);
            _tc1Value.Draw(g, $"{pageGraphics.TC}", this.Scale);

            _tc2Header.Draw(g, "TC2", this.Scale);
            _tc2Value.Draw(g, $"{pageGraphics.TCCut}", this.Scale);

            _mapHeader.Draw(g, "Map", this.Scale);
            _mapValue.Draw(g, $"{pageGraphics.EngineMap + 1}", this.Scale);
        }
    }
}
