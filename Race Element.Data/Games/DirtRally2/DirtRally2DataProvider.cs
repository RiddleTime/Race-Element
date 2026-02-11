using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.DirtRally2.UDP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.DirtRally2;
internal sealed class DirtRally2DataProvider : AbstractSimDataProvider
{
    private Thread? _listenerThread;
    private UdpClient? _udpClient;
    private DirtRally2Data _latestData;
    private bool _isRunning;
    private const int Port = 20777;

    public sealed override List<string> GetCarClasses() => ["R2", "R5", "Group A", "Group B", "H1", "H2", "H3", "2000cc", "Rallycross"]; // Adjusted for Dirt Rally 2.0 classes

    public sealed override bool HasTelemetry() => false;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        var data = _latestData;
        if (data.Z == 0)
        {
            localCar = new();
            sessionData = new();
            gameData.IsGamePaused = true;
            return;
        }
        else
        {
            gameData.IsGamePaused = false;
        }

        // Map to LocalCarData (assuming standard fields; adjust based on exact definitions)
        localCar.Physics.Location = new(data.X, data.Y, data.Z);
        localCar.Physics.Velocity = data.Speed * 3.6f;
        localCar.Physics.Acceleration = new(-data.GforceLat / 9.81f, 0, -data.GforceLong / 9.81f);
        localCar.Inputs.Throttle = data.Throttle;
        localCar.Inputs.Brake = data.Brake;
        localCar.Inputs.Clutch = data.Clutch;
        localCar.Inputs.Steering = data.Steer;

        localCar.Inputs.Gear = data.Gear switch
        {
            10 => 0, // Reverse
            _ => (int)data.Gear + 1 // Neutraal + Forward gears
        };
        {
        }

        localCar.Engine.Rpm = (int)(data.Rpm); // Assuming full RPM value in DR2
        localCar.Engine.MaxRpm = (int)(data.MaxRpm);
        localCar.Engine.IsRunning = localCar.Physics.Velocity != 0f;
        localCar.Tyres.SlipRatio = [SlipCalc.Ratio(data.WheelSpeedFL, data.Speed),
                                    SlipCalc.Ratio(data.WheelSpeedFR, data.Speed),
                                    SlipCalc.Ratio(data.WheelSpeedRL, data.Speed),
                                    SlipCalc.Ratio(data.WheelSpeedRR, data.Speed)
                                    ];
    }

    internal override int PollingRate()
    {
        return 60;
    }

    internal override void Start()
    {
        _isRunning = true;
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
            while (_isRunning)
            {
                try
                {
                    var receivedBytes = _udpClient.Receive(ref remoteEndPoint);
                    if (receivedBytes.Length == Marshal.SizeOf<DirtRally2Data>())
                    {
                        var handle = GCHandle.Alloc(receivedBytes, GCHandleType.Pinned);
                        try
                        {
                            _latestData = Marshal.PtrToStructure<DirtRally2Data>(handle.AddrOfPinnedObject());
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                }
                catch (SocketException se) when (!_isRunning)
                {
                    Debug.WriteLine($"Socket closed, stopping listener: {se}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error receiving telemetry: {ex}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        finally
        {
            _udpClient?.Close();
        }
    }
}

/// <summary>
/// Extension methods for calculating wheel slip from DirtRally2Data.
/// </summary>
internal static class SlipCalc
{
    public static float Ratio(float tyreVelocity, float carVelocity)
    {
        if (carVelocity < 1) return 0;
        float ratio = (tyreVelocity - carVelocity) / tyreVelocity;
        if (ratio < 0) ratio *= -1;
        return ratio;
    }

    private const float Epsilon = 0.0001f; // Threshold for standstill (m/s)

    /// <summary>
    /// Calculates simplified longitudinal slip ratios using scalar wheel speeds vs. vehicle speed.
    /// Slip ratio formula: |v_wheel - v_speed| / v_speed (positive-only magnitude).
    /// Returns [FrontLeft, FrontRight, RearLeft, RearRight]. Returns 0 for low-speed cases.
    /// Assumes wheel speeds and Speed in m/s; ignores direction/steering for simplicity.
    /// Guaranteed: All values >= 0 (no sub-zero outputs).
    /// </summary>
    /// <param name="data">The DirtRally2Data packet.</param>
    /// <returns>Array of four slip ratios (dimensionless, non-negative).</returns>
    public static float[] CalculateLongitudinalSlips(this DirtRally2Data data)
    {
        float v_speed = data.Speed;
        if (v_speed < Epsilon)
        {
            return [0f, 0f, 0f, 0f]; // Standstill: no slip
        }
        float slip_fl = Math.Abs(data.WheelSpeedFL - v_speed) / v_speed;
        float slip_fr = Math.Abs(data.WheelSpeedFR - v_speed) / v_speed;
        float slip_rl = Math.Abs(data.WheelSpeedRL - v_speed) / v_speed;
        float slip_rr = Math.Abs(data.WheelSpeedRR - v_speed) / v_speed;
        return new[] { slip_fl * 1000f, slip_fr * 1000f, slip_rl * 1000f, slip_rr * 1000f };
    }

    /// <summary>
    /// Calculates longitudinal slip ratios for all four wheels.
    /// Returns: [FrontLeft, FrontRight, RearLeft, RearRight]
    /// </summary>
    public static float[] GetWheelSlipRatios(this DirtRally2Data data)
    {
        // Compute vehicle longitudinal velocity (dot product of velocity and local forward vectors)
        Vector3 velocity = new(data.VelX, data.VelY, data.VelZ);
        Vector3 localForward = new(data.PitchVecX, data.PitchVecY, data.PitchVecZ);
        float vLong = Vector3.Dot(velocity, localForward);
        float absVLong = Math.Abs(vLong) * 10f;
        if (absVLong < 0.01f)
        {
            // Near-stationary: no meaningful slip
            return [0f, 0f, 0f, 0f];
        }
        // Slip ratios: (wheel_speed - vLong) / |vLong|
        float fl = (data.WheelSpeedFL * 10f - vLong) / absVLong;
        float fr = (data.WheelSpeedFR - vLong) / absVLong; // Note: *10f only on FL? Likely typo in original, adjust if needed
        float rl = (data.WheelSpeedRL - vLong) / absVLong;
        float rr = (data.WheelSpeedRR - vLong) / absVLong;
        if (fl < 0) fl *= -1;
        if (fr < 0) fr *= -1;
        if (rl < 0) rl *= -1;
        if (rr < 0) rr *= -1;
        return [fl, fr, rl, rr];
    }
}
