using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.ProjectMotorRacing.ProjectMotorRacingUDP;
using System.Numerics;

namespace RaceElement.Data.Games.ProjectMotorRacing;
internal sealed class ProjectMotorRacingDataProvider : AbstractSimDataProvider
{
    private readonly DataStore _dataStore = new DataStore();
    private UDPThread _udpThread;

    private Vector3 _lastGForces = new();
    private int _sameGForceCount = 0;

    internal override int PollingRate() => 100;

    private static void ResetData(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        localCar = new();
        sessionData = new();
        gameData = new();
    }

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        int playerVehicleId = -1;
        UDPParticipantRaceState? participant = null;
        foreach (var item in _dataStore.GetLeaderboard())
            if (item.m_isPlayer)
            {
                participant = item;
                playerVehicleId = item.m_vehicleId;
                break;
            }
        if (playerVehicleId == -1 || participant == null)
        {
            ResetData(ref localCar, ref sessionData, ref gameData);
            gameData.IsGamePaused = true;
            return;
        }

        var telemetry = _dataStore.GetTelemetryForVehicle(playerVehicleId);
        if (telemetry == null)
        {
            ResetData(ref localCar, ref sessionData, ref gameData);
            gameData.IsGamePaused = true;
            return;
        }

        gameData.IsRunning = true;


        // car 
        localCar.CarModel.GameId = participant.m_vehicleId;
        localCar.CarModel.GameName = participant.m_vehicleName;

        // Inputs
        localCar.Inputs.Throttle = telemetry.m_input.m_accelerator;
        localCar.Inputs.Brake = telemetry.m_input.m_brake;
        localCar.Inputs.Clutch = telemetry.m_input.m_clutch;
        localCar.Inputs.Steering = telemetry.m_input.m_steering;
        localCar.Inputs.MaxSteeringAngle = telemetry.m_general.m_steeringWheelAngle;
        localCar.Inputs.Gear = telemetry.m_input.m_gear + 1;

        // Physica
        localCar.Physics.Velocity = telemetry.m_general.m_estLinearSpeed * 3.6f;
        localCar.Physics.Acceleration = new(-telemetry.m_chassis.m_accelerationLS.z / 9.80665f, telemetry.m_chassis.m_accelerationLS.y / 9.80665f, telemetry.m_chassis.m_accelerationLS.x / 9.80665f);
        localCar.Physics.Rotation = new(telemetry.m_chassis.m_quat.x, telemetry.m_chassis.m_quat.y, telemetry.m_chassis.m_quat.z, telemetry.m_chassis.m_quat.w);

        // Engine
        localCar.Engine.IsRunning = telemetry.m_drivetrain.m_engineRunning;
        localCar.Engine.MaxRpm = (int)telemetry.m_drivetrain.m_gears.Last().m_upshiftRPM;
        localCar.Engine.Rpm = (int)telemetry.m_drivetrain.m_engineRPM;
        localCar.Engine.IsPitLimiterOn = telemetry.m_drivetrain.m_speedLimiterActive;
        localCar.Engine.FuelLiters = telemetry.m_drivetrain.m_fuelRemaining;
        localCar.Engine.MaxFuelLiters = telemetry.m_constant.m_fuelCapacity;

        // Electronics
        localCar.Electronics.TractionControlActivation = telemetry.m_drivetrain.m_tractionControlActive ? 1 : 0;
        localCar.Electronics.AbsActivation = telemetry.m_general.m_absActive ? 1 : 0;

        // Tyres
        localCar.Tyres.SlipRatio = [
            telemetry.m_wheels[0].m_slipRatio,
            telemetry.m_wheels[1].m_slipRatio,
            telemetry.m_wheels[2].m_slipRatio,
            telemetry.m_wheels[3].m_slipRatio,
        ];
        for (int i = 0; i < 4; i++)
            if (localCar.Tyres.SlipRatio[i] < 0)
                localCar.Tyres.SlipRatio[i] *= -1;

        // TODO
        localCar.Tyres.SlipAngle = [
            telemetry.m_wheels[0].m_slipAngle,
            telemetry.m_wheels[1].m_slipAngle,
            telemetry.m_wheels[2].m_slipAngle,
            telemetry.m_wheels[3].m_slipAngle,
        ];

        // handle game pausing by checking acceleration
        if (localCar.Physics.Acceleration == _lastGForces)
        {
            if (_sameGForceCount < 60)
                _sameGForceCount++;
            else
                gameData.IsGamePaused = true;
        }
        else
        {
            _sameGForceCount = 0;
            gameData.IsGamePaused = false;
        }
        _lastGForces = localCar.Physics.Acceleration;

    }

    internal override void Start()
    {
        _udpThread?.Shutdown(this, new());

        _udpThread = new(_dataStore, []);
        _udpThread?.StartThread();
    }

    internal override void Stop()
    {
        _udpThread?.Shutdown(this, new());
        _dataStore?.Clear();
    }


    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;
}
