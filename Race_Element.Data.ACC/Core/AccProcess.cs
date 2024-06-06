using RaceElement.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace RaceElement.Data.ACC.Core;

public sealed class AccProcess
{
    private static ProcessTracker _handlerInstance;
    private static ProcessTracker Handler
    {
        get
        {
            _handlerInstance ??= new ProcessTracker("AC2-Win64-Shipping");
            return _handlerInstance;
        }
    }

    public static bool IsRunning { get => Handler.IsRunning; }

    private sealed class ProcessTracker : IDisposable
    {
        public bool IsRunning = false;

        public event EventHandler<bool> IsRunningChanged;

        private Process _acc;

        private readonly string _name;

        private bool _shouldTrack = true;
        private DateTime _lastCheck = DateTime.MinValue;

        public ProcessTracker(string name)
        {
            _name = name;
            FindProcess();
            TrackProcess();
        }

        private void TrackProcess()
        {
            new Thread(x =>
            {
                while (_shouldTrack)
                {
                    Thread.Sleep(5000);

                    try
                    {
                        if (!IsRunning || _acc == null)
                            FindProcess();
                        else if (_acc != null && _acc.Handle != IntPtr.Zero && _acc.HasExited)
                            FindProcess();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        LogWriter.WriteToLog(e);
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        private void FindProcess()
        {
            if (_lastCheck.AddSeconds(3) < DateTime.UtcNow)
            {
                if (!IsRunning || _acc == null || _acc.HasExited)
                {
                    Process processes = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == _name);
                    if (processes != null)
                    {
                        IsRunning = true;
                        IsRunningChanged?.Invoke(this, IsRunning);
                    }
                    else
                    {
                        IsRunning = false;
                        IsRunningChanged?.Invoke(this, IsRunning);
                    }
                }

                _lastCheck = DateTime.UtcNow;
            }
        }

        public void Dispose()
        {
            _acc?.Dispose();
            _acc = null;
            _shouldTrack = false;
        }
    }
}
