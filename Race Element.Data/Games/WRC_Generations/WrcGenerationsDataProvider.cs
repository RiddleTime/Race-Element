using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.WRC_Generations.UDP;

namespace RaceElement.Data.Games.WRC_Generations;
internal sealed class WrcGenerationsDataProvider : AbstractSimDataProvider
{
    private Thread? _listenerThread;
    private UdpClient? _udpClient;
    private WRCGenData _latestData;
    private bool _isRunning;
    private const int Port = 20777;

    public override List<string> GetCarClasses() => ["WRC", "WRC2", "Legends"];

    public override bool HasTelemetry() => true;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        var data = _latestData;
        if (data.PositionZ == 0)
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
        localCar.Physics.Location = new(data.PositionX, data.PositionY, data.PositionZ);

        localCar.Physics.Velocity = data.Speed * 3.6f;

        localCar.Physics.Acceleration = new(data.LateralGForce, data.LongitudinalGForce, 0);

        localCar.Inputs.Throttle = data.Throttle;
        localCar.Inputs.Brake = data.Brake;
        localCar.Inputs.Clutch = data.Clutch;
        localCar.Inputs.Steering = data.SteerAngle;
        localCar.Inputs.Gear = (sbyte)data.CurrentGear; // Assuming Gear is sbyte
        localCar.Engine.Rpm = (int)(data.EngineRpms * 10f);
        localCar.Engine.MaxRpm = (int)(data.MaxRpms * 10f);

        localCar.Engine.IsRunning = localCar.Engine.Rpm > 0;

        //// Wheel speeds (assuming order FL, FR, RL, RR)
        //localCar.WheelSpeedFrontLeft = data.WheelSpeedFrontLeft;
        //localCar.WheelSpeedFrontRight = data.WheelSpeedFrontRight;
        //localCar.WheelSpeedRearLeft = data.WheelSpeedRearLeft;
        //localCar.WheelSpeedRearRight = data.WheelSpeedRearRight;

        //// Suspension positions
        //localCar.SuspensionTravelFrontLeft = data.SuspensionPositionFrontLeft;
        //localCar.SuspensionTravelFrontRight = data.SuspensionPositionFrontRight;
        //localCar.SuspensionTravelRearLeft = data.SuspensionPositionRearLeft;
        //localCar.SuspensionTravelRearRight = data.SuspensionPositionRearRight;

        //// Suspension velocities
        //localCar.SuspensionVelocityFrontLeft = data.SuspensionVelocityFrontLeft;
        //localCar.SuspensionVelocityFrontRight = data.SuspensionVelocityFrontRight;
        //localCar.SuspensionVelocityRearLeft = data.SuspensionVelocityRearLeft;
        //localCar.SuspensionVelocityRearRight = data.SuspensionVelocityRearRight;


        //localCar.CurrentLapTime = data.CurrentLapTime;
        //localCar.CurrentLapDistance = data.CurrentLapDistance;
        //localCar.CurrentLapNumber = (int)data.CurrentLap;
        //localCar.LastLapTime = data.LastLapTime;

        //// Brake temperatures (assuming order FL, FR, RL, RR)
        //localCar.BrakeTemperatureFrontLeft = data.BrakeTemp0;
        //localCar.BrakeTemperatureFrontRight = data.BrakeTemp1;
        //localCar.BrakeTemperatureRearLeft = data.BrakeTemp2;
        //localCar.BrakeTemperatureRearRight = data.BrakeTemp3;

        //// Wheel pressures (assuming order FL, FR, RL, RR)
        //localCar.TyrePressureFrontLeft = data.WheelPressure0;
        //localCar.TyrePressureFrontRight = data.WheelPressure1;
        //localCar.TyrePressureRearLeft = data.WheelPressure2;
        //localCar.TyrePressureRearRight = data.WheelPressure3;


        // Map to SessionData
        //sessionData.SessionType = (SessionType)(int)data.SessionType; // Assuming SessionType enum matches 0-3
        //sessionData.LapCount = (int)data.TotalLaps;
        //sessionData.CurrentSectorIndex = (int)data.CurrentSector;
        //sessionData.Sector1Time = data.Sector1Time;
        //sessionData.Sector2Time = data.Sector2Time;
        //sessionData.TrackName = $"Track {data.TrackNumber}"; // Placeholder

        // Map to GameData
        //gameData.PlayerCarIndex = (int)data.CarPosition - 1; // Assuming 1-based position
        //gameData.TrackLength = data.TrackSize; // If applicable

        // Note: Additional mappings can be added based on exact field availability in LocalCarData/SessionData/GameData
    }

    internal override int PollingRate()
    {
        return 60; // 60 Hz
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
                    if (receivedBytes.Length == Marshal.SizeOf<WRCGenData>())
                    {
                        var handle = GCHandle.Alloc(receivedBytes, GCHandleType.Pinned);
                        try
                        {
                            _latestData = Marshal.PtrToStructure<WRCGenData>(handle.AddrOfPinnedObject());
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                }
                catch (SocketException) when (!_isRunning)
                {
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            _udpClient?.Close();
        }
    }
}
