using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;

namespace RaceElement.Data.Games.BeamNG;

internal sealed class BeamNGDataProvider : AbstractSimDataProvider
{
    private Thread? _outGaugeThread;
    private Thread? _motionSimThread;
    private UdpClient? _outGaugeClient;
    private UdpClient? _motionSimClient;
    private OutGaugePacket _latestOutGauge = new();
    private MotionSimPacket _latestMotionSim = new();
    private bool _isRunning;

    // Common default port from community tools (SimHub, RealDash, etc.)
    // Often users set both to same port, but to avoid conflict we can use separate or same if broadcasting
    // For basics: start with common 4444 for OutGauge, and perhaps 4445 for MotionSim if separate needed
    // But many setups use same port for both if the receiver can distinguish by packet header
    private const int OutGaugePort = 4444;
    private const int MotionSimPort = 4444; // Many use same; packets differ by content/format

    public override List<string> GetCarClasses() => ["Various (BeamNG.drive vehicles)"];

    public override bool HasTelemetry() => true;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        // Use MotionSim for physics/location if available, fallback to basics
        if (_latestMotionSim.format?[0] != 'B' || _latestMotionSim.format?[1] != 'N' || _latestMotionSim.format?[2] != 'G' || _latestMotionSim.format?[3] != '1')
        {
            // No valid MotionSim data → consider paused/no sim
            localCar = new();
            sessionData = new();
            gameData.IsGamePaused = true;
            return;
        }

        gameData.IsGamePaused = false;

        // Basic mapping from MotionSim (better for position/velocity/acc)
        localCar.Physics.Location = new(_latestMotionSim.posX, _latestMotionSim.posY, _latestMotionSim.posZ);

        // Approximate speed from velocity vector magnitude
        float velMagnitude = (float)Math.Sqrt(
            _latestMotionSim.velX * _latestMotionSim.velX +
            _latestMotionSim.velY * _latestMotionSim.velY +
            _latestMotionSim.velZ * _latestMotionSim.velZ);
        localCar.Physics.Velocity = velMagnitude * 3.6f; // m/s → km/h

        // Acceleration (gravity not included, per docs)
        localCar.Physics.Acceleration = new(
            _latestMotionSim.accX / -9.81f,   // lateral-ish, but actually world axes → approximate
            _latestMotionSim.accZ / 9.81f,
            _latestMotionSim.accY / 9.81f);

        // From OutGauge (inputs, rpm, gear, etc.)
        localCar.Inputs.Throttle = _latestOutGauge.throttle;
        localCar.Inputs.Brake = _latestOutGauge.brake;
        localCar.Inputs.Clutch = _latestOutGauge.clutch;
        localCar.Inputs.Steering = 0f; // Not in basic OutGauge


        localCar.Inputs.Gear = (sbyte)_latestOutGauge.Gear;

        localCar.Engine.Rpm = (int)_latestOutGauge.rpm;
        //localCar.Engine.MaxRpm = 8000; // BeamNG varies; no field → placeholder
        localCar.Engine.IsRunning = localCar.Engine.Rpm > 500; // rough guess


        //localCar.Tyres.SlipRatio = new float[4]; need to find a way to better data

    }

    internal override int PollingRate() => 200; // Matches your original

    internal override void Start()
    {
        _isRunning = true;

        _outGaugeThread = new Thread(ListenOutGauge) { IsBackground = true };
        _outGaugeThread.Start();

        _motionSimThread = new Thread(ListenMotionSim) { IsBackground = true };
        _motionSimThread.Start();
    }

    internal override void Stop()
    {
        _isRunning = false;
        _outGaugeClient?.Close();
        _motionSimClient?.Close();
        _outGaugeThread?.Join(1500);
        _motionSimThread?.Join(1500);
    }

    private void ListenOutGauge()
    {
        try
        {
            _outGaugeClient = new UdpClient(OutGaugePort);
            IPEndPoint remote = new(IPAddress.Any, 0);

            while (_isRunning)
            {
                try
                {
                    byte[] data = _outGaugeClient.Receive(ref remote);
                    if (data.Length >= Marshal.SizeOf<OutGaugePacket>())
                    {
                        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        try
                        {
                            _latestOutGauge = Marshal.PtrToStructure<OutGaugePacket>(handle.AddrOfPinnedObject());
                        }
                        finally { handle.Free(); }
                    }
                }
                catch (SocketException) when (!_isRunning) { }
                catch (Exception ex) { Debug.WriteLine($"OutGauge error: {ex.Message}"); }
            }
        }
        catch (Exception ex) { Debug.WriteLine($"OutGauge listener failed: {ex.Message}"); }
        finally { _outGaugeClient?.Close(); }
    }

    private void ListenMotionSim()
    {
        try
        {
            _motionSimClient = new UdpClient(MotionSimPort);
            IPEndPoint remote = new(IPAddress.Any, 0);

            while (_isRunning)
            {
                try
                {
                    byte[] data = _motionSimClient.Receive(ref remote);
                    if (data.Length >= Marshal.SizeOf<MotionSimPacket>())
                    {
                        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        try
                        {
                            _latestMotionSim = Marshal.PtrToStructure<MotionSimPacket>(handle.AddrOfPinnedObject());
                        }
                        finally { handle.Free(); }
                    }
                }
                catch (SocketException) when (!_isRunning) { }
                catch (Exception ex) { Debug.WriteLine($"MotionSim error: {ex.Message}"); }
            }
        }
        catch (Exception ex) { Debug.WriteLine($"MotionSim listener failed: {ex.Message}"); }
        finally { _motionSimClient?.Close(); }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
internal struct OutGaugePacket
{
    public uint Time;               // ms, usually 0
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public char[] Car;              // "beam"
    public ushort Flags;
    public char Gear;
    public char plid;               // usually 0
    public float Speed;             // m/s
    public float rpm;
    public float turbo;             // bar
    public float engTemp;           // °C
    public float fuel;              // 0-1
    public float oilPressure;       // bar, usually 0
    public float oilTemp;           // °C
    public uint dashLights;
    public uint showLights;
    public float throttle;          // 0-1
    public float brake;             // 0-1
    public float clutch;            // 0-1
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public char[] display1;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public char[] display2;
    public int id;                  // optional
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MotionSimPacket
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public char[] format;           // "BNG1"
    public float posX, posY, posZ;
    public float velX, velY, velZ;
    public float accX, accY, accZ;
    public float upX, upY, upZ;
    public float rollPos, pitchPos, yawPos;
    public float rollVel, pitchVel, yawVel;
    public float rollAcc, pitchAcc, yawAcc;
}