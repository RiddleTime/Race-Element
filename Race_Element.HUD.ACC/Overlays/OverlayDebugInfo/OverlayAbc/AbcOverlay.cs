using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayAbc
{
#if DEBUG
    [Overlay(Name = "ABC", Description = "scrabble this, testing", OverlayType = OverlayType.Release)]
#endif
    internal sealed class AbcOverlay : AbstractOverlay
    {
        private Font _font;

        private PanelText[][] panelTexts;
        private CachedBitmap panelBackground;

        private readonly AbcConfiguration _config = new AbcConfiguration();
        private sealed class AbcConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("test", "Description here")]
            public TestGrouping Test { get; set; } = new TestGrouping();
            public class TestGrouping
            {
                [IntRange(1, 20, 1)]
                public int RowCount { get; set; } = 10;

                [IntRange(1, 50, 1)]
                public int Herz { get; set; } = 2;
            }

            public AbcConfiguration() => AllowRescale = true;
        }

        public AbcOverlay(Rectangle rectangle) : base(rectangle, "ABC") { }

        public override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(12);

            RefreshRateHz = _config.Test.Herz;

            int panelWidth = _font.Height * 4;
            int panelHeight = _font.Height;

            Width = 2 * panelWidth + 1;
            Height = _config.Test.RowCount * panelHeight + 1;

            panelBackground = new CachedBitmap(panelWidth, panelHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, panelWidth, panelHeight);
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), panelRect, 6);
            });

            panelTexts = new PanelText[_config.Test.RowCount][];
            for (int row = 0; row < _config.Test.RowCount; row++)
            {
                panelTexts[row] = new PanelText[2];

                for (int col = 0; col < 2; col++)
                {
                    panelTexts[row][col] = new PanelText(_font, panelBackground, new RectangleF(panelWidth * col, panelHeight * row, panelWidth, panelHeight));

                    if (col == 0)
                        panelTexts[row][col].StringFormat.Alignment = StringAlignment.Near;
                    else
                        panelTexts[row][col].StringFormat.Alignment = StringAlignment.Center;
                }
            }
        }

        public override void BeforeStop()
        {
            _font?.Dispose();
            panelBackground?.Dispose();
        }

        public override void Render(Graphics g)
        {
            for (int row = 0; row < _config.Test.RowCount; row++)
                for (int col = 0; col < 2; col++)
                    panelTexts[row][col].Draw(g, $"{pagePhysics.AirTemp:F2}");
        }
    }
}
