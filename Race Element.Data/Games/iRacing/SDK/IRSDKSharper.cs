using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.iRacing.SDK
{
    public class IRSDKSharper
    {
        private const string MapName = "Local\\IRSDKMemMapFileName";
        private const string EventName = "Local\\IRSDKDataValidEvent";
        private const string BroadcastMessageName = "IRSDK_BROADCASTMSG";

        public readonly IRacingSdkData Data;

        public bool IsStarted { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;

        public int UpdateInterval { get; set; } = 1;

        public event Action<Exception>? OnException = null;
        public event Action? OnConnected = null;
        public event Action? OnDisconnected = null;
        public event Action? OnSessionInfo = null;
        public event Action? OnTelemetryData = null;
        public event Action? OnStopped = null;

        private int isStarted = 0;

        private bool connectionLoopRunning = false;
        private bool telemetryDataLoopRunning = false;
        private bool sessionInfoLoopRunning = false;

        private MemoryMappedFile? memoryMappedFile = null;
        private MemoryMappedViewAccessor? memoryMappedViewAccessor = null;

        private AutoResetEvent? simulatorAutoResetEvent = null;
        private AutoResetEvent? sessionInfoAutoResetEvent = null;

        private int lastTelemetryDataUpdate = -1;

        private int lastSessionInfoUpdate = 0;
        private int sessionInfoUpdateChangedCount = 0;
        private int sessionInfoUpdateReady = 0;

        private readonly uint broadcastWindowMessage = Windows.RegisterWindowMessage(BroadcastMessageName);

        private EventSystem? eventSystem = null;

        /// <summary>
        /// <para>Welcome to IRSDKSharper!</para>
        /// This is the basic process to start it up:
        /// <code>
        /// var irsdk = new IRSDKSharper();
        /// 
        /// irsdk.OnException += OnException;
        /// irsdk.OnConnected += OnConnected;
        /// irsdk.OnDisconnected += OnDisconnected;
        /// irsdk.OnTelemetryData += OnTelemetryData;
        /// irsdk.OnSessionInfo += OnSessionInfo;
        /// irsdk.OnStopped += OnStopped;
        /// 
        /// irsdkSharper.Start();
        /// </code>
        /// </summary>
        /// <param name="throwYamlExceptions">Set this to true to throw exceptions when our IRacingSdkSessionInfo class is missing properties that exist in the YAML data string.</param>
        public IRSDKSharper(bool throwYamlExceptions = false)
        {
            Debug.WriteLine("IRSDKSharper is being instantiated.");

            Data = new(throwYamlExceptions);
        }

        public void Start()
        {
            if (Interlocked.Exchange(ref isStarted, 1) == 1)
            {
                Debug.WriteLine("IRSDKSharper has already been started or is starting.");
            }
            else
            {
                Debug.WriteLine("IRSDKSharper is starting.");

                Task.Run(ConnectionLoop);

                IsStarted = true;

                Debug.WriteLine("IRSDKSharper has been started.");
            }
        }

        public void Stop()
        {
            if (Interlocked.Exchange(ref isStarted, 0) == 0)
            {
                Debug.WriteLine("IRSDKSharper has already been stopped or is stopping.");
            }
            else
            {
                Debug.WriteLine("IRSDKSharper is stopping.");

                Task.Run(() =>
                {
                    if (sessionInfoLoopRunning)
                    {
                        Debug.WriteLine("Waiting for session info loop to stop.");

                        sessionInfoAutoResetEvent?.Set();

                        while (sessionInfoLoopRunning)
                        {
                            Thread.Sleep(0);
                        }
                    }

                    if (telemetryDataLoopRunning)
                    {
                        Debug.WriteLine("Waiting for telemetry data loop to stop.");

                        while (telemetryDataLoopRunning)
                        {
                            Thread.Sleep(0);
                        }
                    }

                    Data.Reset();

                    if (connectionLoopRunning)
                    {
                        Debug.WriteLine("Waiting for connection loop to stop.");

                        while (connectionLoopRunning)
                        {
                            Thread.Sleep(0);
                        }
                    }

                    IsStarted = false;
                    IsConnected = false;

                    memoryMappedFile = null;
                    memoryMappedViewAccessor = null;

                    simulatorAutoResetEvent = null;
                    sessionInfoAutoResetEvent = null;

                    lastTelemetryDataUpdate = -1;

                    lastSessionInfoUpdate = 0;
                    sessionInfoUpdateChangedCount = 0;
                    sessionInfoUpdateReady = 0;

                    eventSystem?.Reset();

                    Debug.WriteLine("IRSDKSharper has been stopped.");

                    OnStopped?.Invoke();
                });
            }
        }

        /// <summary>
        /// This event system feature is a work in progress - please do not use it yet.
        /// </summary>
        public void EnableEventSystem(string? directory)
        {
            eventSystem = null;

            if (directory != null)
            {
                eventSystem = new EventSystem(directory);
            }
        }

        #region simulator remote control

        public void CamSwitchPos(IRacingSdkEnum.CamSwitchMode camSwitchMode, int carPosition, int group, int camera)
        {
            if (camSwitchMode != IRacingSdkEnum.CamSwitchMode.FocusAtDriver)
            {
                carPosition = (int)camSwitchMode;
            }

            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.CamSwitchPos, (short)carPosition, (short)group, (short)camera);
        }

        public void CamSwitchNum(IRacingSdkEnum.CamSwitchMode camSwitchMode, int carNumberRaw, int group, int camera)
        {
            if (camSwitchMode != IRacingSdkEnum.CamSwitchMode.FocusAtDriver)
            {
                carNumberRaw = (int)camSwitchMode;
            }

            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.CamSwitchNum, (short)carNumberRaw, (short)group, (short)camera);
        }

        public void CamSetState(IRacingSdkEnum.CameraState cameraState)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.CamSetState, (short)cameraState);
        }

        public void ReplaySetPlaySpeed(int speed, bool slowMotion)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ReplaySetPlaySpeed, (short)speed, slowMotion ? 1 : 0);
        }

        public void ReplaySetPlayPosition(IRacingSdkEnum.RpyPosMode rpyPosMode, int frameNumber)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ReplaySetPlayPosition, (short)rpyPosMode, frameNumber);
        }

        public void ReplaySearch(IRacingSdkEnum.RpySrchMode rpySrchMode)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ReplaySearch, (short)rpySrchMode);
        }

        public void ReplaySetState(IRacingSdkEnum.RpyStateMode rpyStateMode)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ReplaySetState, (short)rpyStateMode);
        }

        public void ReloadTextures(IRacingSdkEnum.ReloadTexturesMode reloadTexturesMode, int carIdx)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ReloadTextures, (short)reloadTexturesMode, carIdx);
        }

        public void ChatComand(IRacingSdkEnum.ChatCommandMode chatCommandMode, int subCommand)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ChatComand, (short)chatCommandMode, subCommand);
        }

        public void PitCommand(IRacingSdkEnum.PitCommandMode pitCommandMode, int parameter)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.PitCommand, (short)pitCommandMode, parameter);
        }

        public void TelemCommand(IRacingSdkEnum.TelemCommandMode telemCommandMode)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.TelemCommand, (short)telemCommandMode);
        }

        public void FFBCommand(IRacingSdkEnum.FFBCommandMode ffbCommandMode, float value)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.FFBCommand, (short)ffbCommandMode, value);
        }

        public void ReplaySearchSessionTime(int sessionNum, int sessionTimeMS)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.ReplaySearchSessionTime, (short)sessionNum, sessionTimeMS);
        }

        public void VideoCapture(IRacingSdkEnum.VideoCaptureMode videoCaptureMode)
        {
            BroadcastMessage(IRacingSdkEnum.BroadcastMsg.VideoCapture, (short)videoCaptureMode);
        }

        #endregion

        #region broadcast message functions

        private void BroadcastMessage(IRacingSdkEnum.BroadcastMsg msg, short var1, int var2 = 0)
        {
            if (!Windows.PostMessage((IntPtr)0xFFFF, broadcastWindowMessage, Windows.MakeLong((short)msg, var1), (IntPtr)var2))
            {
                int errorCode = Marshal.GetLastWin32Error();

                Marshal.ThrowExceptionForHR(errorCode, IntPtr.Zero);
            }
        }

        private void BroadcastMessage(IRacingSdkEnum.BroadcastMsg msg, short var1, float var2)
        {
            if (!Windows.PostMessage((IntPtr)0xFFFF, broadcastWindowMessage, Windows.MakeLong((short)msg, var1), (IntPtr)(var2 * 65536.0f)))
            {
                int errorCode = Marshal.GetLastWin32Error();

                Marshal.ThrowExceptionForHR(errorCode, IntPtr.Zero);
            }
        }

        private void BroadcastMessage(IRacingSdkEnum.BroadcastMsg msg, short var1, short var2, short var3)
        {
            if (!Windows.PostMessage((IntPtr)0xFFFF, broadcastWindowMessage, Windows.MakeLong((short)msg, var1), Windows.MakeLong(var2, var3)))
            {
                int errorCode = Marshal.GetLastWin32Error();

                Marshal.ThrowExceptionForHR(errorCode, IntPtr.Zero);
            }
        }

        #endregion

        #region background tasks

        private void ConnectionLoop()
        {
            Debug.WriteLine("Connection loop has been started.");

            try
            {
                connectionLoopRunning = true;

                while (isStarted == 1)
                {
                    if (memoryMappedFile == null)
                    {
                        try
                        {
                            memoryMappedFile = MemoryMappedFile.OpenExisting(MapName);
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }

                    if (memoryMappedFile != null)
                    {
                        Debug.WriteLine("memoryMappedFile != null");

                        memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor();

                        if (memoryMappedViewAccessor == null)
                        {
                            throw new Exception("Failed to create memory mapped view accessor.");
                        }

                        Data.SetMemoryMappedViewAccessor(memoryMappedViewAccessor);

                        var hEvent = Windows.OpenEvent(Windows.EVENT_ALL_ACCESS, false, EventName);

                        if (hEvent == (IntPtr)null)
                        {
                            int errorCode = Marshal.GetLastWin32Error();

                            Marshal.ThrowExceptionForHR(errorCode, IntPtr.Zero);
                        }
                        else
                        {
                            simulatorAutoResetEvent = new AutoResetEvent(false)
                            {
                                SafeWaitHandle = new SafeWaitHandle(hEvent, true)
                            };

                            sessionInfoAutoResetEvent = new AutoResetEvent(false);

                            Task.Run(SessionInfoLoop);
                            Task.Run(TelemetryDataLoop);
                        }

                        break;
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }

                connectionLoopRunning = false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception caught inside the connection loop.");

                connectionLoopRunning = false;

                OnException?.Invoke(exception);
            }

            Debug.WriteLine("Connection loop has been stopped.");
        }

        private void SessionInfoLoop()
        {
            Debug.WriteLine("Session info loop has been started.");

            try
            {
                sessionInfoLoopRunning = true;

                while (isStarted == 1)
                {
                    Debug.WriteLine("Waiting for session info event.");

                    sessionInfoAutoResetEvent?.WaitOne();

                    while (sessionInfoUpdateChangedCount > 0)
                    {
                        if (isStarted == 1)
                        {
                            Debug.WriteLine("Updating session info.");

                            if (Data.UpdateSessionInfo())
                            {
                                eventSystem?.Update(Data);

                                sessionInfoUpdateReady = 1;
                            }
                        }

                        Interlocked.Decrement(ref sessionInfoUpdateChangedCount);
                    }
                }

                sessionInfoLoopRunning = false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception caught inside the session info loop.");

                sessionInfoLoopRunning = false;

                OnException?.Invoke(exception);
            }

            Debug.WriteLine("Session info loop has been stopped.");
        }

        private void TelemetryDataLoop()
        {
            Debug.WriteLine("Telemetry data loop has been started.");

            try
            {
                telemetryDataLoopRunning = true;

                while (isStarted == 1)
                {
                    var signalReceived = simulatorAutoResetEvent?.WaitOne(250) ?? false;

                    if (signalReceived)
                    {
                        if (!IsConnected)
                        {
                            Debug.WriteLine("The iRacing simulator is running.");

                            IsConnected = true;

                            lastTelemetryDataUpdate = -1;

                            lastSessionInfoUpdate = 0;
                            sessionInfoUpdateReady = 0;

                            OnConnected?.Invoke();
                        }

                        Data.Update();

                        if (lastSessionInfoUpdate != Data.SessionInfoUpdate)
                        {
                            Debug.WriteLine("Data.SessionInfoUpdate has changed.");

                            lastSessionInfoUpdate = Data.SessionInfoUpdate;

                            Interlocked.Increment(ref sessionInfoUpdateChangedCount);

                            sessionInfoAutoResetEvent?.Set();
                        }

                        eventSystem?.Record(Data);

                        if (Interlocked.Exchange(ref sessionInfoUpdateReady, 0) == 1)
                        {
                            Debug.WriteLine("Invoking OnSessionInfo.");

                            OnSessionInfo?.Invoke();
                        }

                        if ((Data.TickCount - lastTelemetryDataUpdate) >= UpdateInterval)
                        {
                            lastTelemetryDataUpdate = Data.TickCount;

                            OnTelemetryData?.Invoke();
                        }
                    }
                    else
                    {
                        if (IsConnected)
                        {
                            Debug.WriteLine("The iRacing simulator is no longer running.");

                            if (sessionInfoUpdateChangedCount > 0)
                            {
                                Debug.WriteLine("Draining sessionInfoUpdateChangedCount.");

                                while (sessionInfoUpdateChangedCount > 0)
                                {
                                    Thread.Sleep(0);
                                }
                            }

                            IsConnected = false;

                            Debug.WriteLine("Invoking OnDisconnected.");

                            OnDisconnected?.Invoke();

                            Data.Reset();

                            eventSystem?.Reset();
                        }
                    }
                }

                telemetryDataLoopRunning = false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception caught inside the telemetry data loop.");

                telemetryDataLoopRunning = false;

                OnException?.Invoke(exception);
            }

            Debug.WriteLine("Telemetry data loop has been stopped.");
        }

        #endregion
    }
}
