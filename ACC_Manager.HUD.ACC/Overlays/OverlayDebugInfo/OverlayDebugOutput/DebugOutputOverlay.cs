using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Linq;
using static RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput.TraceOutputListener;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput
{
    [Overlay(Name = "Debug Output", Version = 1.00, OverlayType = OverlayType.Debug,
        Description = "A panel showing live debug output.")]
    internal sealed class DebugOutputOverlay : AbstractOverlay
    {
        private readonly DebugOutputConfiguration _config = new DebugOutputConfiguration();
        private class DebugOutputConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Output", "Provides settings for overlay docking.")]
            public OutputGrouping Output { get; set; } = new OutputGrouping();
            public class OutputGrouping
            {
                [ToolTip("The amount of lines displayed.")]
                [IntRange(5, 50, 1)]
                public int VisibleLines { get; set; } = 10;

                [ToolTip("Sets the width, allows you to see more.")]
                [IntRange(400, 1000, 1)]
                public int Width { get; set; } = 680;
            }

            [ConfigGrouping("Dock", "Provides settings for overlay docking.")]
            public DockConfigGrouping Dock { get; set; } = new DockConfigGrouping();
            public class DockConfigGrouping
            {
                [ToolTip("Allows you to reposition this debug panel.")]
                public bool Undock { get; set; } = false;
            }

            public DebugOutputConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private readonly Font _font;
        private InfoTable _table;

        public DebugOutputOverlay(Rectangle rectangle) : base(rectangle, "Debug Output")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 1;

            int fontSize = 9;
            _font = FontUtil.FontOrbitron(fontSize);
            _table = new InfoTable(fontSize, new int[] { _config.Output.Width - 66 });
            RefreshRateHz = 5;
            this.Width = _config.Output.Width + 1;
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

            this.Height = (int)((_font.Height - 2) * _config.Output.VisibleLines) + 1;
        }

        public sealed override void BeforeStop()
        {
            if (!this._config.Dock.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }
        }

        public sealed override void Render(Graphics g)
        {
            lock (TraceOutputListener.Instance.Outputs)
                foreach (MessageOut output in TraceOutputListener.Instance.Outputs.Take(_config.Output.VisibleLines))
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
