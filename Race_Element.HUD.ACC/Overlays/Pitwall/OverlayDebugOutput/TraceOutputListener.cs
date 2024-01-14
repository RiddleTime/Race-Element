using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput;

public class TraceOutputListener
{
    public struct MessageOut
    {
        public long time;
        public string message;
    }


    private Stream _outputStream;
    private TextWriterTraceListener _traceListener;
    private long lastPosition = -1;

    private LinkedList<MessageOut> _outputs = new();
    public LinkedList<MessageOut> Outputs { get { lock (_outputs) return _outputs; } }

    private static TraceOutputListener _instance;
    public static TraceOutputListener Instance
    {
        get
        {
            _instance ??= new TraceOutputListener(); return _instance;
        }
    }

    private bool _isRunning;

    public TraceOutputListener()
    {
        _outputStream = new MemoryStream();
        _traceListener = new TextWriterTraceListener(new StreamWriter(_outputStream));

        Trace.Listeners.Add(_traceListener);
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

                StreamReader reader = new(_outputStream);

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
                        _outputs.AddFirst(new MessageOut() { message = reader.ReadLine(), time = DateTime.Now.ToFileTime() });
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
