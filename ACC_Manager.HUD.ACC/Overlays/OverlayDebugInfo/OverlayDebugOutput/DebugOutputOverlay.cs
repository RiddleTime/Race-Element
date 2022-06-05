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

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput
{
    internal class DebugOutputOverlay : AbstractOverlay
    {
        private DebugOutputConfiguration _config = new DebugOutputConfiguration();
        private class DebugOutputConfiguration : OverlayConfiguration
        {
            [ToolTip("Allows you to reposition this debug panel.")]
            internal bool Undock { get; set; } = false;

            [IntRange(5, 50, 1)]
            public int VisibleLines { get; set; } = 10;

            [IntRange(400, 1000, 1)]
            public int Width { get; set; } = 680;

            public DebugOutputConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private Stream _outputStream;
        private TextWriterTraceListener _traceListener;
        private long lastPosition = -1;

        private struct DebugOut
        {
            public long time;
            public string message;
        }

        private LinkedList<DebugOut> _outputs = new LinkedList<DebugOut>();
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

            _outputStream = new MemoryStream();
            _traceListener = new TextWriterTraceListener(new StreamWriter(_outputStream));
            Debug.Listeners.Add(_traceListener);
            Debug.AutoFlush = true;

            this.Height = (int)((_font.Height - 2) * _config.VisibleLines) + 1;

        }

        public override void BeforeStop()
        {
            if (!this._config.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }

            _traceListener.Flush();
            Trace.Listeners.Remove(_traceListener);
            _outputStream.Close();
        }

        public override void Render(Graphics g)
        {
            StreamReader reader = new StreamReader(_outputStream);

            if (lastPosition == -1)
                lastPosition = _outputStream.Position;
            else
                _outputStream.Position = lastPosition;

            while (!reader.EndOfStream)
            {
                if (_outputs.Count >= _config.VisibleLines)
                    _outputs.RemoveLast();
                _outputs.AddFirst(new DebugOut() { message = reader.ReadLine(), time = DateTime.Now.ToFileTime() });
            }
            lastPosition = _outputStream.Position;


            foreach (DebugOut output in _outputs)
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
