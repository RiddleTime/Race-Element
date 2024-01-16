using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.System.OverlayRaceElementProcess
{
    [Overlay(Name = "Race Element Process",
        Description = "Shows details about the Race Element process",
        OverlayType = OverlayType.Pitwall)]
    internal class RaceElementProcessOverlay : AbstractOverlay
    {

        InfoPanel _panel;
        public RaceElementProcessOverlay(Rectangle rectangle) : base(rectangle, "Race Element Process")
        {
            RefreshRateHz = 10;
            Height = 150;
            Width = 300;
            _panel = new InfoPanel(10, 300);
        }

        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            using Process current = Process.GetCurrentProcess();

            _panel.AddLine("Working x64", $"{current.WorkingSet64 / 1_000_000f:F2} MB");
            _panel.AddLine("Peak x64", $"{current.PeakWorkingSet64 / 1_000_000f:F2} MB");
            _panel.AddLine("Private x64", $"{current.PrivateMemorySize64 / 1_000_000f:F2} MB");

            _panel?.Draw(g);
        }
    }
}
