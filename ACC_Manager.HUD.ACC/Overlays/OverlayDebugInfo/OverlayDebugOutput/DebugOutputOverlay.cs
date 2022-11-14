using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Linq;
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput.TraceOutputListener;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput
{
    [Overlay(Name = "Debug Output", Version = 1.00, OverlayType = OverlayType.Debug,
        Description = "A panel showing live debug output.")]
    internal sealed class DebugOutputOverlay : AbstractOverlay
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

        private readonly Font _font;
        private InfoTable _table;

        public DebugOutputOverlay(Rectangle rectangle) : base(rectangle, "Debug Output Overlay")
        {
            this.AllowReposition = false;

            int fontSize = 9;
            _font = FontUtil.FontOrbitron(fontSize);
            _table = new InfoTable(fontSize, new int[] { _config.Width - 66 });
            this.Width = _config.Width + 1;
        }

        private void Instance_WidthChanged(object sender, bool e)
        {
            if (e)
                this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public sealed override void BeforeStart()
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

        public sealed override void BeforeStop()
        {
            if (!this._config.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }
        }

        public sealed override void Render(Graphics g)
        {
            lock (TraceOutputListener.Instance.Outputs)
                foreach (MessageOut output in TraceOutputListener.Instance.Outputs.Take(_config.VisibleLines))
                {
                    DateTime time = DateTime.FromFileTime(output.time);
                    _table.AddRow($"{time:HH\\:mm\\:ss}", new string[] { output.message });
                }

            _table.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }
    }
}
