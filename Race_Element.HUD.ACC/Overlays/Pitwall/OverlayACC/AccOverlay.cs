using RaceElement.Data.ACC.Core;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayACC
{
#if DEBUG
    [Overlay(Name = "ACC Process", Version = 1.00, OverlayType = OverlayType.Pitwall,
       Description = "A panel showing information about ACC.")]
#endif
    internal class AccProcessOverlay : AbstractOverlay
    {
        private readonly AccOverlayConfiguration _config = new();
        private class AccOverlayConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Dock", "Provides settings for overlay docking.")]
            public DockConfigGrouping Dock { get; set; } = new DockConfigGrouping();
            public class DockConfigGrouping
            {
                [ToolTip("Allows you to reposition this debug panel.")]
                public bool Undock { get; set; } = false;
            }

            public AccOverlayConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private InfoPanel _panel;

        public AccProcessOverlay(Rectangle rectangle) : base(rectangle, "ACC Process")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 10;
            _panel = new InfoPanel(9, 350);
            this.Width = 350;
            this.Height = _panel.FontHeight * 10;
        }

        private void Instance_WidthChanged(object sender, bool e)
        {
            if (e)
                this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public sealed override void BeforeStart()
        {
            if (this._config.Dock.Undock)
                this.AllowReposition = true;
            else
            {
                DebugInfoHelper.Instance.WidthChanged += Instance_WidthChanged;
                DebugInfoHelper.Instance.AddOverlay(this);
                this.X = DebugInfoHelper.Instance.GetX(this);
                this.Y = 0;
            }
        }

        public sealed override void BeforeStop()
        {
            if (!this._config.Dock.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }
        }

        public override void Render(Graphics g)
        {

            _panel.AddLine("Process Alive", $"{AccProcess.IsRunning}");
            _panel.Draw(g);
        }

        public override bool ShouldRender() => true;
    }
}
