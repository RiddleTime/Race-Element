using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput
{
    internal class DebugOutputListener
    {
        public struct DebugOut
        {
            public long time;
            public string message;
        }


        private Stream _outputStream;
        private TextWriterTraceListener _traceListener;
        private long lastPosition = -1;

        private LinkedList<DebugOut> _outputs = new LinkedList<DebugOut>();
        public LinkedList<DebugOut> Outputs
        {
            get
            {
                lock (_outputs)
                {
                    return _outputs;
                }
            }
        }

        private static DebugOutputListener _instance;
        internal static DebugOutputListener Instance
        {
            get
            {
                if (_instance == null) _instance = new DebugOutputListener(); return _instance;
            }
        }

        private bool _isRunning;

        public DebugOutputListener()
        {
            _outputStream = new MemoryStream();
            _traceListener = new TextWriterTraceListener(new StreamWriter(_outputStream));
            Debug.Listeners.Add(_traceListener);
            Debug.AutoFlush = true;

            Start();
        }

        public void Start()
        {
            _isRunning = true;
            new Thread(x =>
            {
                while (_isRunning)
                {
                    Thread.Sleep(100);

                    StreamReader reader = new StreamReader(_outputStream);

                    if (lastPosition == -1)
                        lastPosition = 0;
                    else
                        _outputStream.Position = lastPosition;

                    while (!reader.EndOfStream)
                    {
                        lock (_outputs)
                        {
                            if (_outputs.Count >= 100)
                                _outputs.RemoveLast();
                            _outputs.AddFirst(new DebugOut() { message = reader.ReadLine(), time = DateTime.Now.ToFileTime() });
                        }
                    }

                    lastPosition = _outputStream.Position;
                }
            }).Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _traceListener.Flush();
            Trace.Listeners.Remove(_traceListener);
            _outputStream.Close();

            Debug.AutoFlush = false;
            _outputStream.Position = 0;
        }
    }
}
