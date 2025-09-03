using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.Forza.ForzaUDP;
using System.Net.Sockets;
using System.Numerics;

namespace RaceElement.Data.Games.Forza
{
    public class ForzaDataProvider : AbstractSimDataProvider
    {
        private const int FORZA_DATA_OUT_PORT = 5300;
        private UdpClient _udpClient;
        private Task _receiverTask;
        private bool _isRunning;


        // Store latest data for Update method (if required by base class)
        private Lock _lock = new();
        private LocalCarData _localCar = new();
        private SessionData _sessionData = new();
        private GameData _gameData = new();

        internal override void Start()
        {
            _isRunning = true;
            _udpClient = new UdpClient(FORZA_DATA_OUT_PORT);
            _receiverTask = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    var result = await _udpClient.ReceiveAsync();
                    var packet = result.Buffer;
                    if (ForzaMotorsportsData.IsValidFormat(packet))
                    {
                        UpdateFromPacket(packet);
                    }
                }
            });
        }

        internal override void Stop()
        {
            _isRunning = false;
            _udpClient?.Close();
            _udpClient?.Dispose();
            _receiverTask?.Wait(1000); // Allow graceful shutdown
        }

        internal override int PollingRate() => 60;

        public override List<string> GetCarClasses()
        {
            // Map byte CarClass to string representations (example mapping)
            return new List<string> { "D", "C", "B", "A", "S", "R", "X" }; // Adjust based on actual Forza class mapping
        }

        public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            localCar = _localCar;
            sessionData = _sessionData;
            gameData = _gameData;
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
                sled = ForzaMotorsportsData.GetSledData(packet);
            }
            else if (ForzaMotorsportsData.IsDashFormat(packet))
            {
                sled = ForzaMotorsportsData.GetSledData(packet);
                dash = ForzaMotorsportsData.GetDashData(packet);
            }
            else if (ForzaMotorsportsData.IsFH4Format(packet))
            {
                var fh4 = ForzaMotorsportsData.GetFH4Data(packet);
                sled = fh4.Sled;
                dash = fh4.Dash;
            }
            else if (ForzaMotorsportsData.IsFM8Format(packet))
            {
                var fm8 = ForzaMotorsportsData.GetFM8Data(packet);
                sled = fm8.Sled;
                dash = fm8.Dash;
            }
            else
            {
                return; // Invalid packet
            }

            // Map SledData to LocalCarData
            localCar.Engine.Rpm = (int)sled.CurrentEngineRpm;
            localCar.Engine.IsRunning = localCar.Engine.Rpm > 0;
            localCar.Engine.MaxRpm = (int)sled.EngineMaxRpm;
            localCar.Engine.FuelLiters = dash.Fuel; // Only if dash data available
            localCar.Physics.Acceleration = new Vector3(sled.AccelerationX, sled.AccelerationY, sled.AccelerationZ);
            localCar.Physics.Velocity = (float)Math.Sqrt(sled.VelocityX * sled.VelocityX + sled.VelocityY * sled.VelocityY + sled.VelocityZ * sled.VelocityZ) * 3.6f; // Convert m/s to km/h
            localCar.Physics.Location = new Vector3(dash.PositionX, dash.PositionY, dash.PositionZ); // Only if dash data
            localCar.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(sled.Yaw, sled.Pitch, sled.Roll);
            localCar.Tyres.SlipAngle = [sled.TireSlipAngleFl, sled.TireSlipAngleFr, sled.TireSlipAngleRl, sled.TireSlipAngleRr];
            localCar.Tyres.SlipRatio = [sled.TireSlipRatioFl, sled.TireSlipRatioFr, sled.TireSlipRatioRl, sled.TireSlipRatioRr];
            localCar.Tyres.CoreTemperature = [dash.TireTempFl, dash.TireTempFr, dash.TireTempRl, dash.TireTempRr]; // Only if dash data
            localCar.Tyres.Velocity = [sled.WheelRotationSpeedFl, sled.WheelRotationSpeedFr, sled.WheelRotationSpeedRl, sled.WheelRotationSpeedRr];
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
            localCar.Inputs.Throttle = dash.Accelerator / 255f; // Normalize 0-255 to 0-1
            localCar.Inputs.Brake = dash.Brake / 255f;
            localCar.Inputs.Clutch = dash.Clutch / 255f;
            localCar.Inputs.HandBrake = dash.Handbrake / 255f;
            localCar.Inputs.Steering = dash.Steer / 127f; // Normalize -127 to 127 to -1 to 1
            localCar.Inputs.Gear = dash.Gear;
            localCar.Race.LapsDriven = dash.Lap;
            localCar.Race.GlobalPosition = dash.RacePosition;
            localCar.Timing.CurrentLaptimeMS = (int)(dash.CurrentLapTime * 1000);
            localCar.Timing.LapTimeBestMs = (int)(dash.BestLapTime * 1000);
            localCar.Timing.HasLapTimeBest = dash.BestLapTime > 0;

            // Map to GameData
            gameData.Name = ForzaMotorsportsData.IsFH4Format(packet) ? "Forza Horizon 4/5" : ForzaMotorsportsData.IsFM8Format(packet) ? "Forza Motorsport 8" : "Forza Motorsport 7";


            // Update shared data (thread-safe access may be needed depending on AbstractSimDataProvider)
            lock (_lock)
            {
                _localCar = localCar;
                _sessionData = sessionData;
                _gameData = gameData;
            }
        }

        public override bool HasTelemetry() => false;
    }
}