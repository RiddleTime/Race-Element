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
            [IntRange(5, 30, 1)]
            public int VisibleLines { get; set; } = 10;

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
            _font = FontUtil.FontOrbitron(9);
            _table = new InfoTable(_font.Size, new int[] { 600 });
            RefreshRateHz = 5;
            this.Width = 700;
        }

        public override void BeforeStart()
        {
            _outputStream = new MemoryStream();
            _traceListener = new TextWriterTraceListener(new StreamWriter(_outputStream));
            Debug.Listeners.Add(_traceListener);
            Debug.AutoFlush = true;

            this.Height = (int)(_font.Height * _config.VisibleLines) + 1;

        }

        public override void BeforeStop()
        {
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


            _table._headerWidthSet = false;
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
