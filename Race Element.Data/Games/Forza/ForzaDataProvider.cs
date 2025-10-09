using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.Forza.ForzaUDP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace RaceElement.Data.Games.Forza;

public sealed class ForzaDataProvider(Game Game) : AbstractSimDataProvider
{
    private const int FORZA_DATA_OUT_PORT = 5300;
    private UdpClient _udpClient;
    private Task _receiverTask;
    private bool _isRunning;

    private Lock _lock = new();
    private LocalCarData _localCar = new();
    private SessionData _sessionData = new();
    private GameData _gameData = new();

    internal override void Start()
    {
        _isRunning = true;
        try
        {
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, FORZA_DATA_OUT_PORT));
            Debug.WriteLine($"Listening for Forza telemetry on port {FORZA_DATA_OUT_PORT}");
        }
        catch (SocketException ex)
        {
            Debug.WriteLine($"Failed to bind to port {FORZA_DATA_OUT_PORT}: {ex.Message}");
            throw;
        }

        _receiverTask = Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    var packet = result.Buffer;
                    //Debug.WriteLine($"Received packet of length {packet.Length}");
                    if (ForzaMotorsportsData.IsValidFormat(packet))
                    {
                        UpdateFromPacket(packet);
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid packet format: length {packet.Length}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error receiving UDP packet: {ex.Message}");
                }
            }
        });
    }

    internal override void Stop()
    {
        _isRunning = false;
        _udpClient?.Close();
        _udpClient?.Dispose();
        _receiverTask?.Wait(1000);
    }

    internal override int PollingRate() => 200;

    public override List<string> GetCarClasses()
    {
        return new List<string> { "D", "C", "B", "A", "S", "R", "X" };
    }

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        lock (_lock)
        {
            localCar = _localCar;
            sessionData = _sessionData;
            gameData = _gameData;
        }
    }

    private void UpdateFromPacket(byte[] packet)
    {
        var localCar = new LocalCarData();
        var sessionData = new SessionData();
        var gameData = new GameData();

        ForzaMotorsportsData.SledData sled;
        ForzaMotorsportsData.DashData dash = default;

        if (ForzaMotorsportsData.IsSledFormat(packet))
        {
            //Debug.WriteLine("Processing sled-only packet (232 bytes)");
            sled = ForzaMotorsportsData.GetSledData(packet);
        }
        else if (ForzaMotorsportsData.IsDashFormat(packet))
        {
            //Debug.WriteLine("Processing FM7 dash packet (311 bytes)");
            sled = ForzaMotorsportsData.GetSledData(packet);
            dash = ForzaMotorsportsData.GetDashData(packet);
        }
        else if (ForzaMotorsportsData.IsFH4Format(packet))
        {
            //Debug.WriteLine("Processing FH4/FH5 packet (324 bytes)");
            var (sledData, dashData) = ForzaMotorsportsData.GetFH4Data(packet);
            sled = sledData;
            dash = dashData;
        }
        else if (ForzaMotorsportsData.IsFM8Format(packet))
        {
            //Debug.WriteLine("Processing FM8 packet (331 bytes)");
            var (sledData, dashData) = ForzaMotorsportsData.GetFM8Data(packet);
            sled = sledData;
            dash = dashData;
        }
        else
        {
            //Debug.WriteLine("Invalid packet");
            return;
        }


        // Map SledData to LocalCarData (unchanged as per request)
        localCar.Engine.Rpm = (int)sled.CurrentEngineRpm;
        localCar.Engine.IsRunning = dash.Fuel > 0;
        localCar.Engine.MaxRpm = (int)sled.EngineMaxRpm;
        localCar.Engine.FuelLiters = dash.Fuel;
        localCar.Physics.Acceleration = new Vector3(-sled.AccelerationX / 9.80665f, sled.AccelerationY / 9.80665f, sled.AccelerationZ / 9.80665f);
        localCar.Physics.Velocity = (float)Math.Sqrt(sled.VelocityX * sled.VelocityX + sled.VelocityY * sled.VelocityY + sled.VelocityZ * sled.VelocityZ) * 3.6f;
        localCar.Physics.Location = new Vector3(dash.PositionX, dash.PositionY, dash.PositionZ);
        localCar.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(sled.Yaw, sled.Pitch, sled.Roll);
        localCar.Tyres.SlipAngle = [sled.TireSlipAngleFr / -2f, sled.TireSlipAngleFl / -2f, sled.TireSlipAngleRr / -2f, sled.TireSlipAngleRl / -2f];
        localCar.Tyres.SlipRatio = [NegateIfNegative(sled.TireCombinedSlipFr), NegateIfNegative(sled.TireCombinedSlipFl), NegateIfNegative(sled.TireCombinedSlipRr), NegateIfNegative(sled.TireCombinedSlipRl)];
        localCar.Tyres.CoreTemperature = [dash.TireTempFr, dash.TireTempFl, dash.TireTempRr, dash.TireTempRl];
        localCar.Tyres.Velocity = [sled.WheelRotationSpeedFr, sled.WheelRotationSpeedFl, sled.WheelRotationSpeedRr, sled.WheelRotationSpeedRl];
        localCar.CarModel.GameId = sled.CarOrdinal;
        localCar.CarModel.CarClass = sled.CarClass switch
        {
            0 => "D",
            1 => "C",
            2 => "B",
            3 => "A",
            4 => "S",
            5 => "R",
            6 => "X",
            _ => "Unknown"
        };
        localCar.Inputs.Throttle = dash.Accelerator / 255f;
        localCar.Inputs.Brake = dash.Brake / 255f;
        localCar.Inputs.Clutch = dash.Clutch / 255f;
        localCar.Inputs.HandBrake = dash.Handbrake / 255f;
        localCar.Inputs.Steering = dash.Steer / 127f;
        localCar.Inputs.Gear = (int)dash.Gear > 0 ? dash.Gear + 1 : dash.Gear;
        localCar.Race.LapsDriven = (int)dash.Lap;
        localCar.Race.GlobalPosition = (int)dash.RacePosition;
        localCar.Timing.CurrentLaptimeMS = (int)(dash.CurrentLapTime * 1000f);
        localCar.Timing.LapTimeBestMs = (int)(dash.BestLapTime * 1000f);
        localCar.Timing.HasLapTimeBest = dash.BestLapTime > 0;

        // Map to GameData
        gameData.Name = ForzaMotorsportsData.IsFH4Format(packet) ? "Forza Horizon 4/5" : ForzaMotorsportsData.IsFM8Format(packet) ? "Forza Motorsport 8" : "Forza Motorsport 7";

        // Update shared data
        lock (_lock)
        {
            _localCar = localCar;
            _sessionData = sessionData;
            _gameData = gameData;
        }
    }

    private static float NegateIfNegative(float value) => (value < 0 ? -value : value);

    public override bool HasTelemetry() => true; // Changed to true since telemetry is processed
}
