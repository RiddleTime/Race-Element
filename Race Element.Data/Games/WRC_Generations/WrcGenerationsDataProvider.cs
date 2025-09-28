using System.Numerics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.WRC_Generations.UDP;
using static RaceElement.Data.Games.WRC_Generations.UDP.WrcGenerationsMemoryStructs;

namespace RaceElement.Data.Games.WRC_Generations;

sealed class WrcGenerationsDataProvider : AbstractSimDataProvider
{
    private UdpClient? _udpClient;
    private Task? _listenerTask;
    private bool _isListening = false;

    // Latest packets
    private MotionPacket? _latestMotion;
    private SessionPacket? _latestSession;
    private LapPacket? _latestLap;
    private EventPacket? _latestEvent;
    private ParticipantsPacket? _latestParticipants;
    private CarStatusPacket? _latestCarStatus;
    private CarDamagePacket? _latestCarDamage;
    private TyreSetPacket? _latestTyreSets;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {

        if (!_isListening)
        {
            _isListening = StartListening();
            gameData.IsGamePaused = true;
            return;
        }

        var playerIndex = sessionData.PlayerCarIndex; // Use current or default to 0 if not set

        // Map from MotionPacket
        if (_latestMotion != null)
        {
            var motion = _latestMotion;
            var playerMotion = motion.Value.CarMotionData[playerIndex];

            // Physics
            localCar.Physics.Location = new Vector3(
                playerMotion.WorldPosition[0], // X (forward/longitudinal)
                playerMotion.WorldPosition[1], // Y (lateral/right)
                playerMotion.WorldPosition[2]  // Z (vertical/down)
            );
            localCar.Physics.Acceleration = new Vector3(
                motion.Value.LocalAccelerationPlayer[0], // right
                motion.Value.LocalAccelerationPlayer[1], // forward
                motion.Value.LocalAccelerationPlayer[2]  // down
            );
            localCar.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(
                motion.Value.YawPitchRollPlayer[0], // yaw
                motion.Value.YawPitchRollPlayer[1], // pitch
                motion.Value.YawPitchRollPlayer[2]  // roll
            );
            var speedMs = MathF.Sqrt(
                motion.Value.LocalVelocityPlayer[0] * motion.Value.LocalVelocityPlayer[0] +
                motion.Value.LocalVelocityPlayer[1] * motion.Value.LocalVelocityPlayer[1] +
                motion.Value.LocalVelocityPlayer[2] * motion.Value.LocalVelocityPlayer[2]
            );
            localCar.Physics.Velocity = speedMs * 3.6f; // m/s to km/h

            // Tyres
            localCar.Tyres.Pressure = [motion.Value.TyrePressure[0], motion.Value.TyrePressure[1], motion.Value.TyrePressure[2], motion.Value.TyrePressure[3]];
            localCar.Tyres.SurfaceTemperature = [motion.Value.TyreSurfaceTemp[0], motion.Value.TyreSurfaceTemp[1], motion.Value.TyreSurfaceTemp[2], motion.Value.TyreSurfaceTemp[3]];
            // Core temp as average of inner/middle/outer
            for (int i = 0; i < 4; i++)
            {
                localCar.Tyres.CoreTemperature[i] = (motion.Value.TyreInnerTemp[i] + motion.Value.TyreMiddleTemp[i] + motion.Value.TyreOuterTemp[i]) / 3f;
            }
            localCar.Tyres.SlipAngle = [motion.Value.TyreSlipAnglePlayer[0], motion.Value.TyreSlipAnglePlayer[1], motion.Value.TyreSlipAnglePlayer[2], motion.Value.TyreSlipAnglePlayer[3]];
            localCar.Tyres.SlipRatio = [motion.Value.TyreSlipRatio[0], motion.Value.TyreSlipRatio[1], motion.Value.TyreSlipRatio[2], motion.Value.TyreSlipRatio[3]];
            // Tyre velocity as wheel speed (tangential m/s to km/h)
            for (int i = 0; i < 4; i++)
            {
                localCar.Tyres.Velocity[i] = motion.Value.WheelSpeed[i] * 3.6f;
            }
        }

        // Map from SessionPacket
        if (_latestSession != null)
        {
            var session = _latestSession;
            sessionData.Weather.AirTemperature = session.Value.AirTemperature;
            sessionData.Track.Temperature = session.Value.TrackTemperature;
            // Track name: Map trackId to name (example; extend as needed)
            sessionData.Track.GameName = session.Value.TrackId switch
            {
                0 => "Monte Carlo",
                1 => "Sweden",
                // Add more mappings from WRC tracks
                _ => "Unknown Track"
            };
            sessionData.SessionType = (RaceSessionType)session.Value.SessionType;
            sessionData.SessionTimeLeftSecs = session.Value.TimeLeftInSession;
            // Approximate phase
            sessionData.Phase = session.Value.SessionType switch
            {
                1 or 2 or 3 => SessionPhase.Session, // Practice/Quali/Race
                _ => SessionPhase.PreSession
            };
            // Player index
            sessionData.PlayerCarIndex = session.Value.Header.PlayerCarIndex;
        }

        // Map from LapPacket (Timing and Race)
        if (_latestLap != null)
        {
            var lap = _latestLap;
            var playerLap = lap.Value.LapData[playerIndex];
            localCar.Timing.CurrentLaptimeMS = (int)(playerLap.CurrentLapTime * 1000f);
            localCar.Timing.LapTimeBestMs = playerLap.BestLapTime >= 0 ? (int)(playerLap.BestLapTime * 1000f) : -1;
            localCar.Timing.LapTimeDeltaBestMS = (int)(playerLap.TimeDelta * 1000f);
            localCar.Timing.IsLapValid = playerLap.CurrentLapValid == 1;
            localCar.Timing.HasLapTimeBest = localCar.Timing.LapTimeBestMs != -1;
            localCar.Race.LapsDriven = playerLap.CurrentLapNum;
            // Lap position percentage (approximate from time delta if needed; otherwise default)
            sessionData.LapDeltaToSessionBestLapMs = (float)(playerLap.TimeDelta * 1000f);
        }

        // Map from ParticipantsPacket (Cars and CarModel)
        if (_latestParticipants != null)
        {

            // Set player's car model class
            var playerParticipant = _latestParticipants.Value.Participants.FirstOrDefault(x => x.IsPlayer == 1);
            if (playerParticipant.CarClass != 0)
            {
                localCar.CarModel.CarClass = playerParticipant.CarClass.ToString();
            }
            localCar.CarModel.GameName = "WRC Generations";
            localCar.CarModel.GameId = 1; // Arbitrary ID for WRC Generations
        }

        // Map from CarStatusPacket (Electronics and Engine partial)
        if (_latestCarStatus != null)
        {
            var status = _latestCarStatus;
            var playerStatus = status.Value.CarStatusData[playerIndex];
            localCar.Electronics.TractionControlLevel = playerStatus.TractionControl;
            localCar.Electronics.AbsLevel = playerStatus.AntiLockBrakes;
            localCar.Electronics.BrakeBias = playerStatus.RsbBrakeBias;
            // Engine fuel (percent to liters; assume max 50L for rally car, adjust)
            const float maxFuelLiters = 50f;
            localCar.Engine.FuelLiters = (playerStatus.FuelLevel / 100f) * maxFuelLiters;
            localCar.Engine.MaxFuelLiters = maxFuelLiters;
            localCar.Engine.FuelEstimatedLaps = playerStatus.FuelRemainingLaps;
        }

        // Brakes: No direct data; leave default or approximate from acceleration if needed

        // Other missing: Inputs (throttle, brake, gear, etc.), full Engine (RPM, etc.), Race position, etc. - left as defaults
    }

    internal override int PollingRate() => 200;

    internal override void Start()
    {
        _isListening = StartListening();
    }

    private bool StartListening()
    {
        if (!GameManager.IsGameRunning) return false;

        _listenerTask = Task.Run(async () =>
        {
            _udpClient = new UdpClient(20777);
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            while (GameManager.IsGameRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    ParsePacket(result.Buffer);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            _udpClient?.Close();
            _isListening = false;
        });

        return true;
    }

    internal override void Stop()
    {
        _udpClient?.Close();
        if (_listenerTask != null && !_listenerTask.IsCompleted)
            _listenerTask?.Dispose();
    }

    private void ParsePacket(byte[] data)
    {
        if (data.Length < 24) return;

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            var headerPtr = handle.AddrOfPinnedObject();
            var header = (WrcGenerationsMemoryStructs.PacketHeader)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.PacketHeader));
            if (header.PacketFormat != 2018) return;

            switch (header.PacketId)
            {
                case 0: // Motion
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.MotionPacket>())
                    {
                        _latestMotion = (WrcGenerationsMemoryStructs.MotionPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.MotionPacket));
                    }
                    break;
                case 1: // Session
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.SessionPacket>())
                    {
                        _latestSession = (WrcGenerationsMemoryStructs.SessionPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.SessionPacket));
                    }
                    break;
                case 2: // Lap
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.LapPacket>())
                    {
                        _latestLap = (WrcGenerationsMemoryStructs.LapPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.LapPacket));
                    }
                    break;
                case 3: // Event
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.EventPacket>())
                    {
                        _latestEvent = (WrcGenerationsMemoryStructs.EventPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.EventPacket));
                    }
                    break;
                case 4: // Participants
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.ParticipantsPacket>())
                    {
                        _latestParticipants = (WrcGenerationsMemoryStructs.ParticipantsPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.ParticipantsPacket));
                    }
                    break;
                case 5: // CarStatus
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.CarStatusPacket>())
                    {
                        _latestCarStatus = (WrcGenerationsMemoryStructs.CarStatusPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.CarStatusPacket));
                    }
                    break;
                case 6: // CarDamage
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.CarDamagePacket>())
                    {
                        _latestCarDamage = (WrcGenerationsMemoryStructs.CarDamagePacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.CarDamagePacket));
                    }
                    break;
                case 7: // TyreSets
                    if (data.Length >= Marshal.SizeOf<WrcGenerationsMemoryStructs.TyreSetPacket>())
                    {
                        _latestTyreSets = (WrcGenerationsMemoryStructs.TyreSetPacket)Marshal.PtrToStructure(headerPtr, typeof(WrcGenerationsMemoryStructs.TyreSetPacket));
                    }
                    break;
            }
        }
        finally
        {
            handle.Free();
        }
    }

    public override List<string> GetCarClasses() => ["Rally Car"]; // Example; extend with actual classes

    public override bool HasTelemetry() => true;
}