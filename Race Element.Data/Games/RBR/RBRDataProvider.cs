using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.RBR.UDP;
using Game = RaceElement.Data.Games.Game;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.RBR;

internal sealed class RBRDataProvider : AbstractSimDataProvider
{
    private Thread? _listenerThread;
    private UdpClient? _udpClient;
    private RBRTelemetryData _latestData;
    private bool _isRunning;
    private bool _hasReceivedData;
    private const int Port = 6776;

    public sealed override List<string> GetCarClasses() => ["Group A", "Group B", "Group N", "WRC", "Kit Car", "F2"];

    public sealed override bool HasTelemetry() => false;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        if (!_hasReceivedData)
        {
            localCar = new();
            sessionData = new();
            gameData.IsGamePaused = true;
            return;
        }

        gameData.IsGamePaused = false;
        gameData.Name = Game.RBR.ToShortName();

        var data = _latestData;

        // Physics - RBR Car.Speed is in m/s, convert to km/h
        localCar.Physics.Location = new(data.CarPositionX, data.CarPositionY, data.CarPositionZ);
        localCar.Physics.Velocity = data.CarSpeed * 3.6f;
        localCar.Physics.Acceleration = new(data.AccSway / 9.80665f, data.AccHeave / 9.80665f, data.AccSurge / 9.80665f);
        localCar.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(
            data.CarYaw * (MathF.PI / 180f),
            data.CarPitch * (MathF.PI / 180f),
            data.CarRoll * (MathF.PI / 180f));

        // Inputs - RBR uses 0-1 for throttle/brake/clutch, -1 to 1 for steering
        localCar.Inputs.Throttle = data.ControlThrottle;
        localCar.Inputs.Brake = data.ControlBrake;
        localCar.Inputs.Clutch = data.ControlClutch;
        localCar.Inputs.HandBrake = data.ControlHandbrake;
        localCar.Inputs.Steering = data.ControlSteering;

        // Gear - RBR NGP: 0=reverse, 1=neutral, 2=1st, 3=2nd... (matches overlay: 0=R, 1=N, 2=1, 3=2...)
        localCar.Inputs.Gear = data.ControlGear;

        // Engine
        localCar.Engine.Rpm = (int)data.EngineRpm;
        localCar.Engine.MaxRpm = 8000;
        localCar.Engine.IsRunning = data.EngineRpm > 500;

        // SlipRatio - RBR NGP UDP does not provide wheel slip; use input-based approximation for DSX trigger haptics
        float brake = data.ControlBrake;
        float throttle = data.ControlThrottle;
        float longAccG = data.AccSurge / 9.80665f;
        float brakeSlip = brake * (0.7f + Math.Min(0.3f, Math.Abs(longAccG)));
        float throttleSlip = throttle * (0.7f + Math.Min(0.3f, Math.Max(0, longAccG)));
        localCar.Tyres.SlipRatio = [brakeSlip, brakeSlip, throttleSlip, throttleSlip];
    }

    internal override int PollingRate() => 60;

    internal override void Start()
    {
        _isRunning = true;
        _hasReceivedData = false;
        _listenerThread = new Thread(ListenForTelemetry)
        {
            IsBackground = true
        };
        _listenerThread.Start();
    }

    internal override void Stop()
    {
        _isRunning = false;
        _udpClient?.Close();
        _listenerThread?.Join(1000);
    }

    private void ListenForTelemetry()
    {
        try
        {
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var minSize = Marshal.SizeOf<RBRTelemetryData>();

            while (_isRunning)
            {
                try
                {
                    var receivedBytes = _udpClient.Receive(ref remoteEndPoint);
                    if (receivedBytes.Length >= minSize)
                    {
                        var handle = GCHandle.Alloc(receivedBytes, GCHandleType.Pinned);
                        try
                        {
                            _latestData = Marshal.PtrToStructure<RBRTelemetryData>(handle.AddrOfPinnedObject());
                            _hasReceivedData = true;
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                }
                catch (SocketException se) when (!_isRunning)
                {
                    Debug.WriteLine($"RBR UDP socket closed: {se}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RBR telemetry error: {ex}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"RBR UDP listener failed: {e}");
        }
        finally
        {
            _udpClient?.Close();
        }
    }
}
