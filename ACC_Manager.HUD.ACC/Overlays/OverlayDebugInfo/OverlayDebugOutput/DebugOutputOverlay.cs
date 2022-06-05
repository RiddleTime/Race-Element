using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput.DebugOutputListener;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput
{
    internal class DebugOutputOverlay : AbstractOverlay
    {
        private DebugOutputConfiguration _config = new DebugOutputConfiguration();
        private class DebugOutputConfiguration : OverlayConfiguration
        {
            [ToolTip("Allows you to reposition this debug panel.")]
            internal bool Undock { get; set; } = false;

            [ToolTip("The amount of lines displayed.")]
            [IntRange(5, 50, 1)]
            public int VisibleLines { get; set; } = 10;

            [ToolTip("Sets the width, allows you to see more.")]
            [IntRange(400, 1000, 1)]
            public int Width { get; set; } = 680;

            public DebugOutputConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private Font _font;
        private InfoTable _table;

        public DebugOutputOverlay(Rectangle rectangle) : base(rectangle, "Debug Output Overlay")
        {
            this.AllowReposition = false;

            int fontSize = 9;
            _font = FontUtil.FontOrbitron(fontSize);
            _table = new InfoTable(fontSize, new int[] { 600 });
            RefreshRateHz = 5;
            this.Width = _config.Width;
        }

        private void Instance_WidthChanged(object sender, bool e)
        {
            if (e)
                this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public override void BeforeStart()
        {
            if (this._config.Undock)
                this.AllowReposition = true;
            else
            {
                DebugInfoHelper.Instance.WidthChanged += Instance_WidthChanged;
                DebugInfoHelper.Instance.AddOverlay(this);
                this.X = DebugInfoHelper.Instance.GetX(this);
                this.Y = 0;
            }

            this.Height = (int)((_font.Height - 2) * _config.VisibleLines) + 1;
        }

        public override void BeforeStop()
        {
            if (!this._config.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }
        }

        public override void Render(Graphics g)
        {
            foreach (DebugOut output in DebugOutputListener.Instance.Outputs)
            {
                DateTime time = DateTime.FromFileTime(output.time);
                _table.AddRow($"{time:HH\\:mm\\:ss}", new string[] { output.message });
            }

            _table.Draw(g);
        }

        public override bool ShouldRender()
        {
            return true;
        }
    }
}
