using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.RichardBurnsRally.UDP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using static RaceElement.Data.Games.RichardBurnsRally.RBRMemoryReader;

namespace RaceElement.Data.Games.RichardBurnsRally;

internal sealed class RBRDataProvider : AbstractSimDataProvider
{
    private Thread? _listenerThread;
    private UdpClient? _udpClient;
    private RBRTelemetryData _latestData;
    private bool _isRunning;
    private bool _hasReceivedData;
    private int _udpPort = 6776;
    private RBRMemoryReader? _memoryReader;

    public sealed override List<string> GetCarClasses() => ["Group A", "Group B", "Group N", "WRC", "Kit Car", "F2"];

    internal override int PollingRate() => 300;


    private uint _lastStep = uint.MinValue;
    private long _sameStepBuffer = 0;
    private const long _maxBufferCount = 100;
    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        RBRTelemetryData data = _latestData;

        bool isSameStep = false;
        if (data.TotalSteps == _lastStep)
        {
            _sameStepBuffer++;
            if (_sameStepBuffer > _maxBufferCount)
                isSameStep = true;
        }
        else
        {
            _lastStep = data.TotalSteps;
            _sameStepBuffer = 0;
        }

        if (!_hasReceivedData || isSameStep)
        {
            localCar = new();
            sessionData = new();
            gameData.IsGamePaused = true;
            return;
        }

        gameData.IsGamePaused = false;
        gameData.Name = Game.RichardBurnsRally.ToShortName();

        // Physics - RBR Car.Speed is in m/s, convert to km/h
        localCar.Physics.Location = new(data.CarPositionX, data.CarPositionY, data.CarPositionZ);
        localCar.Physics.Velocity = data.CarSpeed;
        localCar.Physics.Acceleration = new(-data.AccSway, data.AccHeave, data.AccSurge);
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
        // RBR NGP temperatures are in Kelvin, convert to Celsius
        localCar.Engine.WaterTemperature = data.EngineCoolantTemp - 273.15f;
        localCar.Engine.OilTemperature = data.EngineTemp - 273.15f;


        // SlipRatio - Use memory reading for accurate wheel speeds (based on Adaptive_Trigger_RBR.py)
        _memoryReader ??= new RBRMemoryReader();
        if (_memoryReader.TryReadWheelSpeeds(out WheelSpeeds wheelSpeeds))
        {
            // Calculate real slip ratio using actual wheel speeds
            // ground_speed is in km/h, wheel speeds are in km/h
            float groundSpeedKmh = MathF.Sqrt(data.VelSurge * data.VelSurge + data.VelSway * data.VelSway + data.VelHeave * data.VelHeave) * 3.6f;

            if (groundSpeedKmh > 5.0f) // Only calculate when moving > 5 km/h
            {
                // Slip ratio = ((wheel_speed / ground_speed) - 1) * 100
                float flSlip = ((wheelSpeeds.FrontLeft / groundSpeedKmh) - 1.0f) * 100.0f;
                float frSlip = ((wheelSpeeds.FrontRight / groundSpeedKmh) - 1.0f) * 100.0f;
                float rlSlip = ((wheelSpeeds.RearLeft / groundSpeedKmh) - 1.0f) * 100.0f;
                float rrSlip = ((wheelSpeeds.RearRight / groundSpeedKmh) - 1.0f) * 100.0f;

                localCar.Tyres.SlipRatio = [flSlip, frSlip, rlSlip, rrSlip];
            }
            else
            {
                localCar.Tyres.SlipRatio = [0, 0, 0, 0];
            }
        }
        else
        {
            // Fallback: use input-based approximation if memory reading fails
            float brake = data.ControlBrake;
            float throttle = data.ControlThrottle;
            float longAccG = data.AccSurge / 9.80665f;
            float brakeSlip = brake * (0.7f + Math.Min(0.3f, Math.Abs(longAccG)));
            float throttleSlip = throttle * (0.7f + Math.Min(0.3f, Math.Max(0, longAccG)));
            localCar.Tyres.SlipRatio = [brakeSlip, brakeSlip, throttleSlip, throttleSlip];
        }
    }

    internal override void Start()
    {
        GamePortSettings gamePortSettings = new();
        gamePortSettings.Get().GamePorts.TryGetValue(GameManager.CurrentGame, out _udpPort);

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
        _memoryReader?.Dispose();
        _memoryReader = null;
    }

    private void ListenForTelemetry()
    {
        try
        {
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, _udpPort));
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

    public sealed override bool HasTelemetry() => false;
}
